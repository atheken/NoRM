using Norm.BSON;

namespace Norm.Commands.Qualifiers
{
	/// <summary>
	/// The Circle Qualifier "db.places.find({"loc" : {"$within" : {"$center" : [center, radius]}}})"
	/// </summary>
	public class CircleQualifer : QualifierCommand, IWhitinShapeQualifier
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CircleQualifer"/> class.
		/// </summary>
		/// <param name="center">The center point</param>
		/// <param name="radius">The radius of the search</param>
		public CircleQualifer(LatLng center, double radius)
			: base("$center", new object[] { center.ToArray(), radius })
		{

		}
	}
}