using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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
        public class CreateIndex : CreateIndexFixture
        {
            [SuppressMessage("ReSharper", "UnusedMember.Local")]
            private class SuccessfulIndexCreationTest : RedisearchSerializable<SuccessfulIndexCreationTest>             
            {
                public int Id { get; set; }
            }

            [Test]
            public void Should_return_true_when_creating_the_index_is_successful()
            {
                Assert.That(Client.CreateIndex<SuccessfulIndexCreationTest>(), Is.True);

                DropCreatedIndex<SuccessfulIndexCreationTest>();
            }

            [SuppressMessage("ReSharper", "UnusedMember.Local")]
            private class DuplicateIndexTest : RedisearchSerializable<DuplicateIndexTest>
            {
                public int Id { get; set; }
            }

            [Test]
            public void Should_return_false_when_index_already_exists()
            {
                Assert.That(Client.CreateIndex<DuplicateIndexTest>(), Is.True);
                Assert.That(Client.CreateIndex<DuplicateIndexTest>(), Is.False);

                DropCreatedIndex<DuplicateIndexTest>();
            }

            [SuppressMessage("ReSharper", "UnusedMember.Local")]
            private class DefaultIndexNameTest : RedisearchSerializable<DefaultIndexNameTest>
            {
                public int Id { get; set; }
            }

            [Test]
            public void Default_index_name_should_be_set_when_not_overridden()
            {
                Assert.That(Client.CreateIndex<DefaultIndexNameTest>(), Is.True);
                Assert.That(IsIndexCreated("defaultindexnametests-index"), Is.True);
                DropCreatedIndex<DefaultIndexNameTest>();
            }

            [SuppressMessage("ReSharper", "UnusedMember.Local")]
            private class CustomIndexNameTest : RedisearchSerializable<CustomIndexNameTest>
            {
                public int Id { get; set; }

                protected override void OnCreatingSchemaInfo(SchemaMetadataBuilder<CustomIndexNameTest> builder)
                {
                    builder.IndexName("custom-index");
                }
            }

            [Test]
            public void Custom_index_name_should_be_the_name_of_the_index_when_overridden()
            {
                Assert.That(Client.CreateIndex<CustomIndexNameTest>(), Is.True);
                Assert.That(IsIndexCreated("custom-index"), Is.True);

                DropCreatedIndex<CustomIndexNameTest>();
            }

            [SuppressMessage("ReSharper", "UnusedMember.Local")]
            private class DefaultRedisearchTypesTest : RedisearchSerializable<DefaultRedisearchTypesTest>
            {
                public int Id { get; set; }
                public GeoPosition GeoType { get; set; }
                public int NumericType { get; set; }
                public string TextType { get; set; }
                public DateTime DateTimeType { get; set; }
            }

            [Test]
            public void Should_create_a_field_for_every_property_by_default()
            {
                Assert.That(Client.CreateIndex<DefaultRedisearchTypesTest>(), Is.True);

                var fieldsInfo = GetFieldsInfo<DefaultRedisearchTypesTest>();
                Assert.That(fieldsInfo, Has.Exactly(5).Items);

                var fieldNames = fieldsInfo.Select(fi => fi.Name).ToArray();
                Assert.That(fieldNames, Has.Member("Id"));
                Assert.That(fieldNames, Has.Member("GeoType"));
                Assert.That(fieldNames, Has.Member("NumericType"));
                Assert.That(fieldNames, Has.Member("TextType"));
                Assert.That(fieldNames, Has.Member("DateTimeType"));

                DropCreatedIndex<DefaultRedisearchTypesTest>();
            }

            [Test]
            public void Should_assign_proper_redisearch_types_to_the_fields_by_default()
            {
                Assert.That(Client.CreateIndex<DefaultRedisearchTypesTest>(), Is.True);

                var fieldsInfo = GetFieldsInfo<DefaultRedisearchTypesTest>();
                Assert.That(fieldsInfo, Has.Exactly(5).Items);

                var fieldNamesWithTypes = fieldsInfo.ToDictionary(fi => fi.Name, fi => fi.Type);
                Assert.That(fieldNamesWithTypes["Id"], Is.EqualTo("NUMERIC"));
                Assert.That(fieldNamesWithTypes["GeoType"], Is.EqualTo("GEO"));
                Assert.That(fieldNamesWithTypes["NumericType"], Is.EqualTo("NUMERIC"));
                Assert.That(fieldNamesWithTypes["TextType"], Is.EqualTo("TEXT"));
                Assert.That(fieldNamesWithTypes["DateTimeType"], Is.EqualTo("TEXT"));

                DropCreatedIndex<DefaultRedisearchTypesTest>();
            }

            /*
             TODO - custom type serialization
             We should be able to assign a custom redisearch type to a property,
             eg. a datetime property could be treated as a sortable number,
             represented as a unix timestamp - in order to do this the serialization
             parts would need to know how to convert the property value to & from a 
             redisvalue. This mechanism probably(?) would need a per-type, 
             per-property custom serialization code.
             
            private class OverriddenRedisearchTypeTest : RedisearchSerializable<OverriddenRedisearchTypeTest>
            {
                public int Id { get; set; }
                public DateTime OverriddenDateTimeType { get; set; }

                protected override void OnCreatingSchemaInfo(SchemaMetadataBuilder<OverriddenRedisearchTypeTest> builder)
                {
                    builder.Property(t => t.OverriddenDateTimeType).AsRedisearchType(RedisearchPropertyType.Numeric, here_should_be_a_serializer_that_tells_what_the_hell_should_happen_when_it_has_to_convert_to_and_from_a_redisvalue);
                }
            }*/

            #region Text property tests
            [SuppressMessage("ReSharper", "UnusedMember.Local")]
            private class DefaultStemmingTest : RedisearchSerializable<DefaultStemmingTest>
            {
                public int Id { get; set; }
                public string StemmedText { get; set; }
            }

            [Test]
            public void A_text_property_should_have_stemming_by_default()
            {
                Assert.That(Client.CreateIndex<DefaultStemmingTest>(), Is.True);
                var textField = GetFieldsInfo<DefaultStemmingTest>().SingleOrDefault(fi => fi.Name == "StemmedText");

                Assert.That(textField, Is.Not.Null);
                Assert.That(textField.Properties, Does.Not.Contain("NOSTEM"));
                DropCreatedIndex<DefaultStemmingTest>();
            }

            [SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
            [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
            [SuppressMessage("ReSharper", "UnusedMember.Local")]
            private class NoStemmingTest : RedisearchSerializable<NoStemmingTest>
            {
                public int Id { get; set; }
                public string NotStemmedText { get; set; }

                protected override void OnCreatingSchemaInfo(SchemaMetadataBuilder<NoStemmingTest> builder)
                {
                    builder.Property(t => t.NotStemmedText).NotStemmed();
                }
            }

            [Test]
            public void A_text_property_should_disable_stemming_when_nostem_is_set()
            {
                Assert.That(Client.CreateIndex<NoStemmingTest>(), Is.True);
                var textField = GetFieldsInfo<NoStemmingTest>().SingleOrDefault(fi => fi.Name == "NotStemmedText");

                Assert.That(textField, Is.Not.Null);
                Assert.That(textField.Properties, Does.Contain("NOSTEM"));
                DropCreatedIndex<NoStemmingTest>();
            }

            [SuppressMessage("ReSharper", "UnusedMember.Local")]
            private class DefaultWeightTest : RedisearchSerializable<DefaultWeightTest>
            {
                public int Id { get; set; }
                public string TextPropertyWithDefaultWeight { get; set; }
            }
            
            [Test]
            public void Default_weight_for_text_properties_should_be_1()
            {
                Assert.That(Client.CreateIndex<DefaultWeightTest>(), Is.True);
                var textWithDefaultWeight = GetFieldsInfo<DefaultWeightTest>()
                    .SingleOrDefault(fi => fi.Name == "TextPropertyWithDefaultWeight");

                Assert.That(textWithDefaultWeight, Is.Not.Null);
                Assert.That(textWithDefaultWeight.Weight, Is.EqualTo(1.0d));

                DropCreatedIndex<DefaultWeightTest>();
            }

            [SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
            [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
            [SuppressMessage("ReSharper", "UnusedMember.Local")]
            private class CustomWeightTest : RedisearchSerializable<CustomWeightTest>
            {
                public int Id { get; set; }
                public string TextPropertyWithCustomWeight { get; set; }

                protected override void OnCreatingSchemaInfo(SchemaMetadataBuilder<CustomWeightTest> builder)
                {
                    builder.Property(t => t.TextPropertyWithCustomWeight).WithWeight(5.0);
                }
            }

            [Test]
            public void Should_set_custom_weight_when_overridden()
            {
                Assert.That(Client.CreateIndex<CustomWeightTest>(), Is.True);
                var textWithCustomWeight = GetFieldsInfo<CustomWeightTest>()
                    .SingleOrDefault(fi => fi.Name == "TextPropertyWithCustomWeight");

                Assert.That(textWithCustomWeight, Is.Not.Null);
                Assert.That(textWithCustomWeight.Weight, Is.EqualTo(5.0d));
             
                DropCreatedIndex<CustomWeightTest>();
            }

            [SuppressMessage("ReSharper", "UnusedMember.Local")]
            private class NotSortableTextPropertyTest : RedisearchSerializable<NotSortableTextPropertyTest>
            {
                public int Id { get; set; }
                public string NotSortableText { get; set; }
            }

            [Test]
            public void A_text_property_should_not_be_sortable_by_default()
            {
                Assert.That(Client.CreateIndex<NotSortableTextPropertyTest>(), Is.True);
                var textPropertyInfo = GetFieldsInfo<NotSortableTextPropertyTest>()
                    .SingleOrDefault(fi => fi.Name == "NotSortableText");

                Assert.That(textPropertyInfo, Is.Not.Null);
                Assert.That(textPropertyInfo.Properties, Does.Not.Contain("SORTABLE"));

                DropCreatedIndex<NotSortableTextPropertyTest>();
            }

            [SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
            [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
            [SuppressMessage("ReSharper", "UnusedMember.Local")]
            private class SortableTextPropertyTest : RedisearchSerializable<SortableTextPropertyTest>
            {
                public int Id { get; set; }
                public string SortableText { get; set; }

                protected override void OnCreatingSchemaInfo(SchemaMetadataBuilder<SortableTextPropertyTest> builder)
                {
                    builder.Property(t => t.SortableText).AsSortable();
                }
            }

            [Test]
            public void A_text_property_should_be_sortable_when_overridden()
            {
                Assert.That(Client.CreateIndex<SortableTextPropertyTest>(), Is.True);

                var textPropertyInfo = GetFieldsInfo<SortableTextPropertyTest>()
                    .SingleOrDefault(fi => fi.Name == "SortableText");

                Assert.That(textPropertyInfo, Is.Not.Null);
                Assert.That(textPropertyInfo.Properties, Does.Contain("SORTABLE"));

                DropCreatedIndex<SortableTextPropertyTest>();
            }
            #endregion

            #region Numeric property tests
            [SuppressMessage("ReSharper", "UnusedMember.Local")]
            private class NotSortableNumericPropertyTest : RedisearchSerializable<NotSortableNumericPropertyTest>
            {
                public int Id { get; set; }
                public decimal NotSortableNumeric { get; set; }
            }

            [Test]
            public void A_numeric_property_should_not_be_sortable_by_default()
            {
                Assert.That(Client.CreateIndex<NotSortableNumericPropertyTest>(), Is.True);
                var numericPropertyInfo = GetFieldsInfo<NotSortableNumericPropertyTest>()
                    .SingleOrDefault(fi => fi.Name == "NotSortableNumeric");

                Assert.That(numericPropertyInfo, Is.Not.Null);
                Assert.That(numericPropertyInfo.Properties, Does.Not.Contain("SORTABLE"));

                DropCreatedIndex<NotSortableNumericPropertyTest>();
            }

            [SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
            [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
            [SuppressMessage("ReSharper", "UnusedMember.Local")]
            private class SortableNumericPropertyTest : RedisearchSerializable<SortableNumericPropertyTest>
            {
                public int Id { get; set; }
                public decimal SortableNumeric { get; set; }

                protected override void OnCreatingSchemaInfo(SchemaMetadataBuilder<SortableNumericPropertyTest> builder)
                {
                    builder.Property(t => t.SortableNumeric).AsSortable();
                }
            }

            [Test]
            public void A_numeric_property_should_be_sortable_when_overridden()
            {
                Assert.That(Client.CreateIndex<SortableNumericPropertyTest>(), Is.True);

                var textPropertyInfo = GetFieldsInfo<SortableNumericPropertyTest>()
                    .SingleOrDefault(fi => fi.Name == "SortableNumeric");

                Assert.That(textPropertyInfo, Is.Not.Null);
                Assert.That(textPropertyInfo.Properties, Does.Contain("SORTABLE"));

                DropCreatedIndex<SortableNumericPropertyTest>();
            }
            #endregion

            [SuppressMessage("ReSharper", "UnusedMember.Local")]
            private class PropertyIndexedByDefaultTest : RedisearchSerializable<PropertyIndexedByDefaultTest>
            {
                public int Id { get; set; }
                public string Text { get; set; }
            }

            [Test]
            public void Properties_should_not_have_noindex_by_default()
            {
                Assert.That(Client.CreateIndex<PropertyIndexedByDefaultTest>(), Is.True);

                var properties = GetFieldsInfo<PropertyIndexedByDefaultTest>();

                Assert.That(properties, Has.None.Matches<FieldInfo>(fi => fi.Properties.Any(p => p == "NOINDEX")));

                DropCreatedIndex<PropertyIndexedByDefaultTest>();
            }

            [SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
            [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
            [SuppressMessage("ReSharper", "UnusedMember.Local")]
            private class PropertyNotIndexedTest : RedisearchSerializable<PropertyNotIndexedTest>
            {
                public int Id { get; set; }
                public string Text { get; set; }

                protected override void OnCreatingSchemaInfo(SchemaMetadataBuilder<PropertyNotIndexedTest> builder)
                {
                    builder.Property(t => t.Text).NotIndexed();
                }
            }

            [Test]
            public void Should_set_noindex_on_property_when_overridden()
            {
                Assert.That(Client.CreateIndex<PropertyNotIndexedTest>(), Is.True);

                var properties = GetFieldsInfo<PropertyNotIndexedTest>();

                Assert.That(properties, Has.Exactly(1).Matches<FieldInfo>(fi => 
                    fi.Name == "Text" && 
                    fi.Properties.Any(p => p == "NOINDEX")));

                DropCreatedIndex<PropertyNotIndexedTest>();
            }
        }

        [TestFixture]
        public class Search : SearchFixture
        {
            [Test]
            public void Search_by_id_sample()
            {
                var query = new Query<Car>()
                    .WithId(Guid.Parse("11111111-1111-1111-1111-111111111111"))
                    .Build();

                var cars = Client.Search(query);
                Assert.That(cars, Has.One.Items);
            }

            [Test]
            public void Search_sample()
            {
                var query = new Query<Car>()
                    .WithId(
                        Guid.Empty, 
                        Guid.Parse("11111111-1111-1111-1111-111111111111"),
                        Guid.Parse("44444444-4444-4444-4444-444444444444"))
                    .Where(c => c.Make)
                    .MustMatch("kia")
                    .Build();

                var cars = Client.Search(query);
                Assert.That(cars, Has.One.Items);
            }

            [Test]
            public void Search_should_not_set_values_for_ignored_properties()
            {
                var query = new Query<IgnoredPropertyTest>()
                        .Where(t => t.Property1)
                        .MustMatch("property1")
                    .Build();
                var results = Client.Search(query).ToArray();
                Assert.That(results, Has.One.Items);
                Assert.That(results.First().Entity.IgnoredProperty, Is.Null);
            }
        }
    }

    #region Fixtures

    public class CreateIndexFixture
    {
        protected RediSearchClient Client { get; private set; }
        private IConnectionMultiplexer Connection { get; set; }

        protected void DropCreatedIndex<TEntity>()
                where TEntity : RedisearchSerializable<TEntity>, new()
        {
            var indexName = SchemaMetadata<TEntity>
                .GetSchemaMetadata().IndexName;
            Assert.That((string)Connection.GetDatabase().Execute("FT.DROP", indexName), Is.EqualTo("OK"));
        }

        protected class FieldInfo
        {
            public string Name { get; }
            public string Type { get; }
            public string[] Properties { get; }
            public double? Weight { get; }

            public FieldInfo(RedisResult[] fieldInfoArray)
            {
                var length = fieldInfoArray.Length;
                if (length < 3)
                {
                    throw new ArgumentException("Field info array is not valid.");
                }

                Name = (string) fieldInfoArray[0];
                Type = (string) fieldInfoArray[2];
                bool hasWeight = Type == "TEXT";
                int propertiesLength = hasWeight ? fieldInfoArray.Length - 5 : fieldInfoArray.Length - 3;
                if (hasWeight)
                {
                    Weight = (double) fieldInfoArray[4];
                }
                else
                {
                    Weight = null;
                }

                var propertiesAsRedisResult = new ArraySegment<RedisResult>(
                    fieldInfoArray, 
                    hasWeight ? 5 : 3,
                    propertiesLength
                );

                Properties = propertiesAsRedisResult.Select(rr => (string)rr).ToArray();
            }
        }
        
        [OneTimeSetUp]
        public void SetupConnection()
        {
            Connection = ConnectionMultiplexer.Connect("127.0.0.1");
            Client = new RediSearchClient(Connection);
        }

        public bool IsIndexCreated(string indexName)
        {
            try
            {
                GetFieldsInfo(indexName);
            }
            catch (RedisServerException)
            {
                return false;
            }
            return true;
        }

        protected FieldInfo[] GetFieldsInfo<TEntity>()
            where TEntity : RedisearchSerializable<TEntity>, new()
        {
            return GetFieldsInfo(SchemaMetadata<TEntity>.GetSchemaMetadata().IndexName);
        }

        private FieldInfo[] GetFieldsInfo(string indexName)
        {
            var info = (RedisResult[]) Connection.GetDatabase().Execute("FT.INFO", indexName);
            int fieldsIndex = 0;
            while (info[fieldsIndex].ToString() != "fields" && fieldsIndex != info.Length) 
            {
                fieldsIndex++;
            }

            var fieldInfos = (RedisResult[]) info[++fieldsIndex];
            return fieldInfos.Select(fi => new FieldInfo((RedisResult[])fi)).ToArray();
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
            
            protected override void OnCreatingSchemaInfo(SchemaMetadataBuilder<IgnoredPropertyTest> builder)
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
        public void DropIndex()
        {
            Assert.That((string) Db.GetDatabase().Execute("FT.DROP", "cars-index"), Is.EqualTo("OK"));
        }
    }
    #endregion
}