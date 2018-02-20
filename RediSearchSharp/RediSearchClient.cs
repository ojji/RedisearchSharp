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

        public RediSearchClient(IConnectionMultiplexer redisConnection) : this(new RedisearchSerializer(), redisConnection) 
        {
        }

        internal RediSearchClient(IRedisearchSerializer redisearchSerializer, IConnectionMultiplexer redisConnection)
        {
            _serializer = redisearchSerializer ?? throw new ArgumentNullException(nameof(redisearchSerializer));
            _redisConnection = redisConnection ?? throw new ArgumentNullException(nameof(redisConnection));
        }

        public bool AddDocument<TEntity>(string indexName, TEntity entity, double score = 1.0d)
            where TEntity : class, IRedisearchSerializable<TEntity>, new()
        {
            var database = _redisConnection.GetDatabase();
            var serializedDocument = _serializer.Serialize(entity, score);

            return (string)database.Execute("FT.ADD", BuildAddDocumentParameters(indexName, serializedDocument)) == "OK";
        }

        public async Task<bool> AddDocumentAsync<TEntity>(string indexName, TEntity entity, double score = 1.0d)
            where TEntity : class, IRedisearchSerializable<TEntity>, new()
        {
            var database = _redisConnection.GetDatabase();
            var serializedDocument = _serializer.Serialize(entity, score);

            var response = (string) await database
                .ExecuteAsync("FT.ADD", BuildAddDocumentParameters(indexName, serializedDocument))
                .ConfigureAwait(false);

            return response == "OK";
        }

        private object[] BuildAddDocumentParameters(string indexName, Document doc)
        {
            var parameters = new List<object>
            {
                RedisearchIndexCache.GetBoxedIndexName(indexName),
                doc.Id,
                doc.Score,
                RedisearchIndexCache.GetBoxedLiteral("FIELDS")
            };

            foreach (var fieldPairs in doc.Fields)
            {
                parameters.Add(fieldPairs.Key);
                parameters.Add(fieldPairs.Value);
            }

            return parameters.ToArray();    
        }

        public SearchResult<TEntity> Search<TEntity>(string indexName, Query<TEntity> query) where TEntity : class, IRedisearchSerializable<TEntity>, new()
        {
            var database = _redisConnection.GetDatabase();

            var result = (RedisResult[])database.Execute("FT.SEARCH", BuildSearchDocumentParameters(indexName, query));

            return new SearchResult<TEntity>(
                _serializer, 
                result,  
                query.Options.WithScores, 
                query.Options.WithPayloads);
        }

        public async Task<SearchResult<TEntity>> SearchAsync<TEntity>(string indexName, Query<TEntity> query) where TEntity : class, IRedisearchSerializable<TEntity>, new()
        {
            var database = _redisConnection.GetDatabase();

            var result = (RedisResult[])await database.ExecuteAsync("FT.SEARCH", BuildSearchDocumentParameters(indexName, query)).ConfigureAwait(false);

            return new SearchResult<TEntity>(
                _serializer,
                result,
                query.Options.WithScores,
                query.Options.WithPayloads);
        }

        private object[] BuildSearchDocumentParameters<TEntity>(string indexName, Query<TEntity> query)
        {
            var parameters = new List<object>
            {
                RedisearchIndexCache.GetBoxedIndexName(indexName)
            };

            query.SerializeRedisArgs(parameters);
            return parameters.ToArray();
        }
    }
}