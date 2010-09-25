using Norm.BSON;

namespace Norm.Commands.Qualifiers
{
	/// <summary>
	/// The Box Qualifier "db.places.find({"loc" : {"$within" : {"$box" : box}}})"
	/// </summary>
	public class BoxQualifer : QualifierCommand, IWhitinShapeQualifier
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="BoxQualifer"/> class.
		/// </summary>
		/// <param name="northEast">The top right corner of the box</param>
		/// <param name="southWest">The bottom left corner of the box</param>
		public BoxQualifer(LatLng southWest, LatLng northEast)
			: base("$box", new object[] { southWest.ToArray(), northEast.ToArray() })
		{

		}
	}
}