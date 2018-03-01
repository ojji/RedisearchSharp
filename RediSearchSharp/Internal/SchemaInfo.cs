using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using RediSearchSharp.Serialization;
using RediSearchSharp.Utils;
using StackExchange.Redis;

namespace RediSearchSharp.Internal
{
    public class SchemaInfo<TEntity>
        where TEntity : RedisearchSerializable<TEntity>, new()
    {
        public IEnumerable<string> PropertiesToSerialize { get; }
        public RedisValue IndexName { get; }
        public RedisValue DocumentIdPrefix { get; }
        public Func<TEntity, RedisValue> PrimaryKeySelector { get; }

        private static readonly ConcurrentDictionary<Type, SchemaInfo<TEntity>> SchemaInfos = new ConcurrentDictionary<Type, SchemaInfo<TEntity>>();

        internal SchemaInfo(string indexName, string documentIdPrefix, IEnumerable<string> propertiesToSerialize, Func<TEntity, RedisValue> primaryKeySelector)
        {
            IndexName = RedisearchIndexCache.GetBoxedIndexName(indexName);
            DocumentIdPrefix = RedisearchIndexCache.GetBoxedLiteral(documentIdPrefix);
            PropertiesToSerialize = propertiesToSerialize;
            PrimaryKeySelector = primaryKeySelector;
        }

        public static SchemaInfo<TEntity> GetSchemaInfo()
        {
            return SchemaInfos.GetOrAdd(typeof(TEntity), t =>
                {
                    var defaultEntity = new TEntity();
                    var builder = new SchemaInfoBuilder<TEntity>();
                    defaultEntity.OnCreatingSchemaInfo(builder);
                    return builder.Build();
                });
        }
    }
}