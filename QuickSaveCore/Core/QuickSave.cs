using QuickSave.Serialization;

namespace QuickSave
{
    public static class QuickSave
    {
        public static SerializeOption Option { get; set; } = GetDefualtOptions();

        public static void Save<T>(string saveKey, T date, Configuration configuration, Action<bool>? callback = null)
        {
            SaveAsync(saveKey, date, configuration, callback).GetAwaiter().GetResult();
        }

        public static T Load<T>(string saveKey, Configuration configuration, Action<bool>? callback = null)
        {
            return LoadAsync<T>(saveKey, configuration, callback).GetAwaiter().GetResult();
        }

        public static void DeleteSave(string saveKey, Configuration configuration, Action<bool>? callback = null)
        {
            DeleteSaveAsync(saveKey, configuration, (result) => { callback?.Invoke(result); }).GetAwaiter().GetResult();
        }

        public static void DeleteAllSave(Configuration configuration, Action<bool>? callback = null)
        {
            DeleteAllSaveAsync(configuration, callback).GetAwaiter().GetResult();
        }

        public static async Task SaveAsync<T>(string saveKey, T date, Configuration configuration, Action<bool>? callback = null)
        {
            if (await Option.Formatter.SerializeAsync(saveKey, date, configuration, Option))
                callback?.Invoke(true);

            callback?.Invoke(false);
        }

        public static async Task<T> LoadAsync<T>(string saveKey, Configuration configuration, Action<bool>? callback = null)
        {
            const int MaxRetries = 10;
            const int BaseDelayMs = 50;

            Exception? _lastException = null;

            for (int attempt = 0; attempt < MaxRetries; attempt++)
            {
                try
                {
                    T _result = await Option.Formatter.DeserializeAsync<T>(saveKey, configuration, Option);
                    callback?.Invoke(true);
                    return _result;
                }
                catch (Exception expection) when (IsRetryableError(expection))
                {
                    _lastException = expection;
                    if (attempt < MaxRetries - 1)
                        await Task.Delay(BaseDelayMs * (attempt + 1));
                }
                catch
                {
                    callback?.Invoke(false);
                    throw;
                }
            }

            callback?.Invoke(false);
            throw new IOException($"Failed to download after {MaxRetries} attempts.", _lastException);
        }

        public static async Task DeleteSaveAsync(string saveKey, Configuration configuration, Action<bool>? callback = null)
        {
            try
            {
                if (configuration.Paths.TryGetValue(saveKey, out string? path))
                {
                    if (string.IsNullOrEmpty(path))
                        throw new ArgumentException($"Path null or empty");

                    if (!File.Exists(path))
                        throw new ArgumentException($"File not found, from path: {path}.");

                    await Task.Run(() => File.Delete(path));
                    callback?.Invoke(true);

                    return;
                }

                throw new ArgumentException($"No saving by key {saveKey} was found.");
            }
            catch
            {
                callback?.Invoke(false);
                throw;
            }
        }

        public static async Task DeleteAllSaveAsync(Configuration configuration, Action<bool>? callback = null)
        {
            try
            {
                foreach (KeyValuePair<string, string> save in configuration.Paths)
                    await DeleteSaveAsync(save.Key, configuration);

                callback?.Invoke(true);
            }
            catch
            {
                callback?.Invoke(false);
                throw;
            }
        }

        private static bool IsRetryableError(Exception exception)
        {
            if (exception is IOException iOException)
            {
                var _message = iOException.Message.ToLowerInvariant();
                if (_message.Contains("being used") || _message.Contains("used by another process") || (iOException.HResult & 0xFFFF) is 32 or 33)
                    return true;
            }

            if (exception is InvalidDataException ||
                exception is EndOfStreamException ||
                exception is MessagePack.MessagePackSerializationException ||
                (exception is AggregateException aggregateException && aggregateException.InnerExceptions.Any(IsRetryableError)))
            {
                return true;
            }

            return exception.InnerException != null && IsRetryableError(exception.InnerException);
        }

        private static SerializeOption GetDefualtOptions() => new SerializeOption() { Formatter = new MessagePackFormatter() };
    }
}
