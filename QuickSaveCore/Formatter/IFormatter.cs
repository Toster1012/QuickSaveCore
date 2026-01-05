namespace QuickSave.Format
{
    public interface IFormatter
    {
        public bool Serialize(object data, QuickSaveConfiguration configuration);

        public T Deserialize<T>(QuickSaveConfiguration configuration);

        public Task<bool> SerializeAsync(object data, QuickSaveConfiguration configuration);

        public Task<T> DeserializeAsync<T>(QuickSaveConfiguration configuration);
    }
}
