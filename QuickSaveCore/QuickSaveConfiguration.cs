using QuickSave;

namespace QuickSave
{
    public sealed class QuickSaveConfiguration
    {
        public string Path { get; private set; }
        public IFormatter Formatter { get; private set; } = new ProtoBufFormatter();
        public bool CreateDirectoryIfNotExist { get; private set; } = false;
        public bool UseGzipCompression { get; private set; } = false;

        public QuickSaveConfiguration(string path, IFormatter formatter, bool createDirectoryIfNotExist, bool useGzipCompression)
        {
            Path = path;
            Formatter = formatter;
            CreateDirectoryIfNotExist = createDirectoryIfNotExist;
            UseGzipCompression = useGzipCompression;
        }
    }
}
