using System;
using System.Globalization;
using System.Linq.Expressions;
using StackExchange.Redis;

namespace RediSearchSharp.Internal
{
    public class PrimaryKey
    {
        public string PropertyName { get; }
        public Type PropertyClrType { get; }
        public Type EntityClrType { get; }
        
        // this should be a Func<TEntity, RedisValue>
        private object _primaryKeyFromEntityFunc;

        // this should be a Func<TProperty, RedisValue>
        private object _primaryKeyFromPropertyFunc;

        internal PrimaryKey(Type entityType, string propertyName, Type propertyType)
        {
            PropertyClrType = propertyType;
            PropertyName = propertyName;
            EntityClrType = entityType;
            BuildGetPrimaryKeyFromIdProperty(propertyType);
            BuildGetPrimaryKeyFromEntity(entityType, propertyName, propertyType);
        }

        public RedisValue GetPrimaryKeyFromEntity<TEntity>(TEntity entity)
        {
            if (EntityClrType != typeof(TEntity))
            {
                throw new ArgumentException($"Invalid entity type, this primary key belongs to {EntityClrType.Name}");
            }

            return ((Func<TEntity, RedisValue>)_primaryKeyFromEntityFunc)(entity);
        }

        public RedisValue GetPrimaryKeyFromProperty<TProperty>(TProperty property)
        {
            if (PropertyClrType != typeof(TProperty))
            {
                throw new ArgumentException($"Invalid property type, this primary key belongs to {PropertyClrType.Name}");
            }
            return ((Func<TProperty, RedisValue>)_primaryKeyFromPropertyFunc)(property);
        }

        private void BuildGetPrimaryKeyFromEntity(Type entityType, string propertyName, Type propertyType)
        {
            // this should be in the format of either 
            // TEntity entity => (RedisValue)(entity.{IdProperty}.ToString(CultureInfo.InvariantCulture))
            // or
            // TEntity entity => (RedisValue)(entity.{IdProperty}.ToString())

            var parameter = Expression.Parameter(entityType);
            Expression propertyValue = Expression.Property(parameter, propertyName);

            var toStringWithIFormatProvider = propertyType.GetMethod("ToString", new[] { typeof(IFormatProvider) });

            // call tostring
            propertyValue = toStringWithIFormatProvider != null
                ? Expression.Call(propertyValue, toStringWithIFormatProvider,
                    Expression.Constant(CultureInfo.InvariantCulture))
                : Expression.Call(propertyValue, typeof(object).GetMethod("ToString"));

            // and convert the string to a redisvalue
            var body = Expression.Convert(propertyValue, typeof(RedisValue));

            var funcOfEntityToRedisValue = typeof(Func<,>).MakeGenericType(entityType, typeof(RedisValue));
            _primaryKeyFromEntityFunc = Expression.Lambda(funcOfEntityToRedisValue, body, parameter).Compile();
        }


        private void BuildGetPrimaryKeyFromIdProperty(Type propertyType)
        {
            // this should be in the format of either 
            // TProperty property => (RedisValue)(property.ToString(CultureInfo.InvariantCulture))
            // or
            // TProperty property => (RedisValue)(property.ToString())

            var parameter = Expression.Parameter(propertyType);
            var toStringWithIFormatProvider = propertyType.GetMethod("ToString", new[] { typeof(IFormatProvider) });

            // call tostring
            var propertyValue = toStringWithIFormatProvider != null
                ? Expression.Call(parameter, toStringWithIFormatProvider,
                    Expression.Constant(CultureInfo.InvariantCulture))
                : Expression.Call(parameter, typeof(object).GetMethod("ToString"));

            var body = Expression.Convert(propertyValue, typeof(RedisValue));

            var funcOfPropertyToRedisValue = typeof(Func<,>).MakeGenericType(propertyType, typeof(RedisValue));
            _primaryKeyFromPropertyFunc = Expression.Lambda(funcOfPropertyToRedisValue, body, parameter).Compile();
        }
    }
}