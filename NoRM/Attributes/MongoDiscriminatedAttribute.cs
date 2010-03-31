using System;

namespace Norm
{
	/// <summary>
	/// Flags a type as having a discriminator.  Apply to a base type to enable multiple-inheritance.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class MongoDiscriminatedAttribute : Attribute
	{
	}
}