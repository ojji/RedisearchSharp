using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using RediSearchSharp.Internal;
using RediSearchSharp.Query;
using RediSearchSharp.Serialization;
using StackExchange.Redis;

namespace RediSearchSharp.Tests
{
    [TestFixture]
    public class SchemaMetadataTests
    {
        [TestFixture]
        public class GetSchemaMetadata
        {
            [SuppressMessage("ReSharper", "UnusedMember.Local")]
            private class Car : RedisearchSerializable<Car>
            {
                public int Id { get; set; }
                public string Model { get; set; }
                public string Make { get; set; }

                protected override void OnCreatingSchemaInfo(SchemaMetadataBuilder<Car> schemaMetadataBuilder)
                {
                    schemaMetadataBuilder.IndexName("cars-indexname");
                }
            }

            [SuppressMessage("ReSharper", "UnusedMember.Local")]
            [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
            [SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
            private class Boss : RedisearchSerializable<Boss>
            {
                public int Id { get; set; }
                public string Name { get; set; }

                protected override void OnCreatingSchemaInfo(SchemaMetadataBuilder<Boss> builder)
                {
                    builder.PrimaryKey(b => b.Id);
                    builder.DocumentIdPrefix("boss-prefix");
                }
            }

            [Test]
            public void Should_return_set_index_name()
            {
                var schemaMetadata = SchemaMetadata<Car>.GetSchemaMetadata();
                Assert.That(schemaMetadata.IndexName, Is.EqualTo((RedisValue)"cars-indexname"));
            }

            [SuppressMessage("ReSharper", "UnusedMember.Local")]
            private class InvalidIndexNameTest : RedisearchSerializable<InvalidIndexNameTest>
            {
                public int Id { get; set; }
                protected override void OnCreatingSchemaInfo(SchemaMetadataBuilder<InvalidIndexNameTest> builder)
                {
                    builder.IndexName("");
                }
            }

            [Test]
            public void Invalid_index_name_should_throw()
            {
                Assert.Throws<ArgumentNullException>(() =>
                {
                    SchemaMetadata<InvalidIndexNameTest>.GetSchemaMetadata();
                });
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
            private class SerializedPropertiesTest : RedisearchSerializable<SerializedPropertiesTest>
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
            [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
            [SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
            private class IgnoredPropertiesTest : RedisearchSerializable<IgnoredPropertiesTest>
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

            [SuppressMessage("ReSharper", "UnusedMember.Local")]
            private class DefaultLanguageTest : RedisearchSerializable<DefaultLanguageTest>
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
            private class OverriddenLanguageTest : RedisearchSerializable<OverriddenLanguageTest>
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
            private class DefaultRedisearchPropertyTypeTest : RedisearchSerializable<DefaultRedisearchPropertyTypeTest>
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

            [TestFixture]
            public class PrimaryKeyTests
            {
                [SuppressMessage("ReSharper", "UnusedMember.Local")]
                private class IdPropertiesTest1 : RedisearchSerializable<IdPropertiesTest1>
                {
                    public Guid Id { get; set; }
                }

                [SuppressMessage("ReSharper", "UnusedMember.Local")]
                private class IdPropertiesTest2 : RedisearchSerializable<IdPropertiesTest2>
                {
                    public int IdPropertiesTest2Id { get; set; }
                }

                [Test]
                public void Default_id_should_be_recognizable_by_convention()
                {
                    var schemaMetadata1 = SchemaMetadata<IdPropertiesTest1>.GetSchemaMetadata();
                    Assert.That(schemaMetadata1.PrimaryKey.EntityClrType, Is.EqualTo(typeof(IdPropertiesTest1)));
                    Assert.That(schemaMetadata1.PrimaryKey.PropertyName, Is.EqualTo("Id"));
                    Assert.That(schemaMetadata1.PrimaryKey.PropertyClrType, Is.EqualTo(typeof(Guid)));

                    var schemaMetadata2 = SchemaMetadata<IdPropertiesTest2>.GetSchemaMetadata();
                    Assert.That(schemaMetadata2.PrimaryKey.EntityClrType, Is.EqualTo(typeof(IdPropertiesTest2)));
                    Assert.That(schemaMetadata2.PrimaryKey.PropertyName, Is.EqualTo("IdPropertiesTest2Id"));
                    Assert.That(schemaMetadata2.PrimaryKey.PropertyClrType, Is.EqualTo(typeof(int)));
                }

                [SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
                [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
                private class OverriddenIdPropertyTest : RedisearchSerializable<OverriddenIdPropertyTest>
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
                    var schemaMetadata = SchemaMetadata<OverriddenIdPropertyTest>.GetSchemaMetadata();
                    Assert.That(schemaMetadata.PrimaryKey.EntityClrType, Is.EqualTo(typeof(OverriddenIdPropertyTest)));
                    Assert.That(schemaMetadata.PrimaryKey.PropertyName, Is.EqualTo("InterestingId"));
                    Assert.That(schemaMetadata.PrimaryKey.PropertyClrType, Is.EqualTo(typeof(DateTime)));
                }

                [SuppressMessage("ReSharper", "UnusedMember.Local")]
                private class ThrowingIdPropertyTest : RedisearchSerializable<ThrowingIdPropertyTest>
                {
                    public DateTime InterestingId { get; set; }
                }

                [Test]
                public void Should_throw_when_no_default_id_can_be_detected_and_no_override_was_specified()
                {
                    Assert.Throws<ArgumentException>(() =>
                        SchemaMetadata<ThrowingIdPropertyTest>.GetSchemaMetadata());
                }

                [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
                private class GetPrimaryKeyFromEntityTests : RedisearchSerializable<GetPrimaryKeyFromEntityTests>
                {
                    public int Id { get; set; }
                }

                [Test]
                public void GetPrimaryKeyFromEntity_should_throw_when_the_entity_type_is_different()
                {
                    var schemaMetadata = SchemaMetadata<GetPrimaryKeyFromEntityTests>.GetSchemaMetadata();

                    Assert.Throws<ArgumentException>(() =>
                    {
                        schemaMetadata.PrimaryKey.GetPrimaryKeyFromEntity(5);
                    });
                }

                [Test]
                public void GetPrimaryKeyFromEntity_should_return_the_id_value_as_a_RedisValue()
                {
                    var schemaMetadata = SchemaMetadata<GetPrimaryKeyFromEntityTests>.GetSchemaMetadata();

                    var entity = new GetPrimaryKeyFromEntityTests
                    {
                        Id = 42
                    };

                    Assert.That(schemaMetadata.PrimaryKey.GetPrimaryKeyFromEntity(entity), Is.EqualTo((RedisValue) 42));
                }

                [Test]
                public void GetPrimaryKeyFromProperty_should_throw_when_the_property_type_is_different()
                {
                    var schemaMetadata = SchemaMetadata<GetPrimaryKeyFromEntityTests>.GetSchemaMetadata();

                    Assert.Throws<ArgumentException>(() =>
                    {
                        schemaMetadata.PrimaryKey.GetPrimaryKeyFromProperty(Guid.Empty);
                    });
                }

                [Test]
                public void GetPrimaryKeyFromProperty_should_return_the_property_value_as_a_RedisValue()
                {
                    var schemaMetadata = SchemaMetadata<GetPrimaryKeyFromEntityTests>.GetSchemaMetadata();

                    Assert.That(schemaMetadata.PrimaryKey.GetPrimaryKeyFromProperty(42), Is.EqualTo((RedisValue)42));
                }

                
                [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
                [SuppressMessage("ReSharper", "InconsistentNaming")]
                private class IFormattableTest : RedisearchSerializable<IFormattableTest>
                {
                    public DateTime Id { get; set; }
                }

                [Test]
                public void Primary_key_with_of_an_IFormattable_should_return_a_value_with_InvariantCulture()
                {
                    var schemaMetadata = SchemaMetadata<IFormattableTest>.GetSchemaMetadata();

                    var entity = new IFormattableTest
                    {
                        Id = new DateTime(2010, 10, 11, 12, 0, 0)
                    };

                    var property = new DateTime(2010, 10, 11, 12, 0, 0);

                    Assert.That(
                        schemaMetadata.PrimaryKey.GetPrimaryKeyFromEntity(entity),
                        Is.EqualTo((RedisValue) new DateTime(2010, 10, 11, 12, 0, 0).ToString(CultureInfo
                            .InvariantCulture)));

                    Assert.That(
                        schemaMetadata.PrimaryKey.GetPrimaryKeyFromProperty(property),
                        Is.EqualTo((RedisValue)new DateTime(2010, 10, 11, 12, 0, 0).ToString(CultureInfo
                            .InvariantCulture)));
                }
            }
        }
    }
}