using System;
using NUnit.Framework;
using RediSearchSharp.Internal;
using RediSearchSharp.Serialization;
using StackExchange.Redis;

namespace RediSearchSharp.Tests
{
    [TestFixture]
    public class SchemaInfoTests
    {
        public class Car : RedisearchSerializable<Car>
        {
            public int Id { get; set; }
            public string Model { get; set; }
            public string Make { get; set; }

            protected override void OnCreatingSchemaInfo(SchemaInfoBuilder<Car> schemaInfoBuilder)
            {
                schemaInfoBuilder.IndexName("cars-indexname");
            }
        }

        public class Boss : RedisearchSerializable<Boss>
        {
            public int Id { get; set; }
            public string Name { get; set; }

            protected override void OnCreatingSchemaInfo(SchemaInfoBuilder<Boss> builder)
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
                var schemaInfo = SchemaInfo<Car>.GetSchemaInfo();
                Assert.That(schemaInfo.IndexName, Is.EqualTo((RedisValue)"cars-indexname"));
            }

            [Test]
            public void Should_return_default_index_name_when_not_set()
            {
                var schemaInfo = SchemaInfo<Boss>.GetSchemaInfo();
                Assert.That(schemaInfo.IndexName, Is.EqualTo((RedisValue)"boss-index"));
            }

            [Test]
            public void Should_return_set_document_id_prefix()
            {
                var bossSchemaInfo = SchemaInfo<Boss>.GetSchemaInfo();
                Assert.That(bossSchemaInfo.DocumentIdPrefix, Is.EqualTo((RedisValue)"boss-prefix"));
            }

            [Test]
            public void Should_return_default_document_id_prefix_when_not_set()
            {
                var carSchemaInfo = SchemaInfo<Car>.GetSchemaInfo();
                Assert.That(carSchemaInfo.DocumentIdPrefix, Is.EqualTo((RedisValue)"cars:"));
            }

            class SerializedPropertiesTest : RedisearchSerializable<SerializedPropertiesTest>
            {
                public Guid Id { get; set; }
                public string Property1 { get; set; }
            }
            
            [Test]
            public void Serialized_properties_should_include_all_get_set_properties_by_default()
            {
                var schemaInfo = SchemaInfo<SerializedPropertiesTest>.GetSchemaInfo();
                Assert.That(
                    schemaInfo.PropertiesToSerialize, 
                    Is.EquivalentTo(new[] {"Id", "Property1"}));
            }

            class IgnoredPropertiesTest : RedisearchSerializable<IgnoredPropertiesTest>
            {
                public Guid Id { get; set; }
                public string Property1 { get; set; }
                public string IgnoredProperty { get; set; }
                public string Property2 { get; set; }

                protected override void OnCreatingSchemaInfo(SchemaInfoBuilder<IgnoredPropertiesTest> builder)
                {
                    builder.Property(t => t.IgnoredProperty).Ignore();
                }
            }

            [Test]
            public void Serialized_properties_should_not_include_ignored_get_set_properties()
            {
                var schemaInfo = SchemaInfo<IgnoredPropertiesTest>.GetSchemaInfo();
                Assert.That(
                    schemaInfo.PropertiesToSerialize,
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

                var schemaInfo = SchemaInfo<IdPropertiesTest1>.GetSchemaInfo();
                Assert.That(schemaInfo.PrimaryKeySelector, Is.Not.Null);
                Assert.That(schemaInfo.PrimaryKeySelector(testObj1), Is.EqualTo((RedisValue)Guid.Empty.ToString()));

                var schemaInfo2 = SchemaInfo<IdPropertiesTest2>.GetSchemaInfo();
                Assert.That(schemaInfo2.PrimaryKeySelector, Is.Not.Null);
                Assert.That(schemaInfo2.PrimaryKeySelector(testObj2), Is.EqualTo((RedisValue)42));
            }

            class OverriddenIdPropertyTest : RedisearchSerializable<OverriddenIdPropertyTest>
            {
                public DateTime InterestingId { get; set; }

                protected override void OnCreatingSchemaInfo(SchemaInfoBuilder<OverriddenIdPropertyTest> builder)
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

                var schemaInfo = SchemaInfo<OverriddenIdPropertyTest>.GetSchemaInfo();
                Assert.That(schemaInfo.PrimaryKeySelector, Is.Not.Null);
                Assert.That(schemaInfo.PrimaryKeySelector(testObj), Is.EqualTo((RedisValue)testObj.InterestingId.ToString()));
            }

            class ThrowingIdPropertyTest : RedisearchSerializable<ThrowingIdPropertyTest>
            {
                public DateTime InterestingId { get; set; }
            }

            [Test]
            public void Should_throw_when_no_default_id_can_be_detected_and_no_override_was_specified()
            {
                Assert.Throws<ArgumentException>(() => 
                    SchemaInfo<ThrowingIdPropertyTest>.GetSchemaInfo());
            }
        }
    }
}