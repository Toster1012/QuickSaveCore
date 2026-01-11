using QS.Convert;
using QS.Serialization;

namespace QS.Core
{
    public class SerializeOption
    {
        public Formatter Formatter { get; set; } = new BinaryFormatter();

        private List<CustomTypeConverter> _customTypeConverters = new List<CustomTypeConverter>();

        public IEnumerable<CustomTypeConverter> CustomTypeConverters => _customTypeConverters;

        public void AddCustomConverter(params CustomTypeConverter[] customTypeConverters)
        {
            foreach (var converter in customTypeConverters)
            {
                if (!_customTypeConverters.Contains(converter))
                {
                    _customTypeConverters.Add(converter);
                }
            }
        }
    }
}
