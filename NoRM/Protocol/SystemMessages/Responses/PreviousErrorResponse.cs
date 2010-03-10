
namespace NoRM.Responses
{
    /// <summary>
    /// The previous error response.
    /// </summary>
    public class PreviousErrorResponse
    {
        public double? OK { get; set; }
        public long? N { get; set; }
        public string Err { get; set; }
        public long? NPrev { get; set; }
    }
}