using QS.Core;

namespace QS.Serialization
{
    public interface IFormatter
    {
        public bool Serialize<T>(string saveKey, T data, Configuration configuration, SerializeOption option);

        public T Deserialize<T>(string saveKey, Configuration configuration, SerializeOption option);

        public Task<bool> SerializeAsync<T>(string saveKey, T data, Configuration configuration, SerializeOption option);

        public Task<T> DeserializeAsync<T>(string saveKey, Configuration configuration, SerializeOption option);
    }
}
