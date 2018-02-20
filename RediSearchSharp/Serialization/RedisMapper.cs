using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using FastMember;
using StackExchange.Redis;

namespace RediSearchSharp.Serialization
{
    public static class RedisMapper
    {
        private static readonly Dictionary<Type, Dictionary<string, Type>> RegisteredTypes = new Dictionary<Type, Dictionary<string, Type>>();
        private static readonly Dictionary<Type, Func<object, RedisValue>> ToRedisValueConverters = new Dictionary<Type, Func<object, RedisValue>>
        {
            { typeof(int), o => (int)o },
            { typeof(float), o => (float)o },
            { typeof(double), o => (double)o },
            { typeof(bool), o => (bool)o },
            { typeof(string), o => (string)o },
            { typeof(long), o => (long)o },
            { typeof(byte[]), o => (byte[])o },
            { typeof(DateTime), o => ((DateTime)o).ToString("O") },
            { typeof(Guid), o => o.ToString() },
            { typeof(GeoPosition), o =>
                    string.Join(',',
                        ((GeoPosition)o).Longitude.ToString("G17", CultureInfo.InvariantCulture),
                        ((GeoPosition)o).Latitude.ToString("G17", CultureInfo.InvariantCulture))
            }
        };
        private static readonly Dictionary<Type, Func<RedisValue, object>> RedisConverters = new Dictionary<Type, Func<RedisValue, object>>
        {
            { typeof(int), o => (int)o },
            { typeof(float), o => (float)o },
            { typeof(double), o => (double)o },
            { typeof(bool), o => (bool)o },
            { typeof(string), o => (string)o },
            { typeof(long), o => (long)o },
            { typeof(byte[]), o => (byte[])o },
            { typeof(DateTime), o => DateTime.Parse(o) },
            { typeof(Guid), o => new Guid(o.ToString()) },
            { typeof(GeoPosition), o =>
                {
                    var split = o.ToString().Split(',');
                    return new GeoPosition(
                        double.Parse(split[0], CultureInfo.InvariantCulture), 
                        double.Parse(split[1], CultureInfo.InvariantCulture));
                }
            }
        };

        public static void UnregisterAll()
        {
            RegisteredTypes.Clear();
        }

        public static void RegisterType<T>(bool throwOnUnsupportedProperty = true)
        {
            var getSetProperties = typeof(T).GetProperties(BindingFlags.DeclaredOnly |
                                                     BindingFlags.Public |
                                                     BindingFlags.Instance |
                                                     BindingFlags.GetProperty |
                                                     BindingFlags.SetProperty)
                                            .Where(p => p.GetSetMethod() != null);
            RegisterType<T>(getSetProperties.Select(p => p.Name).ToArray(), throwOnUnsupportedProperty);
        }

        public static void RegisterType<T>(string[] properties, bool throwOnUnsupportedProperty = true)
        {
            var typeProperties = new Dictionary<string, Type>();
            foreach (var property in properties)
            {
                Type propertyType = typeof(T).GetProperty(property).PropertyType;
                if (throwOnUnsupportedProperty &&
                    !ImplementsICollectionOfType<string>(propertyType) &&
                    !ImplementsICollectionOfType<Guid>(propertyType) &&
                    !RedisConverters.ContainsKey(propertyType))
                {
                    throw new ArgumentException("Unsupported property type detected on property: {0}", property);
                }
                typeProperties.Add(property, propertyType);
            }

            RegisteredTypes.Add(typeof(T), typeProperties);
        }

        public static Dictionary<string, RedisValue> MapToRedisValues<TEntity>(TEntity entity)
        {
            var objectType = typeof(TEntity);
            if (!IsRegisteredType<TEntity>())
                throw new ArgumentException(
                    "There is no mapping defined for this object. Use the RedisMapper.RegisterType() method to do so.");

            var objectAccessor = ObjectAccessor.Create(entity);
            var hashEntries = new Dictionary<string, RedisValue>();
            foreach (var properties in RegisteredTypes[objectType])
            {
                var hashKey = properties.Key;
                RedisValue hashValue;
                if (ImplementsICollectionOfType<string>(properties.Value))
                {
                    hashValue = StringCollectionToRedisValue((ICollection<string>)objectAccessor[properties.Key]);
                }
                else if (ImplementsICollectionOfType<Guid>(properties.Value))
                {
                    hashValue = GuidCollectionToRedisValue((ICollection<Guid>)objectAccessor[properties.Key]);
                }
                else
                {
                    hashValue = ToRedisValueConverters[properties.Value](objectAccessor[properties.Key]);
                }
                hashEntries.Add(hashKey, hashValue);
            }

            return hashEntries;
        }

        private static RedisValue StringCollectionToRedisValue(ICollection<string> stringCollection)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(stringCollection.GetType().AssemblyQualifiedName);
            sb.Append("||");

            foreach (var str in stringCollection)
            {
                sb.Append(str.Replace("|", @"\|"));
                sb.Append("||");
            }
            return sb.ToString(0, sb.Length - 2);
        }

        private static RedisValue GuidCollectionToRedisValue(ICollection<Guid> guidCollection)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(guidCollection.GetType().AssemblyQualifiedName);
            sb.Append("||");

            foreach (var guid in guidCollection)
            {
                sb.Append(guid);
                sb.Append("||");
            }
            return sb.ToString(0, sb.Length - 2);
        }
        
        private static ICollection<T> RedisValueToCollection<T>(string redisValue, Func<string, T> elementConverter)
        {
            var splitString = redisValue.Split(new[] { "||" }, StringSplitOptions.None);

            string typeName = splitString[0];

            var collection = (ICollection<T>)Activator.CreateInstance(Type.GetType(typeName));

            for (int i = 1; i < splitString.Length; i++)
            {
                collection.Add(elementConverter(splitString[i]));
            }
            return collection;
        }

        private static bool ImplementsICollectionOfType<TElementType>(Type t)
        {
            return t.IsConstructedGenericType && t.GetGenericTypeDefinition() == typeof(ICollection<>) && t.GenericTypeArguments[0] == typeof(TElementType) ||
                t.GetInterfaces().Any(x =>
                x.IsConstructedGenericType &&
                    x.GetGenericTypeDefinition() == typeof(ICollection<>) &&
                    x.GenericTypeArguments[0] == typeof(TElementType)
            );
        }

        public static T FromRedisValues<T>(Dictionary<string, RedisValue> fields) where T : new()
        {
            var objectType = typeof(T);
            if (!IsRegisteredType<T>())
            {
                throw new ArgumentException("There is no mapping defined for this object. Use the RedisMapper.RegisterType() method to do so.");
            }

            var objectAccessor = TypeAccessor.Create(objectType);
            T objectRead = new T();

            foreach (var hashEntry in fields)
            {
                var propertyType = RegisteredTypes[typeof(T)][hashEntry.Key];
                if (ImplementsICollectionOfType<string>(propertyType))
                {
                    objectAccessor[objectRead, hashEntry.Key] =
                        RedisValueToCollection(hashEntry.Value,
                            elem =>
                                elem.Replace(@"\|", "|"));
                }
                else if (ImplementsICollectionOfType<Guid>(propertyType))
                {
                    objectAccessor[objectRead, hashEntry.Key] = RedisValueToCollection(hashEntry.Value, Guid.Parse);
                }
                else
                {
                    objectAccessor[objectRead, hashEntry.Key] = RedisConverters[propertyType](hashEntry.Value);
                }
            }

            return objectRead;
        }

        public static bool IsRegisteredType<T>()
        {
            var entityType = typeof(T);
            return RegisteredTypes.ContainsKey(entityType);
        }
    }
}
