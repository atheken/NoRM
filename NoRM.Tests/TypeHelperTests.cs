using System;
using Norm.BSON;
using Norm.Configuration;
using Xunit;

namespace Norm.Tests
{
	public class TypeHelperTests
	{
		[Fact]
		public void Can_get_discriminator_for_type()
		{
			var helper = TypeHelper.GetHelperForType(typeof(SuperClassObject));

			Assert.Equal("Norm.Tests.SuperClassObject, NoRM.Tests", helper.GetTypeDiscriminator());
		}

		[Fact]
		public void Can_get_discriminator_for_sub_type()
		{
			var helper = TypeHelper.GetHelperForType(typeof(SubClassedObject));

			Assert.Equal("Norm.Tests.SubClassedObject, NoRM.Tests", helper.GetTypeDiscriminator());
		}

		[Fact]
		public void Can_Get_Discriminator_When_Discriminator_Is_On_An_Interface()
		{
			var helper = TypeHelper.GetHelperForType(typeof(InterfaceDiscriminatedClass));
			Assert.Equal("Norm.Tests.InterfaceDiscriminatedClass, NoRM.Tests", helper.GetTypeDiscriminator());
		}

		[Fact]
		public void Can_Get_Id_Property_For_Type()
		{
			var helper = TypeHelper.GetHelperForType(typeof(SuperClassObject));
			Assert.NotNull(helper.FindIdProperty());
		}

		[Fact]
		public void Can_Infer_ID_From_Interface_Attribute()
		{
			var helper = TypeHelper.GetHelperForType(typeof(DtoWithNonDefaultIdClass));

            Assert.NotNull(helper.FindIdProperty());
		}

        [Fact]
        public void Can_Determine_Id_When_Entity_Has__id_Property()
        {
            Assert.Equal("_id", TypeHelper.FindIdProperty(typeof(EntityWithUnderscoreidProperty)).Name);
        }

        [Fact]
        public void Can_Determine_Id_When_Entity_Has_Id_Property()
        {
            Assert.Equal("Id", TypeHelper.FindIdProperty(typeof(EntityWithIdProperty)).Name);
        }

        [Fact]
        public void Can_Determine_Id_When_Entity_Has_Id_Identified_By_MongoIdentifierAttribute()
        {
            Assert.Equal("UnconventionalId", TypeHelper.FindIdProperty(typeof(EntityWithAttributeDefinedId)).Name);
        }

        [Fact]
        public void Can_Determine_Id_When_Entity_Has_Id_Defined_By_Map()
        {
            MongoConfiguration.Initialize(config => config.AddMap<EntityWithIdDefinedByMapConfigurationMap>());
            Assert.Equal("UnconventionalId", TypeHelper.FindIdProperty(typeof(EntityWithIdDefinedByMap)).Name);
        }

        [Fact]
        public void FindIdProperty_Throws_MongoConfigurationException_When_Entity_Has__id_And_MongoIdentifierAttribute()
        {
            Assert.Throws<MongoConfigurationMapException>(() => TypeHelper.FindIdProperty(typeof(EntityWithUnderscoreidAndAttribute)));
        }

        [Fact]
        public void FindIdProperty_Throws_MongoConfigurationException_When_Entity_Has__id_And_MappedId()
        {
            MongoConfiguration.Initialize(config => config.AddMap<EntityWithUnderscoreidAndMappedIdConfigurationMap>());
            Assert.Throws<MongoConfigurationMapException>(() => TypeHelper.FindIdProperty(typeof(EntityWithUnderscoreidAndAttribute)));
        }

        [Fact]
        public void FindIdProperty_Returns_MappedId_Property_When_Entity_Has_MappedId_And_Attribute_Defined_Id()
        {
            MongoConfiguration.Initialize(config => config.AddMap<EntityWithMappedIdAndAttributeDefindIdConfigurationMap>());
            Assert.Equal("MappedId", TypeHelper.FindIdProperty(typeof(EntityWithMappedIdAndAttributeDefindId)).Name);
        }

        [Fact]
        public void FindIdProperty_Returns_Attribute_Defined_Id_Property_When_Entity_Has_Attribute_Defined_Id_And_Conventional_Id()
        {
            Assert.Equal("AttributeDefinedId", TypeHelper.FindIdProperty(typeof(EntityWithAttributeDefindIdAndConventionalId)).Name);
        }

        [Fact]
        public void FindIdProperty_Returns_Null_When_Entity_Has_No_Id_Defined()
        {
            Assert.Null(TypeHelper.FindIdProperty(typeof(EntityWithNoId)));
        }
	}

    public class EntityWithNoId
    {
        public string SomeProperty { get; set; }
    }

    public class EntityWithAttributeDefindIdAndConventionalId
    {
        [MongoIdentifier]
        public Guid AttributeDefinedId { get; set;}

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