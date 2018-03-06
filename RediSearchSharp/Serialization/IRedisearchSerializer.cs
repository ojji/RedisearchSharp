using System.Collections.Generic;
using StackExchange.Redis;

namespace RediSearchSharp.Serialization
{
    public interface IRedisearchSerializer
    {
        Dictionary<string, RedisValue> Serialize<TEntity>(TEntity entity)
            where TEntity : RedisearchSerializable<TEntity>, new();

        TEntity Deserialize<TEntity>(Dictionary<string, RedisValue> fields)
            where TEntity : RedisearchSerializable<TEntity>, new();
    }
}