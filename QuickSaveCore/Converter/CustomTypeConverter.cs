namespace QS.Convert
{
    public abstract class CustomTypeConverter
    {
        public abstract Type SerializeType { get; }
        public abstract Type DeserializeType { get; }

        public abstract object Write(object? value);

        public abstract object Read(object? existingValue);
    }

    public abstract class CustomTypeConverter<TSource, TTarget> : CustomTypeConverter
    {
        public sealed override Type SerializeType => typeof(TSource);
        public sealed override Type DeserializeType => typeof(TTarget);

        public sealed override object Write(object? value)
        {
            if (value == null || value.GetType() != typeof(TSource))
                throw new SerializeInstructionException($"The value cannot be serialized, it is null, or it is not a {typeof(TSource)}!");

            return WriteObject((TSource)value);
        }

        public abstract TTarget WriteObject(TSource? value);

        public sealed override object Read(object? existingValue)
        {
            return ReadObject((TTarget)existingValue);
        }

        public abstract TSource ReadObject(TTarget existingValue);
    }
}
