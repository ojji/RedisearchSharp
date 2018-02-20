using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using StackExchange.Redis;

namespace RediSearchSharp.Serialization
{
    public abstract class RedisearchSerializable<TEntity> : IRedisearchSerializable<TEntity>
        where TEntity : RedisearchSerializable<TEntity>, new()
    {
        protected RedisearchSerializable()
        {
            if (!RedisMapper.IsRegisteredType<TEntity>())
            {
                try
                {
                    RedisMapper.RegisterType<TEntity>();
                }
                catch (Exception)
                {
                    throw new ArgumentException($"Entity type cannot be serialized: {typeof(TEntity)}");
                }
            }
        }

        public abstract Expression<Func<TEntity, string>> IdSelector { get; }

        public virtual Dictionary<string, RedisValue> SerializeToRedisearchFields()
        {
            return RedisMapper.MapToRedisValues((TEntity)this);
        }

        public TEntity DeserializeFromRedisFields(Dictionary<string, RedisValue> fields)
        {
            return RedisMapper.FromRedisValues<TEntity>(fields);
        }
    }
}