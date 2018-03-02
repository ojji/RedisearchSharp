using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using RediSearchSharp.Serialization;
using RediSearchSharp.Utils;

namespace RediSearchSharp.Internal
{
    public class SchemaInfoBuilder<TEntity>
        where TEntity: RedisearchSerializable<TEntity>, new()
    {
        private IRedisearchConventions _conventions;
        private string _indexName;
        private string _documentIdPrefix;
        private readonly Dictionary<string, PropertyInfoBuilder> _propertyInfos;
        private string _language;
        private PrimaryKeyBuilder _primaryKeyBuilder;

        public SchemaInfoBuilder()
        {
            _conventions = RedisearchConventions.DefaultConventions;
            _propertyInfos = new Dictionary<string, PropertyInfoBuilder>(GetAllProperties());
        }

        private IEnumerable<KeyValuePair<string, PropertyInfoBuilder>> GetAllProperties()
        {
            var getSetProperties = typeof(TEntity).GetProperties(BindingFlags.DeclaredOnly |
                                                     BindingFlags.Public |
                                                     BindingFlags.Instance |
                                                     BindingFlags.GetProperty |
                                                     BindingFlags.SetProperty)
                                            .Where(p => p.GetSetMethod() != null);

            return getSetProperties.Select(p =>
                new KeyValuePair<string, PropertyInfoBuilder>(p.Name, new PropertyInfoBuilder(p.Name)));
        }

        public void SetConventions(IRedisearchConventions conventions)
        {
            _conventions = conventions ?? throw new ArgumentNullException(nameof(conventions));
        }

        public void IndexName(string indexName)
        {
            if (string.IsNullOrWhiteSpace(indexName))
            {
                throw new ArgumentNullException(nameof(indexName));
            }

            _indexName = indexName;
        }

        public void PrimaryKey<TProperty>(Expression<Func<TEntity, TProperty>> propertySelector)
        {
            _primaryKeyBuilder = new PrimaryKeyBuilder(propertySelector.GetMemberName(), typeof(TProperty));
        }

        public void DocumentIdPrefix(string prefix)
        {
            if (string.IsNullOrWhiteSpace(prefix))
            {
                throw new ArgumentNullException(nameof(prefix));
            }

            _documentIdPrefix = prefix;
        }

        public void Language(string language)
        {
            if (string.IsNullOrWhiteSpace(language)) 
            {
                throw new ArgumentNullException(nameof(language));
            }

            _language = language;
        }

        public PropertyInfoBuilder Property(Expression<Func<TEntity, object>> propertySelector)
        {
            var propertyName = propertySelector.GetMemberName();
            if (!_propertyInfos.ContainsKey(propertyName))
            {
                throw new ArgumentException($"Property with name {propertyName} is not a valid property.");
            }

            return _propertyInfos[propertyName];
        }

        internal SchemaInfo<TEntity> Build()
        {
            var indexName = _indexName ?? _conventions.GetIndexName<TEntity>();
            var documentIdPrefix = _documentIdPrefix ?? _conventions.GetDocumentIdPrefix<TEntity>();
            var propertiesToSerialize = _propertyInfos.Where(pi => !pi.Value.IsIgnored)
                .Select(pi => pi.Key);
            var primaryKey = _primaryKeyBuilder?.Build<TEntity>() ?? _conventions.GetPrimaryKey<TEntity>();
            var language = _language ?? _conventions.GetDefaultLanguage();

            return new SchemaInfo<TEntity>(indexName, documentIdPrefix, propertiesToSerialize, primaryKey, language);
        }
    }
}