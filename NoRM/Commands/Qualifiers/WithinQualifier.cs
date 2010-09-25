using Norm.BSON;

namespace Norm.Commands.Qualifiers
{
	/// <summary>
	/// The within qualifier
	/// </summary>
	public class WithinQualifier : QualifierCommand
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="WithinQualifier"/> class.
		/// </summary>
		/// <param name="qualifier"></param>
		public WithinQualifier(IWhitinShapeQualifier qualifier)
			: base("$within", qualifier)
		{

		}
	}
}
