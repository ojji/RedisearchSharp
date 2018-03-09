using System;

namespace RediSearchSharp.Internal
{
    public class PropertyMetadata
    {
        public string PropertyName { get; }
        public Type ClrType { get; }
        public bool IsIgnored { get; }
        public RedisearchPropertyType RedisearchType { get; }
        public bool NotIndexed { get; }
        public bool Sortable { get; }
        public bool NoStem { get; }
        public double Weight { get; }

        internal PropertyMetadata(string propertyName, Type clrType, bool isIgnored,
            RedisearchPropertyType redisearchType, bool notIndexed, bool sortable, bool noStem, double weight)
        {
            PropertyName = propertyName;
            ClrType = clrType;
            IsIgnored = isIgnored;
            RedisearchType = redisearchType;
            NotIndexed = notIndexed;
            Sortable = sortable;
            NoStem = noStem;
            Weight = weight;
        }
    }
}