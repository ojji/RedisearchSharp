using System;
using System.Globalization;
using System.Linq.Expressions;
using StackExchange.Redis;

namespace RediSearchSharp.Internal
{
    public class PrimaryKey
    {
        public PrimaryKey(Type entityType, string propertyName, Type propertyType)
        {
            GetPrimaryKeyFromIdProperty = BuildGetPrimaryKeyFromIdProperty(propertyType);
            GetPrimaryKeyFromEntity = BuildGetPrimaryKeyFromEntity(entityType, propertyName, propertyType);
        }

        private LambdaExpression BuildGetPrimaryKeyFromEntity(Type entityType, string propertyName, Type propertyType)
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
            return Expression.Lambda(funcOfEntityToRedisValue, body, parameter);
        }


        private LambdaExpression BuildGetPrimaryKeyFromIdProperty(Type propertyType)
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
            return Expression.Lambda(funcOfPropertyToRedisValue, body, parameter);
        }

        internal LambdaExpression GetPrimaryKeyFromEntity { get; } // this should be Func<TEntity, RedisValue>
        internal LambdaExpression GetPrimaryKeyFromIdProperty { get; } // this should be a Func<TProperty, RedisValue>
    }
}