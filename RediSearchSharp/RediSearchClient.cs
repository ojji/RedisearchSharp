using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RediSearchSharp.Commands;
using RediSearchSharp.Query;
using RediSearchSharp.Serialization;
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

        public RediSearchClient(IRedisearchSerializer redisearchSerializer, IConnectionMultiplexer redisConnection)
        {
            _serializer = redisearchSerializer ?? throw new ArgumentNullException(nameof(redisearchSerializer));
            _redisConnection = redisConnection ?? throw new ArgumentNullException(nameof(redisConnection));
        }

        public bool AddDocument<TEntity>(TEntity entity, double score = 1.0d, string language = null)
            where TEntity : RedisearchSerializable<TEntity>, new()
        {
            var database = _redisConnection.GetDatabase();
            var addCommand = AddCommand.Create(_serializer, entity, score, language);

            return addCommand.Execute(database);
        }

        public async Task<bool> AddDocumentAsync<TEntity>(TEntity entity, double score = 1.0d, string language = null)
            where TEntity : RedisearchSerializable<TEntity>, new()
        {
            var database = _redisConnection.GetDatabase();
            var addCommand = AddCommand.Create(_serializer, entity, score, language);

            return await addCommand.ExecuteAsync(database).ConfigureAwait(false);
        }

        public bool DeleteDocument<TEntity>(TEntity entity, bool deleteFromDatabase = true)
            where TEntity : RedisearchSerializable<TEntity>, new()
        {
            var database = _redisConnection.GetDatabase();
            var deleteCommand = DeleteCommand.Create(entity, deleteFromDatabase);

            return deleteCommand.Execute(database);
        }

        public async Task<bool> DeleteDocumentAsync<TEntity>(TEntity entity, bool deleteFromDatabase = true)
            where TEntity : RedisearchSerializable<TEntity>, new()
        {
            var database = _redisConnection.GetDatabase();
            var deleteCommand = DeleteCommand.Create(entity, deleteFromDatabase);

            return await deleteCommand.ExecuteAsync(database).ConfigureAwait(false);
        }

        public IEnumerable<SearchResult<TEntity>> Search<TEntity>(Query<TEntity> query)
            where TEntity : RedisearchSerializable<TEntity>, new()
        {
            var database = _redisConnection.GetDatabase();
            var searchCommand = SearchCommand.Create(query);
            
            return searchCommand.Execute<TEntity>(database, _serializer);
        }

        public async Task<IEnumerable<SearchResult<TEntity>>> SearchAsync<TEntity>(Query<TEntity> query)
            where TEntity : RedisearchSerializable<TEntity>, new()
        {
            var database = _redisConnection.GetDatabase();
            var searchCommand = SearchCommand.Create(query);

            return await searchCommand.ExecuteAsync<TEntity>(database, _serializer).ConfigureAwait(false);
        }

        public bool CreateIndex<TEntity>()
            where TEntity : RedisearchSerializable<TEntity>, new()
        {
            var database = _redisConnection.GetDatabase();
            var createIndexCommand = CreateIndexCommand.Create<TEntity>();

            return createIndexCommand.Execute(database);
        }

        public async Task<bool> CreateIndexAsync<TEntity>()
            where TEntity : RedisearchSerializable<TEntity>, new()
        {
            var database = _redisConnection.GetDatabase();
            var createIndexCommand = CreateIndexCommand.Create<TEntity>();

            return await createIndexCommand.ExecuteAsync(database).ConfigureAwait(false);
        }
    }
}