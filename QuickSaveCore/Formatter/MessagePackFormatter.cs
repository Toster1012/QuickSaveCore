using MessagePack;
using QS.Compression;
using QS.Convert;
using QS.Core;

namespace QS.Serialization
{
    public sealed class MessagePackFormatter : IFormatter
    {
        private readonly MessagePackSerializerOptions _options = MessagePackSerializer.Typeless.DefaultOptions;

        public bool Serialize<T>(string saveKey, T data, Configuration configuration, SerializeOption option)
        {
            return SerializeAsync(saveKey, data, configuration, option).Result;
        }

        public T Deserialize<T>(string saveKey, Configuration configuration, SerializeOption option)
        {
            return DeserializeAsync<T>(saveKey, configuration, option).Result;
        }

        public async Task<bool> SerializeAsync<T>(string saveKey, T data, Configuration configuration, SerializeOption option)
        {
            if (!configuration.Paths.TryGetValue(saveKey, out string? path))
                return false;

            string? _directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(_directory) && !Directory.Exists(_directory))
            {
                if (configuration.CreateDirectoryIfNotExist)
                {
                    Directory.CreateDirectory(_directory);
                }
                else
                {
                    throw new Exception("Directory not exist");
                }
            }

            try
            {
                await using var _fileStream = new FileStream(path, FileMode.Create, FileAccess.Write,
                     FileShare.Write, bufferSize: 4096, useAsync: true);

                foreach (var customTypeConverter in option.CustomTypeConverters)
                {
                    if (customTypeConverter.SerializeType == typeof(T))
                    {
                        if (configuration.UseGzipCompression)
                            return await Serialize(GZipArchiver.Compress(_fileStream), customTypeConverter.Write(data));
                        else
                            return await Serialize(_fileStream, customTypeConverter.Write(data));   
                    }
                }

                if (configuration.UseGzipCompression)
                    return await Serialize(GZipArchiver.Compress(_fileStream), data);

                return await Serialize(_fileStream, data);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException($"Failed to serialize data to {path}", exception);
            }
        }

        public async Task<T> DeserializeAsync<T>(string saveKey, Configuration configuration, SerializeOption option)
        {
            if (!configuration.Paths.TryGetValue(saveKey, out string? path))
                return default;

            try
            {
                if (!File.Exists(path))
                    throw new Exception($"File not foun from path: {path}");

                await using var _fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, 
                    FileShare.Read, bufferSize: 4096, useAsync: true);

                foreach (var serializeInstruction in option.CustomTypeConverters)
                {
                    if (serializeInstruction.SerializeType == typeof(T))
                    {
                        if (configuration.UseGzipCompression)
                            return await Deserialize<T>(GZipArchiver.Decompress(_fileStream), serializeInstruction);

                        return await Deserialize<T>(_fileStream, serializeInstruction);
                    }
                }

                if (configuration.UseGzipCompression)
                    return await Deserialize<T>(GZipArchiver.Decompress(_fileStream));

                return await Deserialize<T>(_fileStream);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException($"Failed to serialize data to {path}", exception);
            }
        }

        private async Task<bool> Serialize<T>(Stream stream, T serializeObject)
        {
            if (serializeObject == null) 
                return false;

            try
            {
                QSValue _qSValue = new QSValue(serializeObject, typeof(T));
                await MessagePackSerializer.SerializeAsync(stream, _qSValue, _options);
                await stream.FlushAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task<T> Deserialize<T>(Stream stream, CustomTypeConverter? customTypeConverter = null)
        {
            try
            {
                QSValue _qSValue = MessagePackSerializer.Deserialize<QSValue>(stream, _options);

                if (customTypeConverter != null)
                    return (T)customTypeConverter.Read(_qSValue.Value);

                return (T)_qSValue.Value;
            }
            finally
            {
                await stream.FlushAsync();
            }
        }
    }
}
