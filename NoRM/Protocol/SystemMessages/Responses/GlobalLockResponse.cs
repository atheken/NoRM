
namespace NoRM.Responses
{
    /// <summary>
    /// Global lock response.
    /// </summary>
    public class GlobalLockResponse
    {
        public double? TotalTime { get; set; }
        public double? LockTime { get; set; }
        public double? Ratio { get; set; }
    }
}
