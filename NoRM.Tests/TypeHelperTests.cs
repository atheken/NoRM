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