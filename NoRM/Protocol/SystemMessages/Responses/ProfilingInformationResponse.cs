using System;

namespace Norm.Responses
{
    /// <summary>
    /// The profiling information response.
    /// </summary>
    public class ProfilingInformationResponse
    {
        public DateTime? Ts { get; set; }
        public string Info { get; set; }
        public double? Millis { get; set; }
    }
}