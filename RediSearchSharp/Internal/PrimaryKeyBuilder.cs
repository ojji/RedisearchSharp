using System;

namespace RediSearchSharp.Internal
{
    internal class PrimaryKeyBuilder
    {
        private readonly string _propertyName;
        private readonly Type _propertyType;

        internal PrimaryKeyBuilder(string propertyName, Type propertyType)
        {
            _propertyName = propertyName;
            _propertyType = propertyType;
        }

        internal PrimaryKey Build<TEntity>()
        {
            return new PrimaryKey(typeof(TEntity), _propertyName, _propertyType);
        }
    }
}