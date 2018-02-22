using NUnit.Framework;
using RediSearchSharp.Internal;
using StackExchange.Redis;

namespace RediSearchSharp.Tests
{
    [TestFixture]
    public class SchemaInfoTests
    {
        public class Car
        {
            public int Id { get; set; }
            public string Model { get; set; }
            public string Make { get; set; }
        }

        public class Boss
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        [TestFixture]
        public class GetSchemaInfo
        {
            [Test]
            public void Should_Return_IndexName()
            {
                var schemaInfo = SchemaInfo.GetSchemaInfo<Car>();
                Assert.That(schemaInfo.IndexName, Is.EqualTo((RedisValue)"cars-index"));

                schemaInfo = SchemaInfo.GetSchemaInfo<Boss>();
                Assert.That(schemaInfo.IndexName, Is.EqualTo((RedisValue)"boss-index"));
            }

            [Test]
            public void Should_Return_DocumentIdPrefix()
            {
                var carSchemaInfo = SchemaInfo.GetSchemaInfo<Car>();
                Assert.That(carSchemaInfo.DocumentIdPrefix, Is.EqualTo((RedisValue)"cars:"));

                var bossSchemaInfo = SchemaInfo.GetSchemaInfo<Boss>();
                Assert.That(bossSchemaInfo.DocumentIdPrefix, Is.EqualTo((RedisValue)"boss:"));
            }
        }
    }
}