using QS.Compression;
using QS.Convert;
using QS.Core;
using System;
using System.IO;
using System.Threading.Tasks;

namespace QS.Serialization
{
    public abstract class Formatter : IFormatter
    {
        public bool Serialize<T>(string saveKey, T data, Configuration configuration, SerializeOption option)
        {
            return SerializeAsync(saveKey, data, configuration, option).GetAwaiter().GetResult();
        }

        public T? Deserialize<T>(string saveKey, Configuration configuration, SerializeOption option)
        {
            return DeserializeAsync<T>(saveKey, configuration, option).GetAwaiter().GetResult();
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
                     FileShare.Read, bufferSize: 4096, useAsync: true);

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

        public async Task<T?> DeserializeAsync<T>(string saveKey, Configuration configuration, SerializeOption option)
        {
            if (!configuration.Paths.TryGetValue(saveKey, out string? path))
                return default;

            try
            {
                if (!File.Exists(path))
                    throw new Exception($"File not foun from path: {path}");

                await using var _fileStream = new FileStream(path, FileMode.Open, FileAccess.Read,
                    FileShare.Read, bufferSize: 4096, useAsync: true);

                foreach (var customConverter in option.CustomTypeConverters)
                {
                    if (customConverter.SerializeType == typeof(T))
                    {
                        if (configuration.UseGzipCompression)
                            return await Deserialize<T>(GZipArchiver.Decompress(_fileStream), customConverter);

                        return await Deserialize<T>(_fileStream, customConverter);
                    }
                }

                if (configuration.UseGzipCompression)
                    return await Deserialize<T>(GZipArchiver.Decompress(_fileStream));

                return await Deserialize<T>(_fileStream);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException($"Failed to deserialize data to {path}", exception);
            }
        }

        protected abstract Task<bool> Serialize<T>(Stream stream, T serializeObject);

        protected abstract Task<T?> Deserialize<T>(Stream stream, CustomTypeConverter? customTypeConverter = null);
    }
}
