using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using RediSearchSharp.Serialization;
using RediSearchSharp.Utils;
using StackExchange.Redis;

namespace RediSearchSharp.Internal
{
    public class SchemaMetadata<TEntity>
        where TEntity : RedisearchSerializable<TEntity>, new()
    {
        public PropertyMetadata[] Properties { get; }
        public RedisValue IndexName { get; }
        public RedisValue DocumentIdPrefix { get; }
        public RedisValue Language { get; }
        private PrimaryKey PrimaryKey { get; }

        private static readonly ConcurrentDictionary<Type, SchemaMetadata<TEntity>> SchemaInfos = new ConcurrentDictionary<Type, SchemaMetadata<TEntity>>();
        private object _primaryKeyFromEntityFunc;
        private object _primaryKeyFromPropertyFunc;

        internal SchemaMetadata(string indexName, string documentIdPrefix, PropertyMetadata[] properties, PrimaryKey primaryKey, string language)
        {
            IndexName = RedisearchIndexCache.GetBoxedIndexName(indexName);
            DocumentIdPrefix = RedisearchIndexCache.GetBoxedLiteral(documentIdPrefix);
            Properties = properties;
            PrimaryKey = primaryKey;
            Language = RedisearchIndexCache.GetBoxedLiteral(language);
        }

        public Func<TEntity, RedisValue> GetPrimaryKeySelectorFromEntity()
        {
            if (_primaryKeyFromEntityFunc == null)
            {
                _primaryKeyFromEntityFunc = ((Expression<Func<TEntity, RedisValue>>)PrimaryKey.GetPrimaryKeyFromEntity).Compile();
            }
            return (Func<TEntity, RedisValue>) _primaryKeyFromEntityFunc;
        }

        public Func<TProperty, RedisValue> GetPrimaryKeySelectorFromProperty<TProperty>()
        {
            if (_primaryKeyFromPropertyFunc == null)
            {
                _primaryKeyFromPropertyFunc = ((Expression<Func<TProperty, RedisValue>>)PrimaryKey.GetPrimaryKeyFromIdProperty).Compile();
            }
            return (Func<TProperty, RedisValue>) _primaryKeyFromPropertyFunc;
        }

        public static SchemaMetadata<TEntity> GetSchemaMetadata()
        {
            return SchemaInfos.GetOrAdd(typeof(TEntity), t =>
                {
                    var defaultEntity = new TEntity();
                    var builder = new SchemaMetadataBuilder<TEntity>();
                    defaultEntity.OnCreatingSchemaInfo(builder);
                    return builder.Build();
                });
        }
    }
}