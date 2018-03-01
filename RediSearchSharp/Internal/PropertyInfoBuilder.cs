namespace RediSearchSharp.Internal
{
    public class PropertyInfoBuilder
    {
        internal string PropertyName { get; }
        internal bool IsIgnored { get; private set; }

        internal PropertyInfoBuilder(string propertyName)
        {
            PropertyName = propertyName;
        }

        public void Ignore()
        {
            IsIgnored = true;
        }
    }
}