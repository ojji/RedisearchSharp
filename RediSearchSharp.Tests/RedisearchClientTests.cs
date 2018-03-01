using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using NUnit.Framework;
using RediSearchSharp.Internal;
using RediSearchSharp.Query;
using RediSearchSharp.Serialization;
using StackExchange.Redis;

namespace RediSearchSharp.Tests
{
    [TestFixture]
    public class RedisearchClientTests
    {
        public class Car : RedisearchSerializable<Car>
        {
            public Guid Id { get; set; }
            public string Make { get; set; }
            public string Model { get; set; }
            public int Year { get; set; }
            public string Country { get; set; }
            public string Engine { get; set; }
            public string Class { get; set; }
            public double Price { get; set; }
            public GeoPosition Location { get; set; }
        }

        [TestFixture]
        public class AddDocument : AddDocumentFixture
        {
            [Test]
            public void Should_return_true_if_successful()
            {
                var car = new Car
                {
                    Id = Guid.NewGuid(),
                    Make = "Kia",
                    Model = "Niro 1.6 GDi Hybrid Comfortline",
                    Year = 2016,
                    Country = "Korea",
                    Engine = "hybrid",
                    Class = "suv",
                    Price = 25995,
                    Location = new GeoPosition(19.03991, 47.49801)
                };

                var client = new RediSearchClient(Db);
                Assert.That(client.AddDocument(car), Is.True);
            }

            public class UnexistingIndexObject : RedisearchSerializable<UnexistingIndexObject>
            {
                public int Id { get; set; }
            }

            [Test]
            public void Should_return_false_when_index_does_not_exist()
            {
                var unexistingIndexObject = new UnexistingIndexObject
                {
                    Id = 1
                };

                var client = new RediSearchClient(Db);
                Assert.That(client.AddDocument(unexistingIndexObject), Is.False);
            }

            [Test]
            public void Should_return_false_when_a_document_already_exists_with_the_same_id()
            {
                var car1 = new Car
                {
                    Id = Guid.Empty,
                    Make = "Kia",
                    Model = "Niro 1.6 GDi Hybrid Comfortline",
                    Year = 2016,
                    Country = "Korea",
                    Engine = "hybrid",
                    Class = "suv",
                    Price = 25995,
                    Location = new GeoPosition(19.03991, 47.49801)
                };

                var car2 = new Car
                {
                    Id = Guid.Empty,
                    Make = "Hyundai",
                    Model = "Santa Fe 2.2 CRDi R 2WD Premium",
                    Year = 2016,
                    Country = "Korea",
                    Engine = "diesel",
                    Class = "suv",
                    Price = 65022,
                    Location = new GeoPosition(21.71671, 47.95539)
                };

                var client = new RediSearchClient(Db);
                Assert.That(client.AddDocument(car1), Is.True);
                Assert.That(client.AddDocument(car2), Is.False);
            }
        }

        [TestFixture]
        public class AddDocumentAsync : AddDocumentFixture
        {
            [Test]
            public async Task Should_return_true_if_successful()
            {
                var car = new Car
                {
                    Id = Guid.NewGuid(),
                    Make = "Kia",
                    Model = "Niro 1.6 GDi Hybrid Comfortline",
                    Year = 2016,
                    Country = "Korea",
                    Engine = "hybrid",
                    Class = "suv",
                    Price = 25995,
                    Location = new GeoPosition(19.03991, 47.49801)
                };

                var client = new RediSearchClient(Db);
                Assert.That(await client.AddDocumentAsync(car), Is.True);
            }

            public class UnexistingIndexObject : RedisearchSerializable<UnexistingIndexObject>
            {
                public int Id { get; set; }
            }

            [Test]
            public async Task Should_return_false_when_index_does_not_exist()
            {
                var unexistingIndexObject = new UnexistingIndexObject
                {
                    Id = 1
                };

                var client = new RediSearchClient(Db);
                Assert.That(await client.AddDocumentAsync(unexistingIndexObject), Is.False);
            }

            [Test]
            public async Task Should_return_false_when_a_document_already_exists_with_the_same_id()
            {
                var car1 = new Car
                {
                    Id = Guid.Empty,
                    Make = "Kia",
                    Model = "Niro 1.6 GDi Hybrid Comfortline",
                    Year = 2016,
                    Country = "Korea",
                    Engine = "hybrid",
                    Class = "suv",
                    Price = 25995,
                    Location = new GeoPosition(19.03991, 47.49801)
                };

                var car2 = new Car
                {
                    Id = Guid.Empty,
                    Make = "Hyundai",
                    Model = "Santa Fe 2.2 CRDi R 2WD Premium",
                    Year = 2016,
                    Country = "Korea",
                    Engine = "diesel",
                    Class = "suv",
                    Price = 65022,
                    Location = new GeoPosition(21.71671, 47.95539)
                };

                var client = new RediSearchClient(Db);
                Assert.That(await client.AddDocumentAsync(car1), Is.True);
                Assert.That(await client.AddDocumentAsync(car2), Is.False);
            }
        }

        [TestFixture]
        public class Search : SearchFixture
        {
            [Test]
            public void Search_sample()
            {
                var query = new Query<Car>()
                    .Where(c => c.Make)
                    .MustMatch("kia")
                    .Build();

                var cars = Client.Search(query);
                Assert.That(cars.Results, Has.One.Items);
            }

            [Test]
            public void Search_should_not_set_values_for_ignored_properties()
            {
                var query = new Query<SearchFixture.IgnoredPropertyTest>()
                        .Where(t => t.Property1)
                        .MustMatch("property1")
                    .Build();
                var results = Client.Search(query);
                Assert.That(results.Results, Has.One.Items);
                Assert.That(results.Results.First().IgnoredProperty, Is.Null);
            }
        }
    }

    public class SearchFixture
    {
        public class IgnoredPropertyTest : RedisearchSerializable<IgnoredPropertyTest>
        {
            public string Id { get; set; }
            public string Property1 { get; set; }
            public string IgnoredProperty { get; set; }
            public string Property2 { get; set; }
            
            protected override void OnCreatingSchemaInfo(SchemaInfoBuilder<IgnoredPropertyTest> builder)
            {
                builder.Property(t => t.IgnoredProperty).Ignore();
            }
        }

        protected RediSearchClient Client { get; private set; }
        private IConnectionMultiplexer Connection { get; set; }

        [OneTimeSetUp]
        public void SetupConnection()
        {
            Connection = ConnectionMultiplexer.Connect("127.0.0.1");
            Client = new RediSearchClient(Connection);
            CreateIndex();
            foreach (var car in CarsDb)
            {
                Assert.That(Client.AddDocument(car), Is.True);
            }
            Assert.That(Client.AddDocument(new IgnoredPropertyTest()
            {
                Id = "id1",
                IgnoredProperty = "ignored",
                Property1 = "property1",
                Property2 = "property2"
            }));
        }

        [OneTimeTearDown]
        public void TeardownConnection()
        {
            DropIndex();
            Connection.Dispose();
        }

        public void CreateIndex()
        {
            Assert.That(
                (string)Connection.GetDatabase().Execute("FT.CREATE", "cars-index", "SCHEMA", "Id", "TEXT", "NOSTEM", "Make",
                    "TEXT", "NOSTEM", "Model", "TEXT", "NOSTEM", "Year", "NUMERIC", "SORTABLE", "Country", "TEXT",
                    "NOSTEM", "Engine", "TEXT", "NOSTEM", "Class", "TEXT", "NOSTEM", "Price", "NUMERIC", "SORTABLE",
                    "Location", "GEO"), Is.EqualTo("OK"));

            Assert.That(
                (string)Connection.GetDatabase().Execute("FT.CREATE", "ignoredpropertytests-index", "SCHEMA", "Id", "TEXT", "NOSTEM", "Property1",
                    "TEXT", "NOSTEM", "Property2", "TEXT", "NOSTEM"), Is.EqualTo("OK"));
        }

        private void DropIndex()
        {
            Assert.That(
                (string) Connection.GetDatabase().Execute("FT.DROP", "cars-index"), Is.EqualTo("OK"));

            Assert.That(
                (string)Connection.GetDatabase().Execute("FT.DROP", "ignoredpropertytests-index"), Is.EqualTo("OK"));
        }
        
        private static List<RedisearchClientTests.Car> CarsDb => new List<RedisearchClientTests.Car>
        {
            new RedisearchClientTests.Car
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Make = "Mercedes-Benz",
                Model = "S 500 e",
                Year = 2014,
                Country = "germany",
                Engine = "electric",
                Class = "luxury-sedan",
                Price = 124149,
                Location = new GeoPosition(19.03991,47.49801)
            },
            new RedisearchClientTests.Car
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Make = "Mercedes-Benz",
                Model = "AMG GT",
                Year = 2014,
                Country = "germany",
                Engine = "petrol",
                Class = "luxury-coupe",
                Price = 157506,
                Location = new GeoPosition(20.3,48.21667)
            },
            new RedisearchClientTests.Car
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Make = "Mercedes-Benz",
                Model = "S 65 AMG Speedshift Plus 7G-Tronic auto 2d",
                Year = 2017,
                Country = "germany",
                Engine = "petrol",
                Class = "luxury-sedan",
                Price = 197510,
                Location = new GeoPosition(19.35515,47.59657)
            },
            new RedisearchClientTests.Car
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                Make = "Kia",
                Model = "Niro 1.6 GDi Hybrid Comfortline",
                Year = 2016,
                Country = "korea",
                Engine = "hybrid",
                Class = "suv",
                Price = 25995,
                Location = new GeoPosition(19.03991,47.49801)
            },
            new RedisearchClientTests.Car
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                Make = "Hyundai",
                Model = "Santa Fe 2.2 CRDi R 2WD Premium",
                Year = 2016,
                Country = "korea",
                Engine = "diesel",
                Class = "suv",
                Price = 65022,
                Location = new GeoPosition(21.71671,47.95539)
            },
            new RedisearchClientTests.Car
            {
                Id = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                Make = "Audi",
                Model = "Q5 2.0 TDI",
                Year = 2017,
                Country = "germany",
                Engine = "diesel",
                Class = "suv",
                Price = 62710,
                Location = new GeoPosition(18.23333,46.08333)
            },
        };
    }

    public class AddDocumentFixture
    {
        protected IConnectionMultiplexer Db { get; private set; }

        [OneTimeSetUp]
        public void SetupConnection()
        {
            Db = ConnectionMultiplexer.Connect("127.0.0.1");
        }

        [OneTimeTearDown]
        public void TeardownConnection()
        {
            Db.Dispose();
        }

        [SetUp]
        public void CreateIndex()
        {
            Assert.That(
                (string) Db.GetDatabase().Execute("FT.CREATE", "cars-index", "SCHEMA", "Id", "TEXT", "NOSTEM", "Make",
                    "TEXT", "NOSTEM", "Model", "TEXT", "NOSTEM", "Year", "NUMERIC", "SORTABLE", "Country", "TEXT",
                    "NOSTEM", "Engine", "TEXT", "NOSTEM", "Class", "TEXT", "NOSTEM", "Price", "NUMERIC", "SORTABLE",
                    "Location", "GEO"), Is.EqualTo("OK"));
        }

        [TearDown]
        public void CleanIndex()
        {
            Assert.That((string) Db.GetDatabase().Execute("FT.DROP", "cars-index"), Is.EqualTo("OK"));
        }
    }
}