using System.Collections.Generic;
using System.Threading.Tasks;
using RediSearchSharp.Query;
using RediSearchSharp.Serialization;
using StackExchange.Redis;

namespace RediSearchSharp.Internal
{
    internal class SearchCommand : RetrieveEntitiesCommand
    {
        private bool WithScores { get; }
        private bool WithPayloads { get; }

        public SearchCommand(List<object> arguments, bool withScores, bool withPayloads) : base("FT.SEARCH", arguments)
        {
            WithScores = withScores;
            WithPayloads = withPayloads;
        }

        public override IEnumerable<SearchResult<TEntity>> Retrieve<TEntity>(IDatabase database, IRedisearchSerializer serializer)
        {
            var response = (RedisResult[]) database.Execute(Command, Arguments);

            return SearchResult<TEntity>.LoadSearchResults(
                serializer,
                response,
                WithScores,
                WithPayloads);
        }

        public override async Task<IEnumerable<SearchResult<TEntity>>> RetrieveAsync<TEntity>(IDatabase database, IRedisearchSerializer serializer)
        {
            var response = (RedisResult[]) await database.ExecuteAsync(Command, Arguments);

            return SearchResult<TEntity>.LoadSearchResults(
                serializer,
                response,
                WithScores,
                WithPayloads);
        }
    }
}