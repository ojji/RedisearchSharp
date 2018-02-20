using System.Collections.Concurrent;
using StackExchange.Redis;

namespace RediSearchSharp.Utils
{
    public static class RedisearchIndexCache
    {
        private static ConcurrentDictionary<string, object> IndexNameCache =>
            new ConcurrentDictionary<string, object>();
        private static ConcurrentDictionary<string, object> LiteralCache =>
            new ConcurrentDictionary<string, object>();

        public static object GetBoxedIndexName(string indexName)
        {
            return IndexNameCache.GetOrAdd(indexName, (RedisValue) indexName);
        }

        public static object GetBoxedLiteral(string literal)
        {
            return IndexNameCache.GetOrAdd(literal, (RedisValue)literal);
        }
    }
}