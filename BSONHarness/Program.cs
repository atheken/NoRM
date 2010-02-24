namespace BSONHarness
{
    using System;
    using NoRM;
    using NoRM.BSON.DbTypes;

    public class User
    {
        public OID _id { get; set;}
        public string Name { get; set; }
        public string Email { get; set; }
    }

    internal class Program
    {
        private static void Main(string[] args)
        {
            using (var mongo = new Mongo("mongodb://localhost/test?strict=true"))
            {
                var collection = mongo.Database.GetCollection<User>();
                try
                {
                    collection.Insert(new User
                    {
                        _id = null,
                        Name = "goku2",
                        Email = "goku2@dbz.com"
                    });
                }
                catch (Exception ex)
                {
                    var x = ex;
                }                
            }

            using (var mongo = new Mongo("mongodb://localhost/test", "strict=false"))
            {
                var collection = mongo.Database.GetCollection<User>();
                try
                {
                    collection.Insert(new User
                    {
                        _id = null,
                        Name = "goku2",
                        Email = "goku2@dbz.com"
                    });
                }
                catch (Exception ex)
                {
                    var x = ex;
                }  
            }
        }
    }
}