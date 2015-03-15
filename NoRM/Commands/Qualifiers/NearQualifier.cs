using Norm.BSON;

namespace Norm.Commands.Qualifiers
{
	/// <summary>
	/// The Near Qualifier "db.places.find( { loc : { $near : [50,50] } } )"
	/// </summary>
	public class NearQualifier : QualifierCommand
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="NearQualifier"/> class.
		/// </summary>
		/// <param name="center">The center point</param>
		public NearQualifier(LatLng center)
			: base("$near", center.ToArray())
		{
		}
	}
}