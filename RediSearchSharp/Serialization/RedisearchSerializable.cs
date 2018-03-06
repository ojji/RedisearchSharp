using RediSearchSharp.Internal;

namespace RediSearchSharp.Serialization
{
    public abstract class RedisearchSerializable<TEntity>
        where TEntity : RedisearchSerializable<TEntity>, new()
    {
        protected internal virtual void OnCreatingSchemaInfo(SchemaMetadataBuilder<TEntity> builder)
        {
        }
    }
}