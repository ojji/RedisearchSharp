using System.Collections.Generic;
using System.Threading.Tasks;
using RediSearchSharp.Query;
using RediSearchSharp.Serialization;
using StackExchange.Redis;

namespace RediSearchSharp.Commands
{
    public abstract class SearchCommand
    {
        private string Command { get; }
        private List<object> Arguments { get; }

        private SearchCommand(string command, List<object> arguments)
        {
            Command = command;
            Arguments = arguments;
        }

        public static SearchCommand Create<TEntity>(Query<TEntity> query) 
            where TEntity : RedisearchSerializable<TEntity>, new()
        {
            if (query.IsMGetQuery)
            {
                return new MGetCommand(query.CreateMGetArgs());
            }

            return new PlainSearchCommand(query.CreateSearchArgs(), query.Options.WithScores, query.Options.WithPayloads);
        }

        public abstract IEnumerable<SearchResult<TEntity>> Execute<TEntity>(IDatabase database, IRedisearchSerializer serializer) 
            where TEntity : RedisearchSerializable<TEntity>, new();

        public abstract Task<IEnumerable<SearchResult<TEntity>>> ExecuteAsync<TEntity>(IDatabase database, IRedisearchSerializer serializer)
            where TEntity : RedisearchSerializable<TEntity>, new();

        private class MGetCommand : SearchCommand
        {
            public MGetCommand(List<object> arguments) : base("FT.MGET", arguments)
            {
            }

            public override IEnumerable<SearchResult<TEntity>> Execute<TEntity>(IDatabase database, IRedisearchSerializer serializer)
            {
                var response = (RedisResult[])database.Execute(Command, Arguments);
                return SearchResult<TEntity>.LoadMGetResults(serializer, response);
            }

            public override async Task<IEnumerable<SearchResult<TEntity>>> ExecuteAsync<TEntity>(IDatabase database, IRedisearchSerializer serializer)
            {
                var response = (RedisResult[])await database.ExecuteAsync(Command, Arguments).ConfigureAwait(false);
                return SearchResult<TEntity>.LoadMGetResults(serializer, response);
            }
        }

        private class PlainSearchCommand : SearchCommand
        {
            private bool WithScores { get; }
            private bool WithPayloads { get; }

            public PlainSearchCommand(List<object> arguments, bool withScores, bool withPayloads) : base("FT.SEARCH", arguments)
            {
                WithScores = withScores;
                WithPayloads = withPayloads;
            }

            public override IEnumerable<SearchResult<TEntity>> Execute<TEntity>(IDatabase database, IRedisearchSerializer serializer)
            {
                var response = (RedisResult[])database.Execute(Command, Arguments);

                return SearchResult<TEntity>.LoadSearchResults(
                    serializer,
                    response,
                    WithScores,
                    WithPayloads);
            }

            public override async Task<IEnumerable<SearchResult<TEntity>>> ExecuteAsync<TEntity>(IDatabase database, IRedisearchSerializer serializer)
            {
                var response = (RedisResult[])await database.ExecuteAsync(Command, Arguments).ConfigureAwait(false);

                return SearchResult<TEntity>.LoadSearchResults(
                    serializer,
                    response,
                    WithScores,
                    WithPayloads);
            }
        }
    }
}