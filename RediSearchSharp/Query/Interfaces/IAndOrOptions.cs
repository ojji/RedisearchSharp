using System;
using System.Linq.Expressions;
using RediSearchSharp.Serialization;

namespace RediSearchSharp.Query.Interfaces
{
    public interface IAndOrOptions<TEntity> : IMatching<TEntity>, IOptions<TEntity> 
        where TEntity : RedisearchSerializable<TEntity>, new()
    {
        IMatching<TEntity> And();
        IMatching<TEntity> And<TProperty>(params Expression<Func<TEntity, TProperty>>[] propertySelectors);
    }
}