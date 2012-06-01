﻿using System;
using System.Linq;
using Norm.BSON;
using Norm.Configuration;
using NUnit.Framework;


namespace Norm.Tests
{
    [TestFixture]
    public class IdPropertyFinderTests
    {
        
        [Test]
        public void Can_Determine_Id_When_Entity_Has__id_Property()
        {
            Assert.AreEqual("_id", CreateIdPropertyFinderFor<EntityWithUnderscoreidProperty>().IdProperty.Name);
        }

        [Test]
        public void Can_Determine_Id_When_Entity_Has_Id_Property()
        {
            Assert.AreEqual("Id", CreateIdPropertyFinderFor<EntityWithIdProperty>().IdProperty.Name);
        }

        [Test]
        public void Can_Determine_Id_When_Entity_Has_Id_Identified_By_MongoIdentifierAttribute()
        {
            Assert.AreEqual("UnconventionalId", CreateIdPropertyFinderFor<EntityWithAttributeDefinedId>().IdProperty.Name);
        }

        [Test]
        public void Can_Determine_Id_When_Entity_Has_Id_Defined_By_Map()
        {
            MongoConfiguration.Initialize(config => config.AddMap<EntityWithIdDefinedByMapConfigurationMap>());
            Assert.AreEqual("UnconventionalId", CreateIdPropertyFinderFor<EntityWithIdDefinedByMap>().IdProperty.Name);
        }

        [Test]
        public void FindIdProperty_Throws_MongoConfigurationException_When_Entity_Has__id_And_MongoIdentifierAttribute()
        {
            Assert.Throws<MongoConfigurationMapException>(() => 
            {
                var id = CreateIdPropertyFinderFor<EntityWithUnderscoreidAndAttribute>().IdProperty;
            });
        }

        [Test]
        public void FindIdProperty_Throws_MongoConfigurationException_When_Entity_Has__id_And_MappedId()
        {
            MongoConfiguration.Initialize(config => config.AddMap<EntityWithUnderscoreidAndMappedIdConfigurationMap>());
            Assert.Throws<MongoConfigurationMapException>(() => 
            {
                var id = CreateIdPropertyFinderFor<EntityWithUnderscoreidAndAttribute>().IdProperty;
            });
        }

        [Test]
        public void FindIdProperty_Returns_MappedId_Property_When_Entity_Has_MappedId_And_Attribute_Defined_Id()
        {
            MongoConfiguration.Initialize(config => config.AddMap<EntityWithMappedIdAndAttributeDefindIdConfigurationMap>());
            Assert.AreEqual("MappedId", CreateIdPropertyFinderFor<EntityWithMappedIdAndAttributeDefindId>().IdProperty.Name);
        }

        [Test]
        public void FindIdProperty_Returns_Attribute_Defined_Id_Property_When_Entity_Has_Attribute_Defined_Id_And_Conventional_Id()
        {
            Assert.AreEqual("AttributeDefinedId", CreateIdPropertyFinderFor<EntityWithAttributeDefindIdAndConventionalId>().IdProperty.Name);
        }

        [Test]
        public void FindIdProperty_Returns_Null_When_Entity_Has_No_Id_Defined()
        {
            MongoConfiguration.Initialize(config => config.RemoveFor<EntityWithNoId>());
            Assert.Null(CreateIdPropertyFinderFor<EntityWithNoId>().IdProperty);
        }

        [Test]
        public void FindIdProperty_Returns_Mapped_Id_With_Correct_Getter_And_Setter() 
        {
            object id = null;
            MongoConfiguration.Initialize(config => config.For<EntityWithNoId>(t => t.IdIs(
                x => id,
                (x, value) => { id = value; }
            )));
            var idProperty = CreateIdPropertyFinderFor<EntityWithNoId>().IdProperty;
            var propertyValue = new object();
            idProperty.Setter(null, propertyValue);

            Assert.AreSame(propertyValue, idProperty.Getter(null));
            Assert.AreSame(propertyValue, id);
        }

        [Test]
        public void FindIdProperty_Returns_Id_Specified_By_Attribute_In_Implemented_Interface()
        {
            Assert.AreEqual("MyId", CreateIdPropertyFinderFor<DtoWithNonDefaultIdClass>().IdProperty.Name);
        }

        private IdPropertyFinder CreateIdPropertyFinderFor<T>()
        {
            var properties = typeof(T).GetProperties();
            var magic = properties.Select(
                property => new MagicProperty(typeof(T), new MagicPropertyConfiguration(property))
            ).ToList();

            return new IdPropertyFinder(typeof(T), magic);
        }
    }

    public class EntityWithNoId
    {
        public string SomeProperty { get; set; }
    }

    public class EntityWithAttributeDefindIdAndConventionalId
    {
        [MongoIdentifier]
        public Guid AttributeDefinedId { get; set; }

        public ObjectId Id { get; set; }
    }

    public class EntityWithMappedIdAndAttributeDefindIdConfigurationMap : MongoConfigurationMap
    {
        public EntityWithMappedIdAndAttributeDefindIdConfigurationMap()
        {
            For<EntityWithMappedIdAndAttributeDefindId>(config => config.IdIs(entity => entity.MappedId));
        }
    }

    public class EntityWithMappedIdAndAttributeDefindId
    {
        public Guid MappedId { get; set; }

        [MongoIdentifier]
        public ObjectId AttributeDefinedId { get; set; }
    }

    public class EntityWithUnderscoreidAndMappedIdConfigurationMap : MongoConfigurationMap
    {
        public EntityWithUnderscoreidAndMappedIdConfigurationMap()
        {
            For<EntityWithUnderscoreidAndMappedId>(config => config.IdIs(entity => entity.UnconventionalId));
        }
    }

    public class EntityWithUnderscoreidAndMappedId
    {
        public ObjectId _id { get; set; }
        public ObjectId UnconventionalId { get; set; }
    }

    public class EntityWithUnderscoreidAndAttribute
    {
        public ObjectId _id { get; set; }

        [MongoIdentifier]
        public Guid UnconventionalId { get; set; }
    }

    public class EntityWithIdDefinedByMap
    {
        public ObjectId UnconventionalId { get; set; }
    }

    public class EntityWithIdDefinedByMapConfigurationMap : MongoConfigurationMap
    {
        public EntityWithIdDefinedByMapConfigurationMap()
        {
            For<EntityWithIdDefinedByMap>(config => config.IdIs(entity => entity.UnconventionalId));
        }
    }

    public class EntityWithAttributeDefinedId
    {
        [MongoIdentifier]
        public ObjectId UnconventionalId { get; set; }
    }

    public class EntityWithIdProperty
    {
        public ObjectId Id { get; set; }
    }

    public class EntityWithUnderscoreidProperty
    {
        public ObjectId _id { get; set; }
    }
}