using ProtoBuf;
using System.IO.Compression;

namespace QuickSave
{
    internal static class GZipArchiver
    {
        public static async Task CompressAsync(Stream stream, object? data, string filePath)
        {
            if (data == null)
                return;

            await using var _gzipStream = new GZipStream(stream, CompressionMode.Compress);
            Serializer.Serialize(_gzipStream, data);
            await _gzipStream.FlushAsync();
        }

        public static async Task<T> DecompressAsync<T>(Stream stream, string filePath)
        {
            await using var _gzipStream = new GZipStream(stream, CompressionMode.Decompress);
            T _data = Serializer.Deserialize<T>(_gzipStream);
            await _gzipStream.FlushAsync();
            return _data;
        }
    }
}
