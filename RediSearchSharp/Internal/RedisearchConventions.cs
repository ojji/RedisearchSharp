using System;
using System.Linq;
using System.Reflection;
using StackExchange.Redis;

namespace RediSearchSharp.Internal
{
    public class RedisearchConventions : IRedisearchConventions
    {
        public static IRedisearchConventions DefaultConventions => new RedisearchConventions();

        public virtual string GetIndexName<TEntity>()
        {
            return $"{Pluralize(typeof(TEntity).Name)}-index";
        }

        public virtual string GetDocumentIdPrefix<TEntity>()
        {
            return $"{Pluralize(typeof(TEntity).Name)}:";
        }

        public virtual Func<TEntity, RedisValue> GetPrimaryKey<TEntity>()
        {
            var getSetProperties = typeof(TEntity).GetProperties(BindingFlags.DeclaredOnly |
                                                     BindingFlags.Public |
                                                     BindingFlags.Instance |
                                                     BindingFlags.GetProperty |
                                                     BindingFlags.SetProperty)
                                            .Where(p => p.GetSetMethod() != null);
            var idProperty = getSetProperties.FirstOrDefault(pi => pi.Name == "Id" || pi.Name == string.Concat(typeof(TEntity).Name, "Id"));

            if (idProperty == null)
            {
                throw new ArgumentException("Could not find a default id property, please specify one.");
            }

            return new PrimaryKeyBuilder(idProperty.Name, idProperty.PropertyType).Build<TEntity>();
        }

        private static string Pluralize(string tableName)
        {
            if (!tableName.EndsWith("s"))
            {
                return string.Format($"{tableName.ToLowerInvariant()}s");
            }

            return tableName.ToLowerInvariant();
        }
    }
}