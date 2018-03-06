using System;
using System.Collections.Generic;
using System.Linq;
using RediSearchSharp.Internal;
using StackExchange.Redis;

namespace RediSearchSharp.Serialization
{
    internal class RedisearchSerializer : IRedisearchSerializer
    {
        public Dictionary<string, RedisValue> Serialize<TEntity>(TEntity entity) where TEntity : RedisearchSerializable<TEntity>, new()
        {
            EnsureEntityMappingsAreRegistered<TEntity>();
            return RedisMapper.MapToRedisValues(entity);
        }

        public TEntity Deserialize<TEntity>(Dictionary<string, RedisValue> fields) where TEntity : RedisearchSerializable<TEntity>, new()
        {
            EnsureEntityMappingsAreRegistered<TEntity>();
            return RedisMapper.FromRedisValues<TEntity>(fields);
        }

        private void EnsureEntityMappingsAreRegistered<TEntity>()
            where TEntity : RedisearchSerializable<TEntity>, new()
        {
            var schemaMetadata = SchemaMetadata<TEntity>.GetSchemaMetadata();

            if (!RedisMapper.IsRegisteredType<TEntity>())
            {
                try
                {
                    RedisMapper.RegisterType<TEntity>(schemaMetadata.Properties.Where(p => !p.IsIgnored).Select(p => p.PropertyName).ToArray());
                }
                catch (Exception ex)
                {
                    throw new ArgumentException($"Entity type cannot be serialized: {typeof(TEntity)}", ex);
                }
            }
        }
    }
}