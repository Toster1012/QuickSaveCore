using System.IO.Compression;

namespace QuickSave.Compression
{
    public static class GZipArchiver
    {
        public static GZipStream Compress(Stream stream)
        {
            return new GZipStream(stream, CompressionMode.Compress, leaveOpen: true);
        }

        public static GZipStream Decompress(Stream stream)
        {
            return new GZipStream(stream, CompressionMode.Decompress, leaveOpen: true);
        }
    }
}
