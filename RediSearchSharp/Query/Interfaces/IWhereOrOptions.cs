using System;
using System.Linq.Expressions;
using RediSearchSharp.Serialization;

namespace RediSearchSharp.Query.Interfaces
{
    public interface IWhereOrOptions<TEntity> : IOptions<TEntity>
        where TEntity : RedisearchSerializable<TEntity>, new()
    {
        IMatching<TEntity> Where();
        IMatching<TEntity> Where<TProperty>(params Expression<Func<TEntity, TProperty>>[] propertySelectors);
    }
}