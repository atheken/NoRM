namespace NoRM.Tests
{
    public class FakeObject
    {
        public ObjectId Id{ get; set;}
        
        public FakeObject()
        {
            Id = ObjectId.NewObjectId();
        }
    }
}