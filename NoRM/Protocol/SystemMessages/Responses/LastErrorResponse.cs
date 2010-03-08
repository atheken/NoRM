namespace NoRM.Responses
{
    /// <summary>
    /// Indicates what the last error the MongoDB server encountered was.
    /// </summary>
    public class LastErrorResponse
    {
        public long? N { get; set; }
        public string Err { get; set; }
        public double? Ok { get; set; }
        public int Code { get; set; }
    }
}