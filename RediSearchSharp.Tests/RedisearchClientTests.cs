using System;
using System.Linq.Expressions;
using RediSearchSharp.Query;
using RediSearchSharp.Serialization;
using StackExchange.Redis;
using Xunit;

namespace RediSearchSharp.Tests
{
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

            public override Expression<Func<Car, string>> IdSelector => c => c.Id.ToString();
        }

        private readonly IConnectionMultiplexer _connection;
        public RedisearchClientTests()
        {
            _connection = ConnectionMultiplexer.Connect("127.0.0.1");
        }

        [Fact]
        public void AddDocument_Should_Add_Document()
        {
            // "FT.CREATE" "cars-db" "SCHEMA" "Id" "TEXT" "NOSTEM" "Make" "TEXT" "NOSTEM" "Model" "TEXT" "NOSTEM" "Year" "NUMERIC" "SORTABLE" "Country" "TEXT" "NOSTEM" "Engine" "TEXT" "NOSTEM" "Class" "TEXT" "NOSTEM" "Price" "NUMERIC" "SORTABLE" "Location" "GEO"
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
            
            var client = new RediSearchClient(_connection);
            Assert.True(client.AddDocument("cars-db",
                car));
        }

        [Fact]
        public void Search_Sample()
        {
            var client = new RediSearchClient(_connection);

            var query = new Query<Car>()
                .Where(c => c.Make)
                    .MustMatch("kia")
                .Build();

            var cars = client.Search("cars-db", query);
            Assert.Single(cars.Results);
        }
    }
}