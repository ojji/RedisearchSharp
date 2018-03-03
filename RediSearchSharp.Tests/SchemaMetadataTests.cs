using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using NUnit.Framework;
using RediSearchSharp.Internal;
using RediSearchSharp.Query;
using RediSearchSharp.Serialization;
using StackExchange.Redis;
// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace RediSearchSharp.Tests
{
    [TestFixture]
    public class SchemaMetadataTests
    {
        public class Car : RedisearchSerializable<Car>
        {
            public int Id { get; set; }
            public string Model { get; set; }
            public string Make { get; set; }

            protected override void OnCreatingSchemaInfo(SchemaMetadataBuilder<Car> schemaMetadataBuilder)
            {
                schemaMetadataBuilder.IndexName("cars-indexname");
            }
        }

        public class Boss : RedisearchSerializable<Boss>
        {
            public int Id { get; set; }
            public string Name { get; set; }

            protected override void OnCreatingSchemaInfo(SchemaMetadataBuilder<Boss> builder)
            {
                builder.PrimaryKey(b => b.Id);
                builder.DocumentIdPrefix("boss-prefix");
            }
        }

        [TestFixture]
        public class GetSchemaInfo
        {
            [Test]
            public void Should_return_set_index_name()
            {
                var schemaMetadata = SchemaMetadata<Car>.GetSchemaMetadata();
                Assert.That(schemaMetadata.IndexName, Is.EqualTo((RedisValue)"cars-indexname"));
            }

            [Test]
            public void Should_return_default_index_name_when_not_set()
            {
                var schemaMetadata = SchemaMetadata<Boss>.GetSchemaMetadata();
                Assert.That(schemaMetadata.IndexName, Is.EqualTo((RedisValue)"boss-index"));
            }

            [Test]
            public void Should_return_set_document_id_prefix()
            {
                var bossSchemaInfo = SchemaMetadata<Boss>.GetSchemaMetadata();
                Assert.That(bossSchemaInfo.DocumentIdPrefix, Is.EqualTo((RedisValue)"boss-prefix"));
            }

            [Test]
            public void Should_return_default_document_id_prefix_when_not_set()
            {
                var carSchemaInfo = SchemaMetadata<Car>.GetSchemaMetadata();
                Assert.That(carSchemaInfo.DocumentIdPrefix, Is.EqualTo((RedisValue)"cars:"));
            }

            [SuppressMessage("ReSharper", "UnusedMember.Local")]
            class SerializedPropertiesTest : RedisearchSerializable<SerializedPropertiesTest>
            {
                public Guid Id { get; set; }
                public string Property1 { get; set; }
            }
            
            [Test]
            public void Serialized_properties_should_include_all_get_set_properties_by_default()
            {
                var schemaMetadata = SchemaMetadata<SerializedPropertiesTest>.GetSchemaMetadata();

                var serializedProperties = schemaMetadata.Properties.Where(p => !p.IsIgnored).Select(p => p.PropertyName);
                Assert.That(serializedProperties, 
                    Is.EquivalentTo(new[] {"Id", "Property1"}));
            }

            [SuppressMessage("ReSharper", "UnusedMember.Local")]
            class IgnoredPropertiesTest : RedisearchSerializable<IgnoredPropertiesTest>
            {
                public Guid Id { get; set; }
                public string Property1 { get; set; }
                public string IgnoredProperty { get; set; }
                public string Property2 { get; set; }

                protected override void OnCreatingSchemaInfo(SchemaMetadataBuilder<IgnoredPropertiesTest> builder)
                {
                    builder.Property(t => t.IgnoredProperty).Ignore();
                }
            }

            [Test]
            public void Serialized_properties_should_not_include_ignored_get_set_properties()
            {
                var schemaMetadata = SchemaMetadata<IgnoredPropertiesTest>.GetSchemaMetadata();

                var serializedProperties = schemaMetadata.Properties.Where(p => !p.IsIgnored).Select(p => p.PropertyName);
                Assert.That(
                    serializedProperties,
                    Is.EquivalentTo(new[] { "Id", "Property1", "Property2" }));
            }

            class IdPropertiesTest1 : RedisearchSerializable<IdPropertiesTest1>
            {
                public Guid Id { get; set; }
            }

            class IdPropertiesTest2 : RedisearchSerializable<IdPropertiesTest2>
            {
                public int IdPropertiesTest2Id { get; set; }
            }

            [Test]
            public void Default_id_should_be_recognizable_by_convention()
            {
                var testObj1 = new IdPropertiesTest1()
                {
                    Id = Guid.Empty
                };

                var testObj2 = new IdPropertiesTest2
                {
                    IdPropertiesTest2Id = 42
                };

                var schemaMetadata = SchemaMetadata<IdPropertiesTest1>.GetSchemaMetadata();
                Assert.That(schemaMetadata.PrimaryKeySelector, Is.Not.Null);
                Assert.That(schemaMetadata.PrimaryKeySelector(testObj1), Is.EqualTo((RedisValue)Guid.Empty.ToString()));

                var schemaInfo2 = SchemaMetadata<IdPropertiesTest2>.GetSchemaMetadata();
                Assert.That(schemaInfo2.PrimaryKeySelector, Is.Not.Null);
                Assert.That(schemaInfo2.PrimaryKeySelector(testObj2), Is.EqualTo((RedisValue)42));
            }

            class OverriddenIdPropertyTest : RedisearchSerializable<OverriddenIdPropertyTest>
            {
                public DateTime InterestingId { get; set; }

                protected override void OnCreatingSchemaInfo(SchemaMetadataBuilder<OverriddenIdPropertyTest> builder)
                {
                    builder.PrimaryKey(t => t.InterestingId);
                }
            }

            [Test]
            public void Default_id_should_be_overridable()
            {
                var testObj = new OverriddenIdPropertyTest
                {
                    InterestingId = new DateTime(2018, 12, 12)
                };

                var schemaMetadata = SchemaMetadata<OverriddenIdPropertyTest>.GetSchemaMetadata();
                Assert.That(schemaMetadata.PrimaryKeySelector, Is.Not.Null);
                Assert.That(schemaMetadata.PrimaryKeySelector(testObj), Is.EqualTo((RedisValue)testObj.InterestingId.ToString()));
            }

            [SuppressMessage("ReSharper", "UnusedMember.Local")]
            class ThrowingIdPropertyTest : RedisearchSerializable<ThrowingIdPropertyTest>
            {
                public DateTime InterestingId { get; set; }
            }

            [Test]
            public void Should_throw_when_no_default_id_can_be_detected_and_no_override_was_specified()
            {
                Assert.Throws<ArgumentException>(() => 
                    SchemaMetadata<ThrowingIdPropertyTest>.GetSchemaMetadata());
            }

            [SuppressMessage("ReSharper", "UnusedMember.Local")]
            class DefaultLanguageTest : RedisearchSerializable<DefaultLanguageTest>
            {
                public int Id { get; set; }
            }

            [Test]
            public void Language_should_be_set_to_default_when_not_overridden()
            {
                var schemaMetadata = SchemaMetadata<DefaultLanguageTest>.GetSchemaMetadata();
                Assert.That(schemaMetadata.Language, Is.EqualTo((RedisValue) "english"));
            }

            [SuppressMessage("ReSharper", "UnusedMember.Local")]
            class OverriddenLanguageTest : RedisearchSerializable<OverriddenLanguageTest>
            {
                public int Id { get; set; }

                protected override void OnCreatingSchemaInfo(SchemaMetadataBuilder<OverriddenLanguageTest> builder)
                {
                    builder.Language(Languages.Hungarian);
                }
            }

            [Test]
            public void Language_should_be_overridable()
            {
                var schemaMetadata = SchemaMetadata<OverriddenLanguageTest>.GetSchemaMetadata();
                Assert.That(schemaMetadata.Language, Is.EqualTo((RedisValue) "hungarian"));
            }

            [SuppressMessage("ReSharper", "UnusedMember.Local")]
            class DefaultRedisearchPropertyTypeTest : RedisearchSerializable<DefaultRedisearchPropertyTypeTest>
            {
                public int Id { get; set; }

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
                public object ObjectProperty { get; set; }
            }

            [Test]
            public void Default_redisearch_properties_should_be_recognized()
            {
                var schemaMetadata = SchemaMetadata<DefaultRedisearchPropertyTypeTest>.GetSchemaMetadata();

                var propertiesWithTypes =
                    schemaMetadata.Properties.ToDictionary(p => p.PropertyName, p => p.RedisearchType);

                Assert.That(propertiesWithTypes["ByteProperty"], Is.EqualTo(RedisearchPropertyType.Numeric));
                Assert.That(propertiesWithTypes["SByteProperty"], Is.EqualTo(RedisearchPropertyType.Numeric));
                Assert.That(propertiesWithTypes["ShortProperty"], Is.EqualTo(RedisearchPropertyType.Numeric));
                Assert.That(propertiesWithTypes["UShortProperty"], Is.EqualTo(RedisearchPropertyType.Numeric));
                Assert.That(propertiesWithTypes["IntProperty"], Is.EqualTo(RedisearchPropertyType.Numeric));
                Assert.That(propertiesWithTypes["UIntProperty"], Is.EqualTo(RedisearchPropertyType.Numeric));
                Assert.That(propertiesWithTypes["LongProperty"], Is.EqualTo(RedisearchPropertyType.Numeric));
                Assert.That(propertiesWithTypes["ULongProperty"], Is.EqualTo(RedisearchPropertyType.Numeric));
                Assert.That(propertiesWithTypes["FloatProperty"], Is.EqualTo(RedisearchPropertyType.Numeric));
                Assert.That(propertiesWithTypes["DoubleProperty"], Is.EqualTo(RedisearchPropertyType.Numeric));
                Assert.That(propertiesWithTypes["DecimalProperty"], Is.EqualTo(RedisearchPropertyType.Numeric));
                Assert.That(propertiesWithTypes["DateTimeProperty"], Is.EqualTo(RedisearchPropertyType.Fulltext));
                Assert.That(propertiesWithTypes["CharProperty"], Is.EqualTo(RedisearchPropertyType.Fulltext));
                Assert.That(propertiesWithTypes["StringProperty"], Is.EqualTo(RedisearchPropertyType.Fulltext));
                Assert.That(propertiesWithTypes["GeoPositionProperty"], Is.EqualTo(RedisearchPropertyType.Geo));
                Assert.That(propertiesWithTypes["ObjectProperty"], Is.EqualTo(RedisearchPropertyType.Fulltext));
            }
        }
    }
}