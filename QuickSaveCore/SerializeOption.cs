namespace QuickSave
{
    public class SerializeOption
    {
        public IFormatter Formatter { get; set; } = new MessagePackFormatter();

        private List<SerializeInstruction> _serializeInstructions = new List<SerializeInstruction>();

        public IEnumerable<SerializeInstruction> SerializeInstructions => _serializeInstructions;

        public void AddInstructions(params SerializeInstruction[] instructions)
        {
            foreach (var instruction in instructions)
            {
                _serializeInstructions.Add(instruction);
            }
        }
    }
}
