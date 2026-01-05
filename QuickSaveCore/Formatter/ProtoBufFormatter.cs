using ProtoBuf;
using System.IO;
using System.IO.Compression;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace QuickSave.Format
{
    public class ProtoBufFormatter : IFormatter
    {
        public async Task<bool> SerializeAsync(object data, QuickSaveConfiguration configuration)
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
                if (!File.Exists(configuration.Path))
                    throw new Exception($"File not foun from path: {configuration.Path}");

                await using var _fileStream = new FileStream(configuration.Path, FileMode.Open, FileAccess.Write);

                if (configuration.UseGzipCompression)
                {
                    await CompressAsync(_fileStream, data, configuration.Path);
                }
                else
                {
                    Serializer.Serialize(_fileStream, data);
                    await _fileStream.FlushAsync();
                }

                return File.Exists(configuration.Path);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to serialize data to {configuration.Path}", ex);
            }
        }

        public async Task<T> DeserializeAsync<T>(QuickSaveConfiguration configuration)
        {
            try
            {
                if (!File.Exists(configuration.Path))
                    throw new Exception($"File not foun from path: {configuration.Path}");

                await using var _fileStream = new FileStream(configuration.Path, FileMode.Open, FileAccess.Read);

                if (configuration.UseGzipCompression)
                {
                    return await DecompressAsync<T>(_fileStream, configuration.Path);
                }
                else
                {
                    T _data = Serializer.Deserialize<T>(_fileStream);
                    await _fileStream.FlushAsync();
                    return _data;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to serialize data to {configuration.Path}", ex);
            }
        }

        public bool Serialize(object data, QuickSaveConfiguration configuration)
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
                if (!File.Exists(configuration.Path))
                    throw new Exception($"File not foun from path: {configuration.Path}");

                using var _fileStream = new FileStream(configuration.Path, FileMode.Open, FileAccess.Write);

                if (configuration.UseGzipCompression)
                {
                    Compress(_fileStream, data, configuration.Path);
                }
                else
                {
                    Serializer.Serialize(_fileStream, data);
                }

                return File.Exists(configuration.Path);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to serialize data to {configuration.Path}", ex);
            }
        }

        public T Deserialize<T>(QuickSaveConfiguration configuration)
        {
            try
            {
                if (!File.Exists(configuration.Path))
                    throw new Exception($"File not foun from path: {configuration.Path}");

                using var _fileStream = new FileStream(configuration.Path, FileMode.Open, FileAccess.Read);

                if (configuration.UseGzipCompression)
                {
                    return Decompress<T>(_fileStream, configuration.Path);
                }
                else
                {
                    T _data = Serializer.Deserialize<T>(_fileStream);
                    return _data;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to serialize data to {configuration.Path}", ex);
            }
        }

        private async Task CompressAsync(Stream stream, object data, string filePath)
        {
            await using var _gzipStream = new GZipStream(stream, CompressionMode.Compress);
            Serializer.Serialize(_gzipStream, data);
            await _gzipStream.FlushAsync();
        }

        private async Task<T> DecompressAsync<T>(Stream stream, string filePath)
        {
            await using var _gzipStream = new GZipStream(stream, CompressionMode.Decompress);
            T _data = Serializer.Deserialize<T>(_gzipStream);
            await _gzipStream.FlushAsync();
            return _data;
        }

        private void Compress(Stream stream, object data, string filePath)
        {
            using var _gzipStream = new GZipStream(stream, CompressionMode.Compress);
            Serializer.Serialize(_gzipStream, data);
        }

        private T Decompress<T>(Stream stream, string filePath)
        {
            using var _gzipStream = new GZipStream(stream, CompressionMode.Decompress);
            T _data = Serializer.Deserialize<T>(_gzipStream);
            return _data;
        }
    }
}
