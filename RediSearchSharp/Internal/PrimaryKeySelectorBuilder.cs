using System;
using System.Collections.Generic;
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
            if (!BuiltInRedisValueTypes.Contains(_propertyType))
            {
                propertyValue = Expression.Call(propertyValue, typeof(object).GetMethod("ToString"));
            }

            var body = Expression.Convert(propertyValue, typeof(RedisValue));

            var retValue = Expression.Lambda<Func<TEntity, RedisValue>>(body, parameter);
            return retValue.Compile();
        }

        private static readonly List<Type> BuiltInRedisValueTypes = new List<Type>
        {
            typeof(byte[]),
            typeof(bool),
            typeof(byte),
            typeof(char),
            typeof(decimal),
            typeof(double),
            typeof(short),
            typeof(int),
            typeof(long),
            typeof(object),
            typeof(sbyte),
            typeof(float),
            typeof(string),
            typeof(ushort),
            typeof(uint),
            typeof(ulong)
        };
    }
}