namespace QuickSave
{
    public sealed class QuickSaveConfiguration
    {
        public string Path { get; set; }
        public IFormatter Formatter { get; set; } = new MessagePackFormatter();
        public bool CreateDirectoryIfNotExist { get; set; } = false;
        public bool UseGzipCompression { get; set; } = false;

        private List<SerializeInstruction> _serializeInstructions = new List<SerializeInstruction>();

        public IEnumerable<SerializeInstruction> SerializeInstructions => _serializeInstructions;

        public void AddInstruction(params SerializeInstruction[] serializeInstructions)
        {
            foreach (var serializeInstruction in serializeInstructions)
            {
                if (!_serializeInstructions.Contains(serializeInstruction))
                {
                    _serializeInstructions.Add(serializeInstruction);
                }
            }
        }
    }
}
