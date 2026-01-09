namespace QuickSave
{
    public interface IFormatter
    {
        public bool Serialize<T>(T data, Configuration configuration, SerializeOption option);

        public T Deserialize<T>(Configuration configuration, SerializeOption option);

        public Task<bool> SerializeAsync<T>(T data, Configuration configuration, SerializeOption option);

        public Task<T> DeserializeAsync<T>(Configuration configuration, SerializeOption option);
    }
}
