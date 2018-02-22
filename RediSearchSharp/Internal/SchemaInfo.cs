using System;
using System.Collections.Concurrent;
using RediSearchSharp.Utils;
using StackExchange.Redis;

namespace RediSearchSharp.Internal
{
    public class SchemaInfo
    {
        public RedisValue IndexName { get; private set; }
        public RedisValue DocumentIdPrefix { get; private set; }

        public static ConcurrentDictionary<Type, SchemaInfo> SchemaInfos = new ConcurrentDictionary<Type, SchemaInfo>();

        public static SchemaInfo GetSchemaInfo<TEntity>()
        {
            return SchemaInfos.GetOrAdd(
                typeof(TEntity),
                new SchemaInfo
                {
                    IndexName = GetIndexName<TEntity>(),
                    DocumentIdPrefix = GetDocumentIdPrefix<TEntity>()
                }
            );
        }

        private static RedisValue GetIndexName<TEntity>()
        {
            return RedisearchIndexCache.GetBoxedIndexName($"{Pluralize(typeof(TEntity).Name)}-index");
        }

        private static RedisValue GetDocumentIdPrefix<TEntity>()
        {
            return RedisearchIndexCache.GetBoxedLiteral($"{Pluralize(typeof(TEntity).Name)}:");
        }
        
        private static string Pluralize(string tableName)
        {
            if (!tableName.EndsWith("s"))
            {
                return string.Format($"{tableName.ToLowerInvariant()}s");
            }

            return tableName.ToLowerInvariant();
        }
    }
}