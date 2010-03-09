
namespace NoRM.BSON
{
    /// <summary>
    /// A set of info about a serialized documet.
    /// </summary>
    internal class Document
    {
        internal int Length { get; set; }
        internal int Digested { get; set; }
        internal Document Parent { get; set; }
    }
}
