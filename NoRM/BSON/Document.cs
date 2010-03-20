
namespace Norm.BSON
{
    internal class Document
    {
        /// <summary>
        /// Document length
        /// </summary>
        internal int Length;
        /// <summary>
        /// Document parent
        /// </summary>
        internal Document Parent;
        /// <summary>
        /// Digested (read/written)
        /// </summary>
        internal int Digested;
    }
}
