using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RediSearchSharp.Internal;
using RediSearchSharp.Serialization;
using RediSearchSharp.Utils;
using StackExchange.Redis;

namespace RediSearchSharp.Commands
{
    public class CreateIndexCommand
    {
        private List<object> Parameters { get; }

        private CreateIndexCommand(List<object> parameters)
        {
            Parameters = parameters;
        }

        public static CreateIndexCommand Create<TEntity>()
            where TEntity : RedisearchSerializable<TEntity>, new()
        {
            var schemaMetadata = SchemaMetadata<TEntity>.GetSchemaMetadata();

            var parameters = new List<object>
            {
                RedisearchIndexCache.GetBoxedIndexName(schemaMetadata.IndexName),
                RedisearchIndexCache.GetBoxedLiteral("SCHEMA")
            };

            foreach (var propertyMetadata in schemaMetadata.Properties.Where(pm => !pm.IsIgnored))
            {
                AddPropertyParametersTo(parameters, propertyMetadata);
            }

            return new CreateIndexCommand(parameters);
        }

        public bool Execute(IDatabase db)
        {
            try
            {
                return (string) db.Execute("FT.CREATE", Parameters) == "OK";
            }
            catch (RedisServerException)
            {
                return false;
            }
        }

        public async Task<bool> ExecuteAsync(IDatabase db)
        {
            try
            {
                return (string) await db.ExecuteAsync("FT.CREATE", Parameters).ConfigureAwait(false) == "OK";
            }
            catch (RedisServerException)
            {
                return false;
            }
        }

        private static void AddPropertyParametersTo(List<object> parameters, PropertyMetadata propertyMetadata)
        {
            parameters.Add(propertyMetadata.PropertyName);
            switch (propertyMetadata.RedisearchType)
            {
                case RedisearchPropertyType.Fulltext:
                    parameters.Add(RedisearchIndexCache.GetBoxedLiteral("TEXT"));
                    if (propertyMetadata.NoStem)
                    {
                        parameters.Add(RedisearchIndexCache.GetBoxedLiteral("NOSTEM"));
                    }
                    parameters.Add(RedisearchIndexCache.GetBoxedLiteral("WEIGHT"));
                    parameters.Add((RedisValue)propertyMetadata.Weight);
                    if (propertyMetadata.Sortable)
                    {
                        parameters.Add(RedisearchIndexCache.GetBoxedLiteral("SORTABLE"));
                    }
                    break;
                case RedisearchPropertyType.Numeric:
                    parameters.Add(RedisearchIndexCache.GetBoxedLiteral("NUMERIC"));
                    if (propertyMetadata.Sortable)
                    {
                        parameters.Add(RedisearchIndexCache.GetBoxedLiteral("SORTABLE"));
                    }
                    break;
                case RedisearchPropertyType.Geo:
                    parameters.Add(RedisearchIndexCache.GetBoxedLiteral("GEO"));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (propertyMetadata.NotIndexed)
            {
                parameters.Add(RedisearchIndexCache.GetBoxedLiteral("NOINDEX"));
            }
        }
    }
}