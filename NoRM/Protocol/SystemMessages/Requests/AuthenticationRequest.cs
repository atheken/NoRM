namespace NoRM.Protocol.SystemMessages.Requests
{
    internal class AuthenticationRequest
    {
        public int authenticate { get { return 1; } }
        public string nonce { get; set; }
        public string user { get; set; }
        public string key { get; set; }
    }
}
