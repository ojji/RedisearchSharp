using System.Collections.Concurrent;
using StackExchange.Redis;

namespace RediSearchSharp.Utils
{
    public static class RedisearchIndexCache
    {
        private static ConcurrentDictionary<string, RedisValue> IndexNameCache =>
            new ConcurrentDictionary<string, RedisValue>();
        private static ConcurrentDictionary<string, RedisValue> LiteralCache =>
            new ConcurrentDictionary<string, RedisValue>();

        public static RedisValue GetBoxedIndexName(string indexName)
        {
            return IndexNameCache.GetOrAdd(indexName, (RedisValue) indexName);
        }

        public static RedisValue GetBoxedLiteral(string literal)
        {
            return IndexNameCache.GetOrAdd(literal, (RedisValue)literal);
        }
    }
}