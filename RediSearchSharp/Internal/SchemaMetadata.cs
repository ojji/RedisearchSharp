﻿using System;
using System.Collections.Concurrent;
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
        public PrimaryKey PrimaryKey { get; }

        private static readonly ConcurrentDictionary<Type, SchemaMetadata<TEntity>> SchemaInfos = new ConcurrentDictionary<Type, SchemaMetadata<TEntity>>();
        
        internal SchemaMetadata(string indexName, string documentIdPrefix, PropertyMetadata[] properties, PrimaryKey primaryKey, string language)
        {
            IndexName = RedisearchIndexCache.GetBoxedIndexName(indexName);
            DocumentIdPrefix = RedisearchIndexCache.GetBoxedLiteral(documentIdPrefix);
            Properties = properties;
            PrimaryKey = primaryKey;
            Language = RedisearchIndexCache.GetBoxedLiteral(language);
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