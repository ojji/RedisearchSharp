using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using StackExchange.Redis;

namespace RediSearchSharp.Serialization
{
    public interface IRedisearchSerializable<TEntity> 
        where TEntity : class, new()
    {
        Expression<Func<TEntity, string>> IdSelector { get; }
        Dictionary<string, RedisValue> SerializeToRedisearchFields();

        TEntity DeserializeFromRedisFields(Dictionary<string, RedisValue> fields);
    }
}