using MessagePack;
using QS.Convert;

namespace QS.Serialization
{
    public sealed class BinaryFormatter : Formatter
    {
        private readonly MessagePackSerializerOptions _options = MessagePackSerializer.Typeless.DefaultOptions;

        protected sealed override async Task<bool> Serialize<T>(Stream stream, T serializeObject)
        {
            try
            {
                if (serializeObject == null) 
                    return false;

                await MessagePackSerializer.Typeless.SerializeAsync(stream, serializeObject, _options);
                await stream.FlushAsync();
                return true;
            }
            catch
            {
                return false;
                throw;
            }
        }

        protected sealed override async Task<T> Deserialize<T>(Stream stream, CustomTypeConverter? customTypeConverter = null)
        {
            try
            {
                object? _deserializeObject;

                if (customTypeConverter != null)
                    _deserializeObject = MessagePackSerializer.Typeless.Deserialize(stream, _options);
                else
                    _deserializeObject = MessagePackSerializer.Typeless.Deserialize(stream, _options);

                if (customTypeConverter != null)
                    return (T)customTypeConverter.Read(_deserializeObject);

                if (_deserializeObject == null)
                    throw new InvalidOperationException("Failed to deserialize value is null");

                if (customTypeConverter != null)
                    return (T)customTypeConverter.Read(_deserializeObject);

                if (_deserializeObject is T result)
                    return result;

                throw new InvalidCastException(
                    $"Cannot cast deserialized value of type {_deserializeObject?.GetType().Name} " +
                    $"to expected type {typeof(T).Name}");
            }
            catch
            {
                throw;
            }
        }
    }
}
