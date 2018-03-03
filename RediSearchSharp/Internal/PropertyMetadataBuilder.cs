using System;
using StackExchange.Redis;

namespace RediSearchSharp.Internal
{
    public class PropertyMetadataBuilder
    {
        private bool _ignored;
        private bool _notIndexed;
        private bool _sortable;
        private readonly string _propertyName;
        private readonly Type _clrType;
        private RedisearchPropertyType _redisearchType;
        private bool _noStem;

        private RedisearchPropertyType AssignRedisearchTypeTo(Type clrType)
        {
            if (IsNumericType(clrType))
            {
                return RedisearchPropertyType.Numeric;
            }

            return IsGeoType(clrType) ? RedisearchPropertyType.Geo : RedisearchPropertyType.Fulltext;
        }

        private bool IsGeoType(Type clrType)
        {
            return clrType == typeof(GeoPosition);
        }

        private bool IsNumericType(Type clrType)
        {
            return clrType == typeof(byte) ||
                   clrType == typeof(sbyte) ||
                   clrType == typeof(short) ||
                   clrType == typeof(ushort) ||
                   clrType == typeof(int) ||
                   clrType == typeof(uint) ||
                   clrType == typeof(long) ||
                   clrType == typeof(ulong) ||
                   clrType == typeof(decimal) ||
                   clrType == typeof(float) ||
                   clrType == typeof(double);
        }

        internal PropertyMetadataBuilder(string propertyName, Type clrType)
        {
            _propertyName = propertyName;
            _clrType = clrType;
            _redisearchType = AssignRedisearchTypeTo(clrType);
        }

        public void Ignore()
        {
            _ignored = true;
        }

        public void AsRedisearchType(RedisearchPropertyType propertyType)
        {
            _redisearchType = propertyType;
        }

        public void NotIndexed()
        {
            _notIndexed = true;
        }

        public void Sortable()
        {
            _sortable = true;
        }

        public void NotStemmed()
        {
            _noStem = true;
        }

        internal PropertyMetadata Build()
        {
            EnsurePropertyMetadataIsValid();
            return new PropertyMetadata(_propertyName, _clrType, _ignored, _redisearchType, _notIndexed, _sortable, _noStem);
        }

        private void EnsurePropertyMetadataIsValid()
        {
            if (_redisearchType == RedisearchPropertyType.Geo && _sortable)
            {
                throw new ArgumentException("You cannot set sortable on a geo property");
            }

            if (_redisearchType != RedisearchPropertyType.Fulltext && _noStem)
            {
                throw new ArgumentException("You can disable stemming only on a fulltext property");
            }
        }
    }
}