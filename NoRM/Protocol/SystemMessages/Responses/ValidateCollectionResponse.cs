
namespace Norm.Responses
{
    /// <summary>
    /// The validate collection response.
    /// </summary>
    public class ValidateCollectionResponse : BaseStatusMessage
    {
        public string Ns { get; set; }
        public string Result { get; set; }
        public bool? Valid { get; set; }
        public double? LastExtentSize { get; set; }
    }
}