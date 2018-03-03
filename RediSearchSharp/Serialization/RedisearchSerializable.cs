using System.Collections.Generic;
using RediSearchSharp.Internal;
using StackExchange.Redis;

namespace RediSearchSharp.Serialization
{
    public abstract class RedisearchSerializable<TEntity>
        where TEntity : RedisearchSerializable<TEntity>, new()
    {
        public virtual Dictionary<string, RedisValue> SerializeToRedisearchFields()
        {
            return RedisMapper.MapToRedisValues(this as TEntity);
        }

        public TEntity DeserializeFromRedisFields(Dictionary<string, RedisValue> fields)
        {
            return RedisMapper.FromRedisValues<TEntity>(fields);
        }

        protected internal virtual void OnCreatingSchemaInfo(SchemaMetadataBuilder<TEntity> builder)
        {
        }
    }
}