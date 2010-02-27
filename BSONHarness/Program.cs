namespace BSONHarness
{    
    using NoRM;
    using NoRM.BSON.DbTypes;

    public class User
    {
        public ObjectId _id { get; set;}
        public string Name { get; set; }
        public string Email { get; set; }
    }

    internal class Program
    {
        private static void Main(string[] args)
        {
            using (var mongo = new Mongo("mongodb://localhost/test?pooling=false"))
            {
                var collection = mongo.GetCollection<User>();
                collection.Insert(new User
                {
                    Name = "goku2",
                    Email = "goku2@dbz.com"
                });             
            }            
        }
    }
}