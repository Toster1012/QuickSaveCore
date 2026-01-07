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
                await using var _fileStream = new FileStream(configuration.Path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);

                if (configuration.UseGzipCompression)
                {
                    foreach (var serializeInstruction in configuration.SerializeInstructions)
                    {
                        if (serializeInstruction.SerializeType == typeof(T))
                        {
                            await GZipArchiver.CompressAsync(_fileStream, serializeInstruction.Write(data), configuration.Path, (stream, serializeData)
                                => Serialize<T>(stream, serializeData));

                            return File.Exists(configuration.Path);
                        }
                    }

                    await GZipArchiver.CompressAsync(_fileStream, data, configuration.Path, (stream, serializeData)
                        => Serialize<T>(stream, serializeData));

                    return File.Exists(configuration.Path);
                }
                else
                {
                    foreach (var serializeInstruction in configuration.SerializeInstructions)
                    {
                        if (serializeInstruction.SerializeType == typeof(T))
                        {
                            await Serialize<T>(_fileStream, serializeInstruction.Write(data));
                            return File.Exists(configuration.Path);
                        }
                    }

                    await Serialize<T>(_fileStream, data);
                    return File.Exists(configuration.Path);
                }
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

                await using var _fileStream = new FileStream(configuration.Path, FileMode.Open, FileAccess.Read, FileShare.None);

                if (configuration.UseGzipCompression)
                {
                    foreach (var serializeInstruction in configuration.SerializeInstructions)
                    {
                        if (serializeInstruction.SerializeType == typeof(T))
                        {
                            return await GZipArchiver.DecompressAsync(_fileStream, configuration.Path, (stream)
                                => Deserialize<T>(stream, serializeInstruction));
                        }
                    }

                    return await GZipArchiver.DecompressAsync(_fileStream, configuration.Path, (stream)
                                => Deserialize<T>(stream));
                }
                else
                {
                    foreach (var serializeInstruction in configuration.SerializeInstructions)
                    {
                        if (serializeInstruction.SerializeType == typeof(T))
                        {
                            return await Deserialize<T>(_fileStream, serializeInstruction);
                        }
                    }

                    return await Deserialize<T>(_fileStream);
                }
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException($"Failed to serialize data to {configuration.Path}", exception);
            }
        }

        private async Task Serialize<T>(Stream stream, object serializeObject)
        {
            if (serializeObject == null)
                return;

            QSValue _qSValue = new QSValue(serializeObject, typeof(T));
            MessagePackSerializer.Serialize(stream, _qSValue, _options);
            await stream.FlushAsync();
        }

        private async Task<T> Deserialize<T>(Stream stream, SerializeInstruction? serializeInstruction = null)
        {
            QSValue _qSValue = MessagePackSerializer.Deserialize<QSValue>(stream, _options);

            try
            {
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
