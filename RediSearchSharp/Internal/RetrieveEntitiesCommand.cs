using System.Collections.Generic;
using System.Threading.Tasks;
using RediSearchSharp.Query;
using RediSearchSharp.Serialization;
using StackExchange.Redis;

namespace RediSearchSharp.Internal
{
    internal abstract class RetrieveEntitiesCommand
    {
        public string Command { get; }
        public List<object> Arguments { get; }

        protected RetrieveEntitiesCommand(string command, List<object> arguments)
        {
            Command = command;
            Arguments = arguments;
        }

        public static RetrieveEntitiesCommand MGet(List<object> arguments)
        {
            return new MGetCommand(arguments);
        }

        public static RetrieveEntitiesCommand Search(List<object> arguments, bool withScores, bool withPayloads)
        {
            return new SearchCommand(arguments, withScores, withPayloads);
        }

        public abstract IEnumerable<SearchResult<TEntity>> Retrieve<TEntity>(IDatabase database, IRedisearchSerializer serializer) 
            where TEntity : RedisearchSerializable<TEntity>, new();

        public abstract Task<IEnumerable<SearchResult<TEntity>>> RetrieveAsync<TEntity>(IDatabase database, IRedisearchSerializer serializer)
            where TEntity : RedisearchSerializable<TEntity>, new();
    }
}