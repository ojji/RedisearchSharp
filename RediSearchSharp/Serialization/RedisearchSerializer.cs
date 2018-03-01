using System;
using System.Linq;
using RediSearchSharp.Internal;

namespace RediSearchSharp.Serialization
{
    internal class RedisearchSerializer : IRedisearchSerializer
    {
        public Document Serialize<TEntity>(TEntity entity, double score) 
            where TEntity : RedisearchSerializable<TEntity>, new()
        {
            var schemaInfo = SchemaInfo<TEntity>.GetSchemaInfo();
            EnsureEntityMappingsAreRegistered<TEntity>(schemaInfo);

            var entityId = schemaInfo.PrimaryKeySelector(entity);

            return new Document
            {
                Id = string.Concat(schemaInfo.DocumentIdPrefix, entityId),
                Fields = entity.SerializeToRedisearchFields(),
                Score = score
            };
        }

        public TEntity Deserialize<TEntity>(Document document) 
            where TEntity : RedisearchSerializable<TEntity>, new()
        {
            var schemaInfo = SchemaInfo<TEntity>.GetSchemaInfo();
            EnsureEntityMappingsAreRegistered<TEntity>(schemaInfo);

            var entity = new TEntity();
            return entity.DeserializeFromRedisFields(document.Fields);
        }

        private void EnsureEntityMappingsAreRegistered<TEntity>(SchemaInfo<TEntity> schemaInfo)
            where TEntity : RedisearchSerializable<TEntity>, new()
        {
            if (!RedisMapper.IsRegisteredType<TEntity>())
            {
                try
                {
                    RedisMapper.RegisterType<TEntity>(schemaInfo.PropertiesToSerialize.ToArray());
                }
                catch (Exception)
                {
                    throw new ArgumentException($"Entity type cannot be serialized: {typeof(TEntity)}");
                }
            }
        }
    }
}