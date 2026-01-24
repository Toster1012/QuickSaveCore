using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using QS.Convert;
using System;
using System.IO;
using System.Threading.Tasks;

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
            TypeNameHandling = TypeNameHandling.All, 
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Full
        };

        protected sealed async override Task<bool> Serialize<T>(Stream stream, T serializeObject)
        {
            try
            {
                if (serializeObject == null)
                    return false;

                using var _streamWriter = new StreamWriter(stream);

                string _json = JsonConvert.SerializeObject(serializeObject, typeof(T), _settings);

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

        protected sealed async override Task<T?> Deserialize<T>(Stream stream, CustomTypeConverter? customTypeConverter = null) where T : default
        {
            try
            {
                using var _streamReader = new StreamReader(stream);

                string _json = await _streamReader.ReadToEndAsync();
                object? _deserializeObject;

                if (customTypeConverter != null)
                    _deserializeObject = JsonConvert.DeserializeObject(_json, customTypeConverter.DeserializeType, _settings);
                else
                    _deserializeObject = JsonConvert.DeserializeObject<T>(_json, _settings);

                if (_deserializeObject == null)
                    throw new InvalidOperationException("Failed to deserialize value is null");

                if (customTypeConverter != null)
                    return (T?)customTypeConverter.Read(_deserializeObject);

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
