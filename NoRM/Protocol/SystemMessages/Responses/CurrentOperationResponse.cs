
namespace Norm.Responses
{
    /// <summary>
    /// The current operation response.
    /// </summary>
    public class CurrentOperationResponse
    {
        /// <summary>TODO::Description.</summary>
        public double? OpID { get; set; }

        /// <summary>TODO::Description.</summary>
        public string Op { get; set; }

        /// <summary>TODO::Description.</summary>
        public string Ns { get; set; }

        /// <summary>TODO::Description.</summary>
        public string Query { get; set; }

        /// <summary>TODO::Description.</summary>
        public double? InLock { get; set; }
    }
}