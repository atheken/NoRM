namespace NoRM.Tests
{
    using BSON.DbTypes;

    public class FakeObject
    {
        public ObjectId _id{ get; set;}
        
        public FakeObject()
        {
            _id = ObjectId.NewOID();
        }
    }
}