using QuickSave.Convert;
using QuickSave.Serialization;

namespace QuickSave
{
    public class SerializeOption
    {
        public IFormatter Formatter { get; set; } = new MessagePackFormatter();

        private List<CustomTypeConverter> _customTypeConverters = new List<CustomTypeConverter>();

        public IEnumerable<CustomTypeConverter> CustomTypeConverters => _customTypeConverters;

        public void AddInstructions(params CustomTypeConverter[] instructions)
        {
            foreach (var instruction in instructions)
            {
                if (!_customTypeConverters.Contains(instruction))
                {
                    _customTypeConverters.Add(instruction);
                }
            }
        }
    }
}
