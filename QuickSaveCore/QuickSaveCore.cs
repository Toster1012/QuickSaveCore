namespace QuickSave
{
    public static class QuickSaveCore
    {
        public static QuickSaveConfiguration Configuration { get; set; }

        public static void Save<T>(T date, Action<bool>? callback = null)
        {
            SaveAsync(date, callback);
        }

        public static T Load<T>()
        {
            return LoadAsync<T>().Result;
        }

        public static async void SaveAsync<T>(T date, Action<bool>? callback = null)
        {
            if (!ConfigurationValid())
                throw new ArgumentNullException("Configuration cannot be null! Please create the configuration file.");

            if (await Configuration.Formatter.SerializeAsync(date, Configuration))
                callback?.Invoke(true);

            callback?.Invoke(false);
        }

        public static async Task<T> LoadAsync<T>()
        {
            if(!ConfigurationValid())
                throw new ArgumentNullException("Configuration cannot be null! Please create the configuration file.");

            return await Configuration.Formatter.DeserializeAsync<T>(Configuration);
        }

        private static bool ConfigurationValid() => Configuration != null;
    }
}
