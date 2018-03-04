using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RediSearchSharp.Internal;
using RediSearchSharp.Query;
using RediSearchSharp.Serialization;
using RediSearchSharp.Utils;
using StackExchange.Redis;

namespace RediSearchSharp
{
    public class RediSearchClient
    {
        private readonly IRedisearchSerializer _serializer;
        private readonly IConnectionMultiplexer _redisConnection;

        public RediSearchClient(IConnectionMultiplexer redisConnection) : this(new RedisearchSerializer(),
            redisConnection)
        {
        }

        internal RediSearchClient(IRedisearchSerializer redisearchSerializer, IConnectionMultiplexer redisConnection)
        {
            _serializer = redisearchSerializer ?? throw new ArgumentNullException(nameof(redisearchSerializer));
            _redisConnection = redisConnection ?? throw new ArgumentNullException(nameof(redisConnection));
        }

        public bool AddDocument<TEntity>(TEntity entity, double score = 1.0d, string language = null)
            where TEntity : RedisearchSerializable<TEntity>, new()
        {
            var database = _redisConnection.GetDatabase();
            var serializedDocument = _serializer.Serialize(entity, score);

            try
            {
                return (string) database.Execute("FT.ADD", BuildAddDocumentParameters<TEntity>(serializedDocument, language)) ==
                       "OK";
            }
            catch (RedisServerException)
            {
                return false;
            }
        }

        public async Task<bool> AddDocumentAsync<TEntity>(TEntity entity, double score = 1.0d, string language = null)
            where TEntity : RedisearchSerializable<TEntity>, new()
        {
            var database = _redisConnection.GetDatabase();
            var serializedDocument = _serializer.Serialize(entity, score);

            try
            {
                var response = (string) await database
                    .ExecuteAsync("FT.ADD", BuildAddDocumentParameters<TEntity>(serializedDocument, language))
                    .ConfigureAwait(false);
                return response == "OK";
            }
            catch (RedisServerException)
            {
                return false;
            }
        }

        private object[] BuildAddDocumentParameters<TEntity>(Document doc, string language) 
            where TEntity : RedisearchSerializable<TEntity>, new()
        {
            var schemaMetadata = SchemaMetadata<TEntity>.GetSchemaMetadata();

            if (string.IsNullOrEmpty(language))
            {
                language = schemaMetadata.Language;
            }

            var parameters = new List<object>
            {
                schemaMetadata.IndexName,
                doc.Id,
                doc.Score,
                RedisearchIndexCache.GetBoxedLiteral("LANGUAGE"),
                RedisearchIndexCache.GetBoxedLiteral(language),
                RedisearchIndexCache.GetBoxedLiteral("FIELDS")
            };

            foreach (var fieldPairs in doc.Fields)
            {
                parameters.Add(fieldPairs.Key);
                parameters.Add(fieldPairs.Value);
            }

            return parameters.ToArray();
        }

        public IEnumerable<SearchResult<TEntity>> Search<TEntity>(Query<TEntity> query)
            where TEntity : RedisearchSerializable<TEntity>, new()
        {
            var database = _redisConnection.GetDatabase();

            var result = (RedisResult[]) database.Execute("FT.SEARCH", BuildSearchDocumentParameters(query));

            return SearchResult<TEntity>.LoadResults(
                _serializer,
                result,
                query.Options.WithScores,
                query.Options.WithPayloads);
        }

        public async Task<IEnumerable<SearchResult<TEntity>>> SearchAsync<TEntity>(Query<TEntity> query)
            where TEntity : RedisearchSerializable<TEntity>, new()
        {
            var database = _redisConnection.GetDatabase();

            var result = (RedisResult[]) await database.ExecuteAsync("FT.SEARCH", BuildSearchDocumentParameters(query))
                .ConfigureAwait(false);

            return SearchResult<TEntity>.LoadResults(
                _serializer,
                result,
                query.Options.WithScores,
                query.Options.WithPayloads);
        }

        private object[] BuildSearchDocumentParameters<TEntity>(Query<TEntity> query)
            where TEntity : RedisearchSerializable<TEntity>, new()
        {
            var schemaMetadata = SchemaMetadata<TEntity>.GetSchemaMetadata();
            var parameters = new List<object>
            {
                schemaMetadata.IndexName
            };

            query.SerializeRedisArgs(parameters);
            return parameters.ToArray();
        }
    }
}