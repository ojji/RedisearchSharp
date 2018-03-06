using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using RediSearchSharp.Serialization;
using StackExchange.Redis;

namespace RediSearchSharp.Tests
{
    public class TestType1
    {
        public bool BoolProperty { get; set; }
        public byte ByteProperty { get; set; }
        public sbyte SByteProperty { get; set; }
        public short ShortProperty { get; set; }
        public ushort UShortProperty { get; set; }
        public int IntProperty { get; set; }
        public uint UIntProperty { get; set; }
        public long LongProperty { get; set; }
        public ulong ULongProperty { get; set; }
        public float FloatProperty { get; set; }
        public double DoubleProperty { get; set; }
        public decimal DecimalProperty { get; set; }
        public DateTime DateTimeProperty { get; set; }
        public char CharProperty { get; set; }
        public string StringProperty { get; set; }
        public GeoPosition GeoPositionProperty { get; set; }
        public byte[] ByteArrayProperty { get; set; }
        public Guid GuidProperty { get; set; }
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

    [TestFixture]
    public class RedisMapperTests
    {
        [TearDown]
        public void UnregisterTypes()
        {
            RedisMapper.UnregisterAll();
        }

        [Test]
        public void RegisterType_should_register_public_get_set_properties()
        {
            RedisMapper.RegisterType<TestType1>();
            Assert.That(RedisMapper.TypesWithRegisteredProperties[typeof(TestType1)], Has.Exactly(18).Items);
        }

        [Test]
        public void RegisterType_should_register_ICollection_of_string_properties()
        {
            RedisMapper.RegisterType<CollectionOfStringsTestType1>();
            Assert.That(
                RedisMapper.TypesWithRegisteredProperties[typeof(CollectionOfStringsTestType1)],
                Has.One.Matches<KeyValuePair<string, Type>>(props =>
                    props.Key == "StringCollection" && props.Value == typeof(ICollection<string>)));

            RedisMapper.RegisterType<CollectionOfStringsTestType2>();
            Assert.That(
                RedisMapper.TypesWithRegisteredProperties[typeof(CollectionOfStringsTestType2)],
                Has.One.Matches<KeyValuePair<string, Type>>(props =>
                    props.Key == "StringCollection" && props.Value == typeof(List<string>)));
        }

        [Test]
        public void RegisterType_should_register_ICollection_of_Guid_properties()
        {
            RedisMapper.RegisterType<CollectionOfGuidTestType1>();
            Assert.That(
                RedisMapper.TypesWithRegisteredProperties[typeof(CollectionOfGuidTestType1)],
                Has.One.Matches<KeyValuePair<string, Type>>(props =>
                    props.Key == "GuidCollection" && props.Value == typeof(ICollection<Guid>)));

            RedisMapper.RegisterType<CollectionOfGuidTestType2>();
            Assert.That(
                RedisMapper.TypesWithRegisteredProperties[typeof(CollectionOfGuidTestType2)],
                Has.One.Matches<KeyValuePair<string, Type>>(props =>
                    props.Key == "GuidCollection" && props.Value == typeof(List<Guid>)));
        }

        [Test]
        public void RegisterType_should_throw_by_default_when_an_unsupported_type_is_detected()
        {
            Assert.Throws<ArgumentException>(() => RedisMapper.RegisterType<UnsupportedPropertyTestType1>());
            Assert.Throws<ArgumentException>(() => RedisMapper.RegisterType<UnsupportedPropertyTestType2>());
        }

        [Test]
        public void ToRedis_should_be_able_to_handle_an_empty_collection_of_strings()
        {
            RedisMapper.RegisterType<CollectionOfStringsTestType1>();
            Assert.That(
                RedisMapper.TypesWithRegisteredProperties[typeof(CollectionOfStringsTestType1)],
                Has.One.Items);

            RedisMapper.RegisterType<CollectionOfStringsTestType2>();
            Assert.That(
                RedisMapper.TypesWithRegisteredProperties[typeof(CollectionOfStringsTestType2)],
                Has.One.Items);

            var subject = new CollectionOfStringsTestType2
            {
                StringCollection = new List<string>()
            };

            var result = RedisMapper.MapToRedisValues(subject);
            Assert.That(
                result["StringCollection"],
                Is.EqualTo((RedisValue) typeof(List<string>).AssemblyQualifiedName));
        }

        [Test]
        public void ToRedis_should_be_able_to_handle_an_empty_collection_of_Guids()
        {
            RedisMapper.RegisterType<CollectionOfGuidTestType1>();
            Assert.That(
                RedisMapper.TypesWithRegisteredProperties[typeof(CollectionOfGuidTestType1)],
                Has.One.Items);

            RedisMapper.RegisterType<CollectionOfGuidTestType2>();
            Assert.That(
                RedisMapper.TypesWithRegisteredProperties[typeof(CollectionOfGuidTestType2)],
                Has.One.Items);

            var subject = new CollectionOfGuidTestType2
            {
                GuidCollection = new List<Guid>()
            };

            var result = RedisMapper.MapToRedisValues(subject);
            Assert.That(result["GuidCollection"],
                Is.EqualTo((RedisValue) typeof(List<Guid>).AssemblyQualifiedName));
        }

        [Test]
        public void ToRedis_should_be_able_to_handle_collection_of_strings()
        {
            RedisMapper.RegisterType<CollectionOfStringsTestType1>();
            Assert.That(
                RedisMapper.TypesWithRegisteredProperties[typeof(CollectionOfStringsTestType1)],
                Has.One.Items);

            RedisMapper.RegisterType<CollectionOfStringsTestType2>();
            Assert.That(
                RedisMapper.TypesWithRegisteredProperties[typeof(CollectionOfStringsTestType2)],
                Has.One.Items);

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
            Assert.That(
                result["StringCollection"],
                Is.EqualTo((RedisValue) $@"{
                        typeof(List<string>).AssemblyQualifiedName
                    }||item1||item2||item3||\|item4"));
        }

        [Test]
        public void ToRedis_should_be_able_to_handle_collection_of_Guids()
        {
            RedisMapper.RegisterType<CollectionOfGuidTestType1>();
            Assert.That(
                RedisMapper.TypesWithRegisteredProperties[typeof(CollectionOfGuidTestType1)],
                Has.One.Items);

            RedisMapper.RegisterType<CollectionOfGuidTestType2>();
            Assert.That(
                RedisMapper.TypesWithRegisteredProperties[typeof(CollectionOfGuidTestType2)],
                Has.One.Items);

            var subject = new CollectionOfGuidTestType2
            {
                GuidCollection = new List<Guid>
                {
                    Guid.Empty,
                    Guid.Parse("11c43ee8-b9d3-4e51-b73f-bd9dda66e29c")
                }
            };

            var result = RedisMapper.MapToRedisValues(subject);
            Assert.That(
                result["GuidCollection"],
                Is.EqualTo(
                    (RedisValue) $@"{
                            typeof(List<Guid>).AssemblyQualifiedName
                        }||00000000-0000-0000-0000-000000000000||11c43ee8-b9d3-4e51-b73f-bd9dda66e29c"));
        }

        [Test]
        public void FromRedis_should_return_an_object_of_a_supported_type_without_a_collection()
        {
            RedisMapper.RegisterType<TestType1>();

            var subject = new Dictionary<string, RedisValue>
            {
                {"BoolProperty", true},
                {"ByteProperty", (byte)255},
                {"SByteProperty", (sbyte)-128},
                {"ShortProperty", (short)-32768},
                {"UShortProperty", (ushort)65535},
                {"IntProperty", (int)-2147483648},
                {"UIntProperty", (uint)4294967295},
                {"LongProperty", (long)-9223372036854775808},
                {"ULongProperty", ((ulong)18446744073709551615).ToString(CultureInfo.InvariantCulture)},
                {"FloatProperty", 2.0f},
                {"DoubleProperty", 3.0d},
                {"DecimalProperty", 1234.5678m.ToString(CultureInfo.InvariantCulture)},
                {"DateTimeProperty", new DateTime(2000, 1, 1, 12, 0, 0).ToString("O")},
                {"CharProperty", 'a'},
                {"StringProperty", "teszt"},
                {"GeoPositionProperty", "12.123,45.456"},
                {"ByteArrayProperty", new byte[] {0, 1, 2}},
                {"GuidProperty", Guid.Empty.ToString()},
            };

            var result = RedisMapper.FromRedisValues<TestType1>(subject);

            Assert.That(result.BoolProperty, Is.True);
            Assert.That(result.ByteProperty, Is.EqualTo(255));
            Assert.That(result.SByteProperty, Is.EqualTo(-128));
            Assert.That(result.ShortProperty, Is.EqualTo(-32768));
            Assert.That(result.UShortProperty, Is.EqualTo(65535));
            Assert.That(result.IntProperty, Is.EqualTo(-2147483648));
            Assert.That(result.UIntProperty, Is.EqualTo(4294967295));
            Assert.That(result.LongProperty, Is.EqualTo(-9223372036854775808));
            Assert.That(result.ULongProperty, Is.EqualTo(18446744073709551615));
            Assert.That(result.FloatProperty, Is.EqualTo(2.0f));
            Assert.That(result.DoubleProperty, Is.EqualTo(3.0d));
            Assert.That(result.DecimalProperty, Is.EqualTo(1234.5678m));
            Assert.That(result.DateTimeProperty, Is.EqualTo(new DateTime(2000, 1, 1, 12, 0, 0)));
            Assert.That(result.CharProperty, Is.EqualTo('a'));
            Assert.That(result.StringProperty, Is.EqualTo("teszt"));
            Assert.That(result.GeoPositionProperty, Is.EqualTo(new GeoPosition(12.123, 45.456)));
            Assert.That(
                result.ByteArrayProperty,
                Is.EquivalentTo(new[] { 0, 1, 2 }));
            Assert.That(result.GuidProperty, Is.EqualTo(Guid.Empty));
        }

        [Test]
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

            var result = RedisMapper.FromRedisValues<CollectionOfStringsTestType1>(subject);

            Assert.That(result.StringCollection, Has.Exactly(4).Items);
            Assert.That(
                result.StringCollection,
                Is.EquivalentTo(new[]
                {
                    "item1",
                    "item2",
                    "item3",
                    "|item4"
                }));
        }

        [Test]
        public void FromRedis_should_return_an_object_of_a_supported_type_with_an_ICollection_of_Guids()
        {
            RedisMapper.RegisterType<CollectionOfGuidTestType1>();

            var subject = new Dictionary<string, RedisValue>
            {
                {
                    "GuidCollection",
                    $@"{
                            typeof(List<Guid>).AssemblyQualifiedName
                        }||00000000-0000-0000-0000-000000000000||11c43ee8-b9d3-4e51-b73f-bd9dda66e29c"
                }
            };

            var result = RedisMapper.FromRedisValues<CollectionOfGuidTestType1>(subject);

            Assert.That(result.GuidCollection, Has.Exactly(2).Items);
            Assert.That(
                result.GuidCollection,
                Is.EquivalentTo(new[]
                {
                    Guid.Empty,
                    Guid.Parse("11c43ee8-b9d3-4e51-b73f-bd9dda66e29c")
                }));
        }
    }
}