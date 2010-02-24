namespace NoRM.Tests
{
    using BSON.DbTypes;

    public class FakeObject
    {
        public OID _id{ get; set;}
        
        public FakeObject()
        {
            _id = OID.NewOID();
        }
    }
}