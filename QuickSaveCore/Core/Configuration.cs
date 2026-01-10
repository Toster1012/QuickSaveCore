namespace QuickSave
{
    public sealed class Configuration
    {
        public Dictionary<string, string> Paths { get; private set; } = new Dictionary<string, string>();
        public bool CreateDirectoryIfNotExist { get; set; } = false;
        public bool UseGzipCompression { get; set; } = false;

        public void AddPath(string key, string path)
        {
            if (Paths == null)
                Paths = new Dictionary<string, string>();

            if (!Paths.ContainsKey(key))
            {
                Paths.Add(key, path);
            }
        }
    }
}
