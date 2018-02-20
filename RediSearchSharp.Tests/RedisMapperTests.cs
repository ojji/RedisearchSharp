using System;
using System.Collections.Generic;
using RediSearchSharp.Serialization;
using StackExchange.Redis;
using Xunit;

namespace RediSearchSharp.Tests
{
    public class TestType1
    {
        public int IntegerProperty { get; set; }
        public long LongProperty { get; set; }
        public float FloatProperty { get; set; }
        public double DoubleProperty { get; set; }
        public bool BoolProperty { get; set; }
        public string StringProperty { get; set; }
        public byte[] ByteArrayProperty { get; set; }
        public DateTime DateTimeProperty { get; set; }
        public Guid GuidProperty { get; set; }
        public GeoPosition GeoPositionProperty { get; set; }
    }

    public class UnsupportedPropertyTestType1
    {
        public int IntegerProperty { get; set; }
        public object UnsupportedProperty { get; set; }
    }

    public class UnsupportedPropertyTestType2
    {
        public int IntegerProperty { get; set; }
        public ICollection<int> UnsupportedProperty { get; set; }
    }

    public class CollectionOfStringsTestType1
    {
        public ICollection<string> StringCollection { get; set; }
    }

    public class CollectionOfStringsTestType2
    {
        public List<string> StringCollection { get; set; }
    }

    public class CollectionOfGuidTestType1
    {
        public ICollection<Guid> GuidCollection { get; set; }
    }

    public class CollectionOfGuidTestType2
    {
        public List<Guid> GuidCollection { get; set; }
    }

    public class RedisMapperTests : IDisposable
    {
        public void Dispose()
        {
            RedisMapper.UnregisterAll();
        }

        [Fact]
        public void RegisterType_should_register_public_get_set_properties()
        {
            RedisMapper.RegisterType<TestType1>();
            Assert.Equal(10, RedisMapper.TypesWithRegisteredProperties[typeof(TestType1)].Count);
        }

        [Fact]
        public void RegisterType_should_register_ICollection_of_string_properties()
        {
            RedisMapper.RegisterType<CollectionOfStringsTestType1>();
            Assert.Contains(RedisMapper.TypesWithRegisteredProperties[typeof(CollectionOfStringsTestType1)],
                props => props.Key == "StringCollection" && props.Value == typeof(ICollection<string>));

            RedisMapper.RegisterType<CollectionOfStringsTestType2>();
            Assert.Contains(RedisMapper.TypesWithRegisteredProperties[typeof(CollectionOfStringsTestType2)],
                props => props.Key == "StringCollection" && props.Value == typeof(List<string>));
        }

        [Fact]
        public void RegisterType_should_register_ICollection_of_Guid_properties()
        {
            RedisMapper.RegisterType<CollectionOfGuidTestType1>();
            Assert.Contains(RedisMapper.TypesWithRegisteredProperties[typeof(CollectionOfGuidTestType1)],
                props => props.Key == "GuidCollection" && props.Value == typeof(ICollection<Guid>));

            RedisMapper.RegisterType<CollectionOfGuidTestType2>();
            Assert.Contains(RedisMapper.TypesWithRegisteredProperties[typeof(CollectionOfGuidTestType2)],
                props => props.Key == "GuidCollection" && props.Value == typeof(List<Guid>));
        }

        [Fact]
        public void RegisterType_should_throw_by_default_when_an_unsupported_type_is_detected()
        {
            Assert.Throws<ArgumentException>(() => RedisMapper.RegisterType<UnsupportedPropertyTestType1>());
            Assert.Throws<ArgumentException>(() => RedisMapper.RegisterType<UnsupportedPropertyTestType2>());
        }

        [Fact]
        public void ToRedis_should_be_able_to_handle_an_empty_collection_of_strings()
        {
            RedisMapper.RegisterType<CollectionOfStringsTestType1>();
            Assert.Single(RedisMapper.TypesWithRegisteredProperties[typeof(CollectionOfStringsTestType1)]);

            RedisMapper.RegisterType<CollectionOfStringsTestType2>();
            Assert.Single(RedisMapper.TypesWithRegisteredProperties[typeof(CollectionOfStringsTestType2)]);

            var subject = new CollectionOfStringsTestType2
            {
                StringCollection = new List<string>()
            };
            
            var result = RedisMapper.MapToRedisValues(subject);
            Assert.Equal($"{typeof(List<string>).AssemblyQualifiedName}", result["StringCollection"]);
        }

        [Fact]
        public void ToRedis_should_be_able_to_handle_an_empty_collection_of_Guids()
        {
            RedisMapper.RegisterType<CollectionOfGuidTestType1>();
            Assert.Single(RedisMapper.TypesWithRegisteredProperties[typeof(CollectionOfGuidTestType1)]);

            RedisMapper.RegisterType<CollectionOfGuidTestType2>();
            Assert.Single(RedisMapper.TypesWithRegisteredProperties[typeof(CollectionOfGuidTestType2)]);

            var subject = new CollectionOfGuidTestType2
            {
                GuidCollection = new List<Guid>()
            };

            var result = RedisMapper.MapToRedisValues(subject);
            Assert.Equal($"{typeof(List<Guid>).AssemblyQualifiedName}", result["GuidCollection"]);
        }
        
        [Fact]
        public void ToRedis_should_be_able_to_handle_collection_of_strings()
        {
            RedisMapper.RegisterType<CollectionOfStringsTestType1>();
            Assert.Single(RedisMapper.TypesWithRegisteredProperties[typeof(CollectionOfStringsTestType1)]);

            RedisMapper.RegisterType<CollectionOfStringsTestType2>();
            Assert.Single(RedisMapper.TypesWithRegisteredProperties[typeof(CollectionOfStringsTestType2)]);

            var subject = new CollectionOfStringsTestType2
            {
                StringCollection = new List<string>
                {
                    "item1",
                    "item2",
                    "item3",
                    @"|item4"
                }
            };

            var result = RedisMapper.MapToRedisValues(subject);
            Assert.Equal($@"{typeof(List<string>).AssemblyQualifiedName}||item1||item2||item3||\|item4",
                result["StringCollection"]);
        }

        [Fact]
        public void ToRedis_should_be_able_to_handle_collection_of_Guids()
        {
            RedisMapper.RegisterType<CollectionOfGuidTestType1>();
            Assert.Single(RedisMapper.TypesWithRegisteredProperties[typeof(CollectionOfGuidTestType1)]);

            RedisMapper.RegisterType<CollectionOfGuidTestType2>();
            Assert.Single(RedisMapper.TypesWithRegisteredProperties[typeof(CollectionOfGuidTestType2)]);

            var subject = new CollectionOfGuidTestType2
            {
                GuidCollection = new List<Guid>
                {
                    Guid.Empty,
                    Guid.Parse("11c43ee8-b9d3-4e51-b73f-bd9dda66e29c")
                }
            };

            var result = RedisMapper.MapToRedisValues(subject);
            Assert.Equal(
                $@"{
                        typeof(List<Guid>).AssemblyQualifiedName
                    }||00000000-0000-0000-0000-000000000000||11c43ee8-b9d3-4e51-b73f-bd9dda66e29c",
                result["GuidCollection"]);
        }

        [Fact]
        public void FromRedis_should_return_an_object_of_a_supported_type_without_a_collection()
        {
            RedisMapper.RegisterType<TestType1>();
            
            var subject = new Dictionary<string, RedisValue>
            {
                {"IntegerProperty", 1},
                {"FloatProperty", 2.0f},
                {"DoubleProperty", 3.0d},
                {"BoolProperty", true},
                {"StringProperty", "teszt"},
                {"ByteArrayProperty", new byte[] {0, 1, 2}},
                {"DateTimeProperty", new DateTime(2000, 1, 1, 12, 0, 0).ToString("O")},
                {"GuidProperty", Guid.Empty.ToString()},
                {"GeoPositionProperty", "12.123,45.456"}
            };

            var result = Assert.IsType<TestType1>(RedisMapper.FromRedisValues<TestType1>(subject));
            Assert.Equal(1, result.IntegerProperty);
            Assert.Equal(2.0f, result.FloatProperty);
            Assert.Equal(3.0d, result.DoubleProperty);
            Assert.True(result.BoolProperty);
            Assert.Equal("teszt", result.StringProperty);
            Assert.Equal(3, result.ByteArrayProperty.Length);
            Assert.Equal(0, result.ByteArrayProperty[0]);
            Assert.Equal(1, result.ByteArrayProperty[1]);
            Assert.Equal(2, result.ByteArrayProperty[2]);
            Assert.Equal(new DateTime(2000, 1, 1, 12, 0, 0), result.DateTimeProperty);
            Assert.Equal(Guid.Empty, result.GuidProperty);
            Assert.Equal(new GeoPosition(12.123,45.456), result.GeoPositionProperty);
        }

        [Fact]
        public void FromRedis_should_return_an_object_of_a_supported_type_with_an_ICollection_of_strings()
        {
            RedisMapper.RegisterType<CollectionOfStringsTestType1>();

            var subject = new Dictionary<string, RedisValue>
            {
                {
                    "StringCollection",
                    $@"{typeof(List<string>).AssemblyQualifiedName}||item1||item2||item3||\|item4"
                }
            };

            var result =
                Assert.IsType<CollectionOfStringsTestType1>(RedisMapper.FromRedisValues<CollectionOfStringsTestType1>(subject));

            Assert.Equal(4, result.StringCollection.Count);
            Assert.Contains("item1", result.StringCollection);
            Assert.Contains("item2", result.StringCollection);
            Assert.Contains("item3", result.StringCollection);
            Assert.Contains("|item4", result.StringCollection);
        }

        [Fact]
        public void FromRedis_should_return_an_object_of_a_supported_type_with_an_ICollection_of_Guids()
        {
            RedisMapper.RegisterType<CollectionOfGuidTestType1>();

            var subject = new Dictionary<string, RedisValue>
            {
                {
                    "GuidCollection",
                    $@"{typeof(List<Guid>).AssemblyQualifiedName}||00000000-0000-0000-0000-000000000000||11c43ee8-b9d3-4e51-b73f-bd9dda66e29c"
                }
            };
            
            var result = Assert.IsType<CollectionOfGuidTestType1>(RedisMapper.FromRedisValues<CollectionOfGuidTestType1>(subject));

            Assert.Equal(2, result.GuidCollection.Count);
            Assert.Contains(Guid.Empty, result.GuidCollection);
            Assert.Contains(Guid.Parse("11c43ee8-b9d3-4e51-b73f-bd9dda66e29c"), result.GuidCollection);
        }
    }
}