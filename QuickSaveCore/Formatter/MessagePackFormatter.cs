using MessagePack;

namespace QuickSave
{
    public sealed class MessagePackFormatter : IFormatter
    {
        private readonly MessagePackSerializerOptions _options = MessagePackSerializer.Typeless.DefaultOptions;

        public bool Serialize<T>(T data, QuickSaveConfiguration configuration)
        {
            return SerializeAsync(data, configuration).Result;
        }

        public T Deserialize<T>(QuickSaveConfiguration configuration)
        {
            return DeserializeAsync<T>(configuration).Result;
        }

        public async Task<bool> SerializeAsync<T>(T data, QuickSaveConfiguration configuration)
        {
            string? _directory = Path.GetDirectoryName(configuration.Path);
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
                await using var _fileStream = new FileStream(configuration.Path, FileMode.Create, FileAccess.Write,
                     FileShare.Write, bufferSize: 4096, useAsync: true);

                foreach (var serializeInstruction in configuration.SerializeInstructions)
                {
                    if (serializeInstruction.SerializeType == typeof(T))
                    {
                        if (configuration.UseGzipCompression)
                            return await Serialize(GZipArchiver.Compress(_fileStream), serializeInstruction.Write(data));
                        else
                            return await Serialize(_fileStream, serializeInstruction.Write(data));   
                    }
                }

                if (configuration.UseGzipCompression)
                    return await Serialize(GZipArchiver.Compress(_fileStream), data);

                return await Serialize(_fileStream, data);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException($"Failed to serialize data to {configuration.Path}", exception);
            }
        }

        public async Task<T> DeserializeAsync<T>(QuickSaveConfiguration configuration)
        {
            try
            {
                if (!File.Exists(configuration.Path))
                    throw new Exception($"File not foun from path: {configuration.Path}");

                await using var _fileStream = new FileStream(configuration.Path, FileMode.Open, FileAccess.Read, 
                    FileShare.Read, bufferSize: 4096, useAsync: true);

                foreach (var serializeInstruction in configuration.SerializeInstructions)
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
                throw new InvalidOperationException($"Failed to serialize data to {configuration.Path}", exception);
            }
        }

        private async Task<bool> Serialize<T>(Stream stream, T serializeObject)
        {
            if (serializeObject == null)
                return false;

            long _startPosition = stream.Position;
            QSValue _qSValue = new QSValue(serializeObject, typeof(T));

            MessagePackSerializer.Serialize(stream, _qSValue, _options);
            await stream.FlushAsync();

            return (stream.Position - _startPosition) > 0;
        }

        private async Task<T> Deserialize<T>(Stream stream, SerializeInstruction? serializeInstruction = null)
        {
            try
            {
                QSValue _qSValue = MessagePackSerializer.Deserialize<QSValue>(stream, _options);

                if (serializeInstruction != null)
                    return (T)serializeInstruction.Read(_qSValue.Value);

                return (T)_qSValue.Value;
            }
            finally
            {
                await stream.FlushAsync();
            }
        }
    }
}
