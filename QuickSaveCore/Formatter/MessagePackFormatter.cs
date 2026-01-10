using MessagePack;
using QS.Convert;

namespace QS.Serialization
{
    public sealed class MessagePackFormatter : Formatter
    {
        private readonly MessagePackSerializerOptions _options = MessagePackSerializer.Typeless.DefaultOptions;

        protected sealed override async Task<bool> Serialize<T>(Stream stream, T serializeObject)
        {
            try
            {
                if (serializeObject == null) 
                    return false;

                QSValue _qSValue = new QSValue(serializeObject, typeof(T));
                await MessagePackSerializer.SerializeAsync(stream, _qSValue, _options);
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
                QSValue _qSValue = MessagePackSerializer.Deserialize<QSValue>(stream, _options);

                if (customTypeConverter != null)
                    return (T)customTypeConverter.Read(_qSValue.Value);

                return (T)_qSValue.Value;
            }
            catch
            {
                throw;
            }
            finally
            {
                await stream.FlushAsync();
            }
        }
    }
}
