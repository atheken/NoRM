namespace BSONHarness
{
    using NoRM;
    using NoRM.BSON.DbTypes;

    public class User
    {
        public OID _id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }

    internal class Program
    {
        private static void Main(string[] args)
        {
            using (var mongo = new MongoServer("mongodb://karl:pass@localhost/test"))
            {
                var collection = mongo.Database.GetCollection<User>();
                collection.Insert(new User
                                      {
                                          _id = OID.NewOID(),
                                          Name = "goku", 
                                          Email = "goku@dbz.com"
                                      });
            }
        }
    }
}