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
			var helper = ReflectionHelper.GetHelperForType(typeof(SuperClassObject));

			Assert.Equal("Norm.Tests.SuperClassObject, NoRM.Tests", helper.GetTypeDiscriminator());
		}

		[Fact]
		public void Can_get_discriminator_for_sub_type()
		{
			var helper = ReflectionHelper.GetHelperForType(typeof(SubClassedObject));

			Assert.Equal("Norm.Tests.SubClassedObject, NoRM.Tests", helper.GetTypeDiscriminator());
		}

		[Fact]
		public void Can_Get_Discriminator_When_Discriminator_Is_On_An_Interface()
		{
			var helper = ReflectionHelper.GetHelperForType(typeof(InterfaceDiscriminatedClass));
			Assert.Equal("Norm.Tests.InterfaceDiscriminatedClass, NoRM.Tests", helper.GetTypeDiscriminator());
		}

		[Fact]
		public void Can_Get_Id_Property_For_Type()
		{
			var helper = ReflectionHelper.GetHelperForType(typeof(SuperClassObject));
			Assert.NotNull(helper.FindIdProperty());
		}

        [Fact]
		public void Can_Infer_ID_From_Interface_Attribute()
		{
			var helper = ReflectionHelper.GetHelperForType(typeof(DtoWithNonDefaultIdClass));
		    var something = helper.FindIdProperty();

            Assert.NotNull(something);
		}
	}
}