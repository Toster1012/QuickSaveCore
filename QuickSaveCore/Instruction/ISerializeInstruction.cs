namespace QuickSave
{
    public interface ISerializeInstruction
    {
        object Serialize(object serializeObject);
        object Deserialize(object deserializeObject);
    }
}
