using RediSearchSharp.Internal;

namespace RediSearchSharp.Serialization
{
    internal class RedisearchSerializer : IRedisearchSerializer
    {
        private string GetPrefixedDocumentId<TEntity>(string id)
        {
            return string.Join(":", Pluralize(typeof(TEntity).Name), id);
        }

        private static string Pluralize(string tableName)
        {
            if (!tableName.EndsWith("s"))
            {
                return string.Format($"{tableName}s");
            }

            return tableName;
        }

        public Document Serialize<TEntity>(TEntity entity, double score) where TEntity : class, IRedisearchSerializable<TEntity>, new()
        {
            var compiledIdSelector = entity.IdSelector.Compile();
            var entityId = compiledIdSelector(entity);

            return new Document
            {
                Id = GetPrefixedDocumentId<TEntity>(entityId),
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