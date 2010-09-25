
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

		/// <summary>
		/// The Earth is Round but Maps are Flat!
		/// The current implementation assumes an idealized model of a flat earth, meaning that an arcdegree of latitude (y) and longitude (x) represent the same distance everywhere.
		/// </summary>
		/// <param name="kilometers">The kilometers to convert into arcdegrees</param>
		/// <returns>Arcdegree from kilometers</returns>
		public static double Kilometers2ArcDegree(int kilometers)
		{
			return (double)kilometers * (1d / 111d);
			// 1/111
		}

		/// <summary>
		/// The Earth is Round but Maps are Flat!
		/// The current implementation assumes an idealized model of a flat earth, meaning that an arcdegree of latitude (y) and longitude (x) represent the same distance everywhere.
		/// </summary>
		/// <param name="kilometers">The kilometers to convert into arcdegrees</param>
		/// <returns>Arcdegree from kilometers</returns>
		public static double Miles2ArcDegree(int miles)
		{
			return (double)miles * (1d / 69d);
		}
	}
}
