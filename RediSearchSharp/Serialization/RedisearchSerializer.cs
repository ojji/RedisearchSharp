using RediSearchSharp.Internal;

namespace RediSearchSharp.Serialization
{
    internal class RedisearchSerializer : IRedisearchSerializer
    {
        public Document Serialize<TEntity>(TEntity entity, double score) where TEntity : class, IRedisearchSerializable<TEntity>, new()
        {
            var compiledIdSelector = entity.IdSelector.Compile();
            var entityId = compiledIdSelector(entity);
            var schemaInfo = SchemaInfo.GetSchemaInfo<TEntity>();

            return new Document
            {
                Id = string.Join(schemaInfo.DocumentIdPrefix, entityId),
                Fields = entity.SerializeToRedisearchFields(),
                Score = score
            };
        }

        public TEntity Deserialize<TEntity>(Document document) where TEntity : class, IRedisearchSerializable<TEntity>, new()
        {
            var entity = new TEntity();
            return entity.DeserializeFromRedisFields(document.Fields);
        }
    }
}