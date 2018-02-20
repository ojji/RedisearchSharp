using RediSearchSharp.Internal;

namespace RediSearchSharp.Serialization
{
    public interface IRedisearchSerializer
    {
        Document Serialize<TEntity>(TEntity entity, double score)
            where TEntity : class, IRedisearchSerializable<TEntity>, new();

        TEntity Deserialize<TEntity>(Document document)
            where TEntity : class, IRedisearchSerializable<TEntity>, new();
    }
}