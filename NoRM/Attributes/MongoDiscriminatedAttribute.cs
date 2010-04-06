using System;
using System.Linq;
using Norm.BSON;

namespace Norm
{
	/// <summary>
	/// Flags a type as having a discriminator.  Apply to a base type to enable multiple-inheritance.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
	public class MongoDiscriminatedAttribute : Attribute
	{
		/// <summary>
		/// Finds the sub-type or interface from the given type that declares itself as a discriminating base class
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static Type GetDiscriminatingTypeFor(Type type)
		{
			var usingType = type;

			while (usingType != typeof(object))
			{
				var discriminator = GetDiscriminatedAttribute(usingType);
				if (discriminator != null)
					return usingType;

				usingType = usingType.BaseType;
			}

			foreach (var iface in type.GetInterfaces())
			{
				var discriminator = GetDiscriminatedAttribute(iface);

				if (discriminator != null)
					return iface;
			}
			return null;
		}

		/// <summary>
		/// Determines whether the type given directly declares itself as saving sub-types with a discriminator
		/// </summary>
		/// <param name="usingType"></param>
		/// <returns></returns>
		private static MongoDiscriminatedAttribute GetDiscriminatedAttribute(Type usingType)
		{
			return usingType.GetCustomAttributes(BsonHelper.MongoDiscriminatedAttribute, false)
				.OfType<MongoDiscriminatedAttribute>()
				.FirstOrDefault();
		}
	}
}