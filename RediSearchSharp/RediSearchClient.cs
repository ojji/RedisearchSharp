using System;
using System.Collections.Generic;
using System.Linq;
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

            try
            {
                return (string) database.Execute("FT.ADD", BuildAddDocumentParameters(entity, score, language)) ==
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
            try
            {
                var response = (string) await database
                    .ExecuteAsync("FT.ADD", BuildAddDocumentParameters(entity, score, language))
                    .ConfigureAwait(false);
                return response == "OK";
            }
            catch (RedisServerException)
            {
                return false;
            }
        }

        private object[] BuildAddDocumentParameters<TEntity>(TEntity entity, double score, string language)
            where TEntity : RedisearchSerializable<TEntity>, new()
        {
            var schemaMetadata = SchemaMetadata<TEntity>.GetSchemaMetadata();

            var indexName = schemaMetadata.IndexName;
            var entityId = string.Concat(schemaMetadata.DocumentIdPrefix, schemaMetadata.GetPrimaryKeySelectorFromEntity()(entity));

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

            foreach (var fieldPairs in _serializer.Serialize(entity))
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

        public bool CreateIndex<TEntity>()
            where TEntity : RedisearchSerializable<TEntity>, new()
        {
            var database = _redisConnection.GetDatabase();

            try
            {
                return (string) database.Execute("FT.CREATE", BuildCreateParameters<TEntity>()) == "OK";
            }
            catch (RedisServerException)
            {
                return false;
            }
        }

        public async Task<bool> CreateIndexAsync<TEntity>()
            where TEntity : RedisearchSerializable<TEntity>, new()
        {
            var database = _redisConnection.GetDatabase();

            try
            {
                return (string) await database.ExecuteAsync("FT.CREATE", BuildCreateParameters<TEntity>())
                           .ConfigureAwait(false) == "OK";
            }
            catch (RedisServerException)
            {
                return false;
            }
        }

        private object[] BuildCreateParameters<TEntity>()
            where TEntity : RedisearchSerializable<TEntity>, new()
        {
            var schemaMetadata = SchemaMetadata<TEntity>.GetSchemaMetadata();
            
            var parameters = new List<object>
            {
                RedisearchIndexCache.GetBoxedIndexName(schemaMetadata.IndexName),
                RedisearchIndexCache.GetBoxedLiteral("SCHEMA")
            };

            foreach (var propertyMetadata in schemaMetadata.Properties.Where(pm => !pm.IsIgnored))
            {
                AddPropertyParametersTo(parameters, propertyMetadata);
            }

            return parameters.ToArray();
        }

        private void AddPropertyParametersTo(List<object> parameters, PropertyMetadata propertyMetadata)
        {
            parameters.Add(propertyMetadata.PropertyName);
            switch (propertyMetadata.RedisearchType)
            {
                case RedisearchPropertyType.Fulltext:
                    parameters.Add(RedisearchIndexCache.GetBoxedLiteral("TEXT"));
                    if (propertyMetadata.NoStem)
                    {
                        parameters.Add(RedisearchIndexCache.GetBoxedLiteral("NOSTEM"));
                    }
                    parameters.Add(RedisearchIndexCache.GetBoxedLiteral("WEIGHT"));
                    parameters.Add((RedisValue) propertyMetadata.Weight);
                    if (propertyMetadata.Sortable)
                    {
                        parameters.Add(RedisearchIndexCache.GetBoxedLiteral("SORTABLE"));
                    }
                    break;
                case RedisearchPropertyType.Numeric:
                    parameters.Add(RedisearchIndexCache.GetBoxedLiteral("NUMERIC"));
                    if (propertyMetadata.Sortable)
                    {
                        parameters.Add(RedisearchIndexCache.GetBoxedLiteral("SORTABLE"));
                    }
                    break;
                case RedisearchPropertyType.Geo:
                    parameters.Add(RedisearchIndexCache.GetBoxedLiteral("GEO"));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (propertyMetadata.NotIndexed)
            {
                parameters.Add(RedisearchIndexCache.GetBoxedLiteral("NOINDEX"));
            }
        }
    }
}