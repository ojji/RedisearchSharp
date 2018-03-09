using System;
using StackExchange.Redis;

namespace RediSearchSharp.Internal
{
    /// <summary>
    /// Provides a simple API for configuring how RediSearch should handle the entity
    /// properties.
    /// </summary>
    public class PropertyMetadataBuilder
    {
        private bool _ignored;
        private bool _notIndexed;
        private bool _sortable;
        private readonly string _propertyName;
        private readonly Type _clrType;
        private RedisearchPropertyType _redisearchType;
        private bool _noStem;
        private double? _weight;

        internal PropertyMetadataBuilder(string propertyName, Type clrType)
        {
            _propertyName = propertyName;
            _clrType = clrType;
            _redisearchType = AssignRedisearchTypeTo(clrType);
        }

        /// <summary>
        /// Ignores the property in the serialization process, meaning it wont be saved 
        /// in the database at all. Useful to mark properties with calculated values.
        /// </summary>
        public void Ignore()
        {
            _ignored = true;
        }

        /// <summary>
        /// Sets the underlying redisearch type for property.
        /// </summary>
        /// <param name="propertyType">The property type to be used by the indexing engine.</param>
        public void AsRedisearchType(RedisearchPropertyType propertyType)
        {
            _redisearchType = propertyType;
        }

        /// <summary>
        /// Sets the property to be not indexed by the engine but keep it in the serialization process.
        /// </summary>
        public void NotIndexed()
        {
            _notIndexed = true;
        }

        /// <summary>
        /// Sets the property to be sortable.
        /// </summary>
        public void AsSortable()
        {
            _sortable = true;
        }

        /// <summary>
        /// Sets a text property to be not stemmed by the engine.
        /// </summary>
        public void NotStemmed()
        {
            _noStem = true;
        }
        
        /// <summary>
        /// Sets the property weight on a text property. This value is used by the
        /// indexing engine's scoring function. When it is not set, the default value
        /// of 1.0d will be used.
        /// </summary>
        /// <param name="weight">The property weight.</param>
        public void WithWeight(double weight)
        {
            if (weight <= 0.0d)
            {
                throw new ArgumentException("Weight must be positive.");
            }
            _weight = weight;
        }

        internal PropertyMetadata Build()
        {
            EnsurePropertyMetadataIsValid();
            return new PropertyMetadata(
                _propertyName, 
                _clrType, 
                _ignored, 
                _redisearchType,
                _notIndexed, 
                _sortable, 
                _noStem, 
                _weight ?? 1.0d);
        }

        private void EnsurePropertyMetadataIsValid()
        {
            if (_redisearchType == RedisearchPropertyType.Geo && _sortable)
            {
                throw new ArgumentException("You cannot set sortable on a geo property.");
            }

            if (_redisearchType != RedisearchPropertyType.Fulltext && _noStem)
            {
                throw new ArgumentException("You can disable stemming only on a fulltext property.");
            }

            if (_redisearchType != RedisearchPropertyType.Fulltext && _weight.HasValue)
            {
                throw new ArgumentException("You can only set weight on fulltext properties.");
            }
        }

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
    }
}