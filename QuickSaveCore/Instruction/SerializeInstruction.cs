namespace QuickSave
{
    public abstract class SerializeInstruction
    {
        public abstract Type SerializeType { get; }

        public abstract object Write(object? value);

        public abstract object Read(object? existingValue);
    }

    public abstract class SerializeInstruction<In, Out> : SerializeInstruction
    {
        public sealed override Type SerializeType => typeof(In);

        public sealed override object Write(object? value)
        {
            if (value == null || value.GetType() != typeof(In))
                throw new SerializeInstructionException($"The value cannot be serialized, it is null, or it is not a {typeof(In)}!");

            return WriteObject((In)value);
        }

        public abstract Out WriteObject(In? value);

        public sealed override object Read(object? existingValue)
        {
            return ReadObject((Out)existingValue);
        }

        public abstract In ReadObject(Out existingValue);
    }
}
