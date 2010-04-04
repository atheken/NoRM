using Norm.BSON;
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
		public void Can_get_discriminator_when_discriminator_is_on_an_interface()
		{
			var helper = TypeHelper.GetHelperForType(typeof(InterfaceDiscriminatedClass));

			Assert.Equal("Norm.Tests.InterfaceDiscriminatedClass, NoRM.Tests", helper.GetTypeDiscriminator());
		}
	}
}