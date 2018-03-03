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
            var schemaMetadata = SchemaMetadata<TEntity>.GetSchemaMetadata();
            EnsureEntityMappingsAreRegistered<TEntity>(schemaMetadata);

            var entityId = schemaMetadata.PrimaryKeySelector(entity);

            return new Document
            {
                Id = string.Concat(schemaMetadata.DocumentIdPrefix, entityId),
                Fields = entity.SerializeToRedisearchFields(),
                Score = score
            };
        }

        public TEntity Deserialize<TEntity>(Document document) 
            where TEntity : RedisearchSerializable<TEntity>, new()
        {
            var schemaMetadata = SchemaMetadata<TEntity>.GetSchemaMetadata();
            EnsureEntityMappingsAreRegistered<TEntity>(schemaMetadata);

            var entity = new TEntity();
            return entity.DeserializeFromRedisFields(document.Fields);
        }

        private void EnsureEntityMappingsAreRegistered<TEntity>(SchemaMetadata<TEntity> schemaMetadata)
            where TEntity : RedisearchSerializable<TEntity>, new()
        {
            if (!RedisMapper.IsRegisteredType<TEntity>())
            {
                try
                {
                    RedisMapper.RegisterType<TEntity>(schemaMetadata.Properties.Where(p => !p.IsIgnored).Select(p => p.PropertyName).ToArray());
                }
                catch (Exception)
                {
                    throw new ArgumentException($"Entity type cannot be serialized: {typeof(TEntity)}");
                }
            }
        }
    }
}