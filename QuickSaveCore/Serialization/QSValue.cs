using MessagePack;

namespace QuickSave.Serialization
{
    [Serializable]
    internal sealed class QSValue
    {
        [Key(0)] public Type Type { get; private set; }
        [Key(1)] public object Value { get; private set; }

        public QSValue() { }

        public QSValue(QSValue qSValue)
        {
            Type = qSValue.Type;
            Value = qSValue.Value;
        }

        public QSValue(object value, Type type)
        {
            Value = value;
            Type = type;
        }
    }
}
