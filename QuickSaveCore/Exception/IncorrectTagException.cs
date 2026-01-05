namespace QuickSave
{
    internal class IncorrectTagException : SystemException
    {
        private int _tag;

        public IncorrectTagException(int tag)
        {
            _tag = tag;
        }

        public override string Message => $"Tag: {_tag} is active, change it to unoccupied";
    }
}
