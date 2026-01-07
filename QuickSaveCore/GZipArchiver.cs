using System.IO.Compression;

namespace QuickSave
{
    internal static class GZipArchiver
    {
        public static async Task CompressAsync<T>(Stream stream, T? data, string filePath, Func<Stream, object, Task> serialize)
        {
            if (data == null)
                return;

            await using var _gzipStream = new GZipStream(stream, CompressionMode.Compress);

            QSValue _qSValue = new QSValue(data, typeof(T));
            await serialize.Invoke(_gzipStream, data);
            await _gzipStream.FlushAsync();
        }

        public static async Task<T> DecompressAsync<T>(Stream stream, string filePath, Func<Stream, Task<T>> deserialize)
        {
            await using var _gzipStream = new GZipStream(stream, CompressionMode.Decompress);
            
            try
            {
                return deserialize.Invoke(_gzipStream).Result;
            }
            finally
            {
                await _gzipStream.FlushAsync();
            }
        }
    }
}
