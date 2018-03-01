using System;
using RediSearchSharp.Serialization;
using StackExchange.Redis;

namespace RediSearchSharp.Internal
{
    public interface IRedisearchConventions
    {
        string GetIndexName<TEntity>();
        string GetDocumentIdPrefix<TEntity>();
        Func<TEntity, RedisValue> GetPrimaryKey<TEntity>();
    }
}