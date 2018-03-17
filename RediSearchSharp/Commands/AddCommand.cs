using System.Collections.Generic;
using System.Threading.Tasks;
using RediSearchSharp.Internal;
using RediSearchSharp.Serialization;
using RediSearchSharp.Utils;
using StackExchange.Redis;

namespace RediSearchSharp.Commands
{
    public class AddCommand
    {
        private List<object> Parameters { get; }

        private AddCommand(List<object> parameters)
        {
            Parameters = parameters;
        }

        public static AddCommand Create<TEntity>(IRedisearchSerializer serializer, TEntity entity, double score, string language)
            where TEntity: RedisearchSerializable<TEntity>, new()
        {
            var schemaMetadata = SchemaMetadata<TEntity>.GetSchemaMetadata();

            var indexName = schemaMetadata.IndexName;
            var entityId = string.Concat(schemaMetadata.DocumentIdPrefix, schemaMetadata.PrimaryKey.GetPrimaryKeyFromEntity(entity));

            if (string.IsNullOrEmpty(language))
            {
                language = schemaMetadata.Language;
            }

            var parameters = new List<object>
            {
                indexName,
                entityId,
                score,
                RedisearchIndexCache.GetBoxedLiteral("LANGUAGE"),
                RedisearchIndexCache.GetBoxedLiteral(language),
                RedisearchIndexCache.GetBoxedLiteral("FIELDS")
            };

            foreach (var fieldPairs in serializer.Serialize(entity))
            {
                parameters.Add(fieldPairs.Key);
                parameters.Add(fieldPairs.Value);
            }

            return new AddCommand(parameters);
        }

        public bool Execute(IDatabase db)
        {
            try
            {
                return (string) db.Execute("FT.ADD", Parameters) == "OK";
            }
            catch (RedisServerException)
            {
                return false;
            }
        }

        public async Task<bool> ExecuteAsync(IDatabase db)
        {
            try
            {
                return (string) await db.ExecuteAsync("FT.ADD", Parameters).ConfigureAwait(false) == "OK";
            }
            catch (RedisServerException)
            {
                return false;
            }
        }
    }
}