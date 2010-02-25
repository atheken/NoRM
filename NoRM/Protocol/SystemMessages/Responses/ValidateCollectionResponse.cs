namespace NoRM.Protocol.SystemMessages.Responses
{
    public class ValidateCollectionResponse
    {
        public string Ns { get; set; }
        public string Result { get; set; }
        public bool? Valid { get; set; }
        public double? LastExtentSize { get; set; }
        public double? OK { get; set; }
    }
}
