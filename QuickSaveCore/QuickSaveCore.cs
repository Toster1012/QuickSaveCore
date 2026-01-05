namespace QuickSave
{
    public static class QuickSaveCore
    {
        public static QuickSaveConfiguration Configuration { get; set; }

        public static void Save(object date, Action<bool>? callback = null)
        {
            if (!ConfigurationValid())
                throw new ArgumentNullException("Configuration cannot be null! Please create the configuration file.");

            if (Configuration.Formatter.Serialize(date, Configuration))
                callback?.Invoke(true);

            callback?.Invoke(false);
        }

        public static T Load<T>()
        {
            if (!ConfigurationValid())
                throw new ArgumentNullException("Configuration cannot be null! Please create the configuration file.");

            return Configuration.Formatter.Deserialize<T>(Configuration);
        }

        public static async void SaveAsync(object date, Action<bool>? callback = null)
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
