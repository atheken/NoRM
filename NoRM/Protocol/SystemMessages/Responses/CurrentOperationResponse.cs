
namespace NoRM.Responses
{
    /// <summary>
    /// The current operation response.
    /// </summary>
    public class CurrentOperationResponse
    {
        public double? OpID { get; set; }
        public string Op { get; set; }
        public string Ns { get; set; }
        public string Query { get; set; }
        public double? InLock { get; set; }
    }
}