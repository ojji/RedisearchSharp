using System;
using System.Linq.Expressions;
using RediSearchSharp.Serialization;

namespace RediSearchSharp.Query.Interfaces
{
    public interface IOptions<TEntity> 
        where TEntity : RedisearchSerializable<TEntity>, new()
    {   
        IOptions<TEntity> WithSlop(int slop);
        IOptions<TEntity> SortBy<TProperty>(Expression<Func<TEntity, TProperty>> propertySelector, SortingOrder order = SortingOrder.Ascending);
        IOptions<TEntity> Limit(int offset, int count);

        IOptions<TEntity> UseVerbatim();
        IOptions<TEntity> WithScores();
        IOptions<TEntity> WithScoreKeys();
        IOptions<TEntity> WithPayloads();
        IOptions<TEntity> WithoutStopwordFiltering();
        IOptions<TEntity> WithDefaultTermResolvingStrategy(TermResolvingStrategy termResolvingStrategy);
        IOptions<TEntity> InOrder();
        IOptions<TEntity> UseLanguage(string language);

        Query<TEntity> Build();
    }
}