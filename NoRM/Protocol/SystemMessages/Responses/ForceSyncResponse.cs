
namespace NoRM.Responses
{
    /// <summary>
    /// Indicates the result of a demand that MongoDB flush non-file committed writes to their respective files.
    /// </summary>
    public class ForceSyncResponse
    {
        public double? OK { get; set; }
        public int? NumFiles { get; set; }
    }
}