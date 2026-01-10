using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using QS.Convert;

namespace QS.Serialization
{
    public sealed class JsonFormatter : Formatter
    {
        private readonly JsonSerializerSettings _settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            TypeNameHandling = TypeNameHandling.Auto
        };

        protected sealed async override Task<bool> Serialize<T>(Stream stream, T serializeObject)
        {
            try
            {
                if (serializeObject == null)
                    return false;

                using var _streamWriter = new StreamWriter(stream, leaveOpen: true);

                QSValue _qSValue = new QSValue(serializeObject, typeof(T));
                string _json = JsonConvert.SerializeObject(_qSValue, typeof(QSValue), _settings);

                await _streamWriter.WriteAsync(_json);
                await stream.FlushAsync();

                return true;
            }
            catch
            {
                return false;
                throw;
            }
        }

        protected sealed async override Task<T> Deserialize<T>(Stream stream, CustomTypeConverter? customTypeConverter = null)
        {
            try
            {
                using var _streamReader = new StreamReader(stream, leaveOpen: true);

                string _json = await _streamReader.ReadToEndAsync();
                QSValue? _qSValue = JsonConvert.DeserializeObject<QSValue>(_json, _settings);

                if (_qSValue?.Value == null)
                    throw new InvalidOperationException("Failed to deserialize value is null");

                if (customTypeConverter != null)
                    return (T)customTypeConverter.Read(_qSValue.Value);

                if (_qSValue.Value is T result)
                    return result;
  
                throw new InvalidCastException(
                    $"Cannot cast deserialized value of type {_qSValue.Value?.GetType().Name} " +
                    $"to expected type {typeof(T).Name}");
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
