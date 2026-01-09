namespace QuickSave
{
    public static class QuickSaveCore
    {
        public static SerializeOption Option { get; set; } = GetDefualtOptions();

        public static void Save<T>(T date, Configuration configuration, Action<bool>? callback = null)
        {
            SaveAsync(date, configuration, callback);
        }

        public static T Load<T>(Configuration configuration, Action<bool>? callback = null)
        {
            return LoadAsync<T>(configuration, callback).GetAwaiter().GetResult();
        }

        public static async void SaveAsync<T>(T date, Configuration configuration, Action<bool>? callback = null)
        {
            callback?.Invoke(await Option.Formatter.SerializeAsync(date, configuration, Option));
        }

        public static async Task<T> LoadAsync<T>(Configuration configuration, Action<bool>? callback = null)
        {
            const int MaxRetries = 10;
            const int BaseDelayMs = 50;

            Exception? _lastException = null;

            for (int attempt = 0; attempt < MaxRetries; attempt++)
            {
                try
                {
                    T _result = await Option.Formatter.DeserializeAsync<T>(configuration, Option);
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
