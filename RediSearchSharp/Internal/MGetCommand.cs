using System.Collections.Generic;
using System.Threading.Tasks;
using RediSearchSharp.Query;
using RediSearchSharp.Serialization;
using StackExchange.Redis;

namespace RediSearchSharp.Internal
{
    internal class MGetCommand : RetrieveEntitiesCommand
    {
        internal MGetCommand(List<object> arguments) : base("FT.MGET", arguments)
        {
        }

        public override IEnumerable<SearchResult<TEntity>> Retrieve<TEntity>(IDatabase database, IRedisearchSerializer serializer)
        {
            var response = (RedisResult[]) database.Execute(Command, Arguments);
            return SearchResult<TEntity>.LoadMGetResults(serializer, response);
        }

        public override async Task<IEnumerable<SearchResult<TEntity>>> RetrieveAsync<TEntity>(IDatabase database, IRedisearchSerializer serializer)
        {
            var response = (RedisResult[]) await database.ExecuteAsync(Command, Arguments);
            return SearchResult<TEntity>.LoadMGetResults(serializer, response);
        }
    }
}