using System;
using Norm.BSON;
using Norm.Configuration;
using NUnit.Framework;

namespace Norm.Tests
{
    [TestFixture]
	public class TypeHelperTests
	{
		
		[Test]
		public void Can_get_discriminator_for_type()
		{
			var helper = ReflectionHelper.GetHelperForType(typeof(SuperClassObject));

			Assert.AreEqual("Norm.Tests.SuperClassObject, NoRM.Tests", helper.GetTypeDiscriminator());
		}

		[Test]
		public void Can_get_discriminator_for_sub_type()
		{
			var helper = ReflectionHelper.GetHelperForType(typeof(SubClassedObject));

			Assert.AreEqual("Norm.Tests.SubClassedObject, NoRM.Tests", helper.GetTypeDiscriminator());
		}

		[Test]
		public void Can_Get_Discriminator_When_Discriminator_Is_On_An_Interface()
		{
			var helper = ReflectionHelper.GetHelperForType(typeof(InterfaceDiscriminatedClass));
			Assert.AreEqual("Norm.Tests.InterfaceDiscriminatedClass, NoRM.Tests", helper.GetTypeDiscriminator());
		}

		[Test]
		public void Can_Get_Id_Property_For_Type()
		{
			var helper = ReflectionHelper.GetHelperForType(typeof(SuperClassObject));
			Assert.NotNull(helper.FindIdProperty());
		}

        [Test]
		public void Can_Infer_ID_From_Interface_Attribute()
		{
			var helper = ReflectionHelper.GetHelperForType(typeof(DtoWithNonDefaultIdClass));
		    var something = helper.FindIdProperty();

            Assert.NotNull(something);
		}
	}
}