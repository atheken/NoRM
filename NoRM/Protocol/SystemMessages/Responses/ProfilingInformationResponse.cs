using System;

namespace Norm.Responses
{
    /// <summary>
    /// The profiling information response.
    /// </summary>
    public class ProfilingInformationResponse
    {
        /// <summary>TODO::Description.</summary>
        public DateTime? Ts { get; set; }

        /// <summary>TODO::Description.</summary>
        public string Info { get; set; }

        /// <summary>TODO::Description.</summary>
        public double? Millis { get; set; }
    }
}