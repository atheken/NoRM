
namespace Norm.BSON
{
	/// <summary>
	/// Represents a coordinate 
	/// </summary>
	public class LatLng
	{

		/// <summary>
		/// The latitude
		/// </summary>
		public double Latitude { get; set; }
		/// <summary>
		/// The longitude
		/// </summary>
		public double Longitude { get; set; }

		/// <summary>
		/// Creates an array of doubles
		/// </summary>
		/// <returns></returns>
		public double[] ToArray()
		{
			return new double[]{
                this.Latitude , 
                this.Longitude 
           };
		}
	}
}
