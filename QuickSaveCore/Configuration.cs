namespace QuickSave
{
    public sealed class Configuration
    {
        public required string Path { get; set; }
        public bool CreateDirectoryIfNotExist { get; set; } = false;
        public bool UseGzipCompression { get; set; } = false;
    }
}
