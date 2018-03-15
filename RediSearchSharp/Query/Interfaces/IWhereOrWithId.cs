using System;
using System.Linq.Expressions;
using RediSearchSharp.Serialization;

namespace RediSearchSharp.Query.Interfaces
{
    public interface IWhereOrWithId<TEntity>
        where TEntity : RedisearchSerializable<TEntity>, new()
    {
        IWhereOrOptions<TEntity> WithId<TProperty>(TProperty id);
        IWhereOrOptions<TEntity> WithId<TProperty>(params TProperty[] ids);
        IMatching<TEntity> Where();
        IMatching<TEntity> Where<TProperty>(params Expression<Func<TEntity, TProperty>>[] propertySelectors);
    }
}