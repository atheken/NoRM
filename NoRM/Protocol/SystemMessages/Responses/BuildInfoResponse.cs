
namespace Norm.Responses
{
    /// <summary>
    /// The build info response.
    /// </summary>
    public class BuildInfoResponse
    {
        public string Version { get; set; }
        public string GitVersion { get; set; }
        public string SysInfo { get; set; }
        public int? bits { get; set; }
        public double? OK { get; set; }
    }
}