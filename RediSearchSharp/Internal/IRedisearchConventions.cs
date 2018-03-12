using System;
using StackExchange.Redis;

namespace RediSearchSharp.Internal
{
    public interface IRedisearchConventions
    {
        string GetIndexName<TEntity>();
        string GetDocumentIdPrefix<TEntity>();
        PrimaryKey GetPrimaryKey<TEntity>();
        string GetDefaultLanguage();
    }
}