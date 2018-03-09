using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using StackExchange.Redis;

namespace RediSearchSharp.Internal
{
    internal class PrimaryKeySelectorBuilder
    {
        private readonly string _propertyName;
        private readonly Type _propertyType;

        internal PrimaryKeySelectorBuilder(string propertyName, Type propertyType)
        {
            _propertyName = propertyName;
            _propertyType = propertyType;
        }

        internal Func<TEntity, RedisValue> Build<TEntity>()
        {
            var parameter = Expression.Parameter(typeof(TEntity));
            Expression propertyValue = Expression.Property(parameter, _propertyName);

            var toStringWithIFormatProvider = _propertyType.GetMethod("ToString", new[] {typeof(IFormatProvider)});

            // call tostring
            propertyValue = toStringWithIFormatProvider != null
                ? Expression.Call(propertyValue, toStringWithIFormatProvider,
                    Expression.Constant(CultureInfo.InvariantCulture))
                : Expression.Call(propertyValue, typeof(object).GetMethod("ToString"));

            // and convert the string to a redisvalue
            var body = Expression.Convert(propertyValue, typeof(RedisValue));
            var retValue = Expression.Lambda<Func<TEntity, RedisValue>>(body, parameter);
            return retValue.Compile();
        }
    }
}