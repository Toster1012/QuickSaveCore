using System;

namespace QS
{
    internal sealed class MissingAttributeException : SystemException
    {
        private Type _missingAttribute;
        private string _message;

        public MissingAttributeException(Type missingAttribute, string message)
        {
            _missingAttribute = missingAttribute;
            _message = message;
        }

        public override string Message => $"Attribute: {_missingAttribute.Name} {_message}";
    }
}
