namespace Norm.Responses
{
    /// <summary>
    /// The assert info response.
    /// </summary>
    public class AssertInfoResponse
    {
        public double? OK { get; set; }
        public bool? DBAsserted { get; set; }
        public bool? Asserted { get; set; }
        public string Assert { get; set; }
        public string AssertW { get; set; }
        public string AssertMSG { get; set; }

        /// <summary>
        /// Gets or sets AssertUser.
        /// </summary>
        public string AssertUser { get; set; }
    }
}