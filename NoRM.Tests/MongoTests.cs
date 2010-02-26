namespace NoRM.Tests
{
    using System.Net;
    using Xunit;

    public class MongoTests
    {
        [Fact]
        public void ConnectsToDefaultServerByDefault()
        {
            using (var mongo = new Mongo())
            {
                var endpoint = (IPEndPoint)mongo.ServerConnection().Client.Client.RemoteEndPoint;
                Assert.Equal("127.0.0.1", endpoint.Address.ToString());
                Assert.Equal(27017, endpoint.Port);
            }
        }

        [Fact]
        public void CreatesConnectionWithOverrideOptions()
        {
            using (var mongo = new Mongo("mongodb://localhost/NoRMTests?strict=true", "strict=false"))
            {
                Assert.Equal(false, mongo.ServerConnection().StrictMode);
            }
        }

        [Fact]
        public void GetsTheLastError()
        {
            using (var mongo = new Mongo("mongodb://localhost/NoRMTests?strict=false&pooling=false"))
            {
                mongo.GetCollection<FakeObject>().Insert(new FakeObject { _id = null });
                mongo.GetCollection<FakeObject>().Insert(new FakeObject { _id = null });
                var error = mongo.LastError();
                Assert.Equal(1d, error.Ok);
                Assert.Equal("E11000 duplicate key error index: NoRMTests.FakeObject.$_id_  dup key: { : null }", error.Err);

            }
        }
    }
}