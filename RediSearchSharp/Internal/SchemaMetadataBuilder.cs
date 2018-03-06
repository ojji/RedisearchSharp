using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using RediSearchSharp.Serialization;
using RediSearchSharp.Utils;

namespace RediSearchSharp.Internal
{
    /// <summary>
    /// Provides a simple API to configure how RediSearch should handle an entity when 
    /// creating indexes, adding entities to the database or searching.
    /// </summary>
    /// <typeparam name="TEntity">An entity type.</typeparam>
    public class SchemaMetadataBuilder<TEntity>
        where TEntity: RedisearchSerializable<TEntity>, new()
    {
        private IRedisearchConventions _conventions;
        private string _indexName;
        private string _documentIdPrefix;
        private readonly Dictionary<string, PropertyMetadataBuilder> _propertyMetadataBuilders;
        private string _language;
        private PrimaryKeySelectorBuilder _primaryKeySelectorBuilder;

        internal SchemaMetadataBuilder()
        {
            _conventions = RedisearchConventions.DefaultConventions;
            _propertyMetadataBuilders = new Dictionary<string, PropertyMetadataBuilder>(GetAllProperties());
        }

        /// <summary>
        /// Sets a series of conventions used in the serialization and schema metadata creation process.
        /// </summary>
        /// <param name="conventions"></param>
        public void SetConventions(IRedisearchConventions conventions)
        {
            _conventions = conventions ?? throw new ArgumentNullException(nameof(conventions));
        }

        /// <summary>
        /// Sets the index name used by redisearch for the entity type.
        /// </summary>
        /// <param name="indexName">The redisearch index name.</param>
        public void IndexName(string indexName)
        {
            if (string.IsNullOrWhiteSpace(indexName))
            {
                throw new ArgumentNullException(nameof(indexName));
            }

            _indexName = indexName;
        }

        /// <summary>
        /// Sets the primary key property for the entity type.
        /// </summary>
        /// <typeparam name="TProperty">The type of the primary key property.</typeparam>
        /// <param name="propertySelector">Expression that selects the primary key property.</param>
        public void PrimaryKey<TProperty>(Expression<Func<TEntity, TProperty>> propertySelector)
        {
            _primaryKeySelectorBuilder = new PrimaryKeySelectorBuilder(propertySelector.GetMemberName(), typeof(TProperty));
        }

        /// <summary>
        /// Sets a prefix for the saved document ids. When adding a document 
        /// to the redisearch database the primary key will be in the form of {prefix}{id}.
        /// </summary>
        /// <param name="prefix">The id prefix for the entity type.</param>
        public void DocumentIdPrefix(string prefix)
        {
            if (string.IsNullOrWhiteSpace(prefix))
            {
                throw new ArgumentNullException(nameof(prefix));
            }

            _documentIdPrefix = prefix;
        }

        /// <summary>
        /// Sets a default language for the entity type. This can be overridden when adding
        /// individual documents. For the supported languages check out the
        /// <see cref="RediSearchSharp.Query.Languages"/> type.
        /// </summary>
        /// <param name="language">The default language used by the indexer for the entity type.</param>
        public void Language(string language)
        {
            if (string.IsNullOrWhiteSpace(language)) 
            {
                throw new ArgumentNullException(nameof(language));
            }

            _language = language;
        }

        /// <summary>
        /// Returns an object that can be used to configure a property of the entity.
        /// </summary>
        /// <param name="propertySelector">The property selector.</param>
        /// <returns>An object to configure the property.</returns>
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
    }
}