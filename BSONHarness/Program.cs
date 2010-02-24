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
            using (var mongo = new Mongo("mongodb://usr:8e156e298e19afdc3a104ddd172a2bee@localhost/test?strict=true"))
            {
                var collection = mongo.Database.GetCollection<User>();
                collection.Insert(new User
                                      {
                                          _id = OID.NewOID(),
                                          Name = "goku2", 
                                          Email = "goku2@dbz.com"
                                      });
            }
        }
    }
}