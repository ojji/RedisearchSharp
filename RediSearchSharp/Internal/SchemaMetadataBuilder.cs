using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using RediSearchSharp.Serialization;
using RediSearchSharp.Utils;

namespace RediSearchSharp.Internal
{
    public class SchemaMetadataBuilder<TEntity>
        where TEntity: RedisearchSerializable<TEntity>, new()
    {
        private IRedisearchConventions _conventions;
        private string _indexName;
        private string _documentIdPrefix;
        private readonly Dictionary<string, PropertyMetadataBuilder> _propertyMetadataBuilders;
        private string _language;
        private PrimaryKeySelectorBuilder _primaryKeySelectorBuilder;

        public SchemaMetadataBuilder()
        {
            _conventions = RedisearchConventions.DefaultConventions;
            _propertyMetadataBuilders = new Dictionary<string, PropertyMetadataBuilder>(GetAllProperties());
        }

        private IEnumerable<KeyValuePair<string, PropertyMetadataBuilder>> GetAllProperties()
        {
            var getSetProperties = typeof(TEntity).GetProperties(BindingFlags.DeclaredOnly |
                                                     BindingFlags.Public |
                                                     BindingFlags.Instance |
                                                     BindingFlags.GetProperty |
                                                     BindingFlags.SetProperty)
                                            .Where(p => p.GetSetMethod() != null);

            return getSetProperties.Select(p =>
                new KeyValuePair<string, PropertyMetadataBuilder>(p.Name, new PropertyMetadataBuilder(p.Name, p.PropertyType)));
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
            _primaryKeySelectorBuilder = new PrimaryKeySelectorBuilder(propertySelector.GetMemberName(), typeof(TProperty));
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

        public PropertyMetadataBuilder Property(Expression<Func<TEntity, object>> propertySelector)
        {
            var propertyName = propertySelector.GetMemberName();
            if (!_propertyMetadataBuilders.ContainsKey(propertyName))
            {
                throw new ArgumentException($"Property with name {propertyName} is not a valid property.");
            }

            return _propertyMetadataBuilders[propertyName];
        }

        internal SchemaMetadata<TEntity> Build()
        {
            var indexName = _indexName ?? _conventions.GetIndexName<TEntity>();
            var documentIdPrefix = _documentIdPrefix ?? _conventions.GetDocumentIdPrefix<TEntity>();
            var propertyMetadata = _propertyMetadataBuilders.Select(pmb => pmb.Value.Build()).ToArray();
            var primaryKey = _primaryKeySelectorBuilder?.Build<TEntity>() ?? _conventions.GetPrimaryKey<TEntity>();
            var language = _language ?? _conventions.GetDefaultLanguage();

            return new SchemaMetadata<TEntity>(indexName, documentIdPrefix, propertyMetadata, primaryKey, language);
        }
    }
}