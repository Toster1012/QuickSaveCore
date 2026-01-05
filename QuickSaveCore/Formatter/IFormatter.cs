namespace QuickSave
{
    public interface IFormatter
    {
        public bool Serialize<T>(T data, QuickSaveConfiguration configuration);

        public T Deserialize<T>(QuickSaveConfiguration configuration);

        public Task<bool> SerializeAsync<T>(T data, QuickSaveConfiguration configuration);

        public Task<T> DeserializeAsync<T>(QuickSaveConfiguration configuration);
    }
}
