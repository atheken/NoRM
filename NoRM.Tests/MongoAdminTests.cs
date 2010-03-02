namespace NoRM.Tests
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text.RegularExpressions;
    using Xunit;

    public class MongoAdminTests : MongoFixture
    {
        [Fact]
        public void ListsAllDatabases()
        {
            var expected = new List<string> {"admin", "temp", "local"};

            //create another database
            using (var mongo = new Mongo(ConnectionString("temp")))
            {
                mongo.GetCollection<FakeObject>().Insert(new FakeObject());
            } 
            
            using (var admin = new MongoAdmin(ConnectionString()))
            {            
                foreach (var db in admin.GetAllDatabases())
                {
                    Assert.Contains(db.Name, expected);
                    expected.Remove(db.Name);
                }
            }
            Assert.Equal(0, expected.Count);
        }
        [Fact]
        public void ListsAllDatabasesThrowsExceptionIfNotConnectedToAdmin()
        {            
            using (var admin = new MongoAdmin(ConnectionString("temp")))
            {
                var ex = Assert.Throws<MongoException>(() => admin.GetAllDatabases());
                Assert.Equal("This command is only valid when connected to admin", ex.Message);
            }
        }

        [Fact]
        public void ReturnsBuildInfo()
        {
            string gitVersion;
            string version;
            using (var process = new Process())
            {
                process.StartInfo = new ProcessStartInfo("mongod", "--version"){ RedirectStandardOutput = true, UseShellExecute = false};
                process.Start();
                using (var stream = process.StandardOutput) 
                {                    
                    var data = stream.ReadToEnd();
                    gitVersion = Regex.Match(data, "git version: ([a-f0-9]+)\r\n").Groups[1].Value;
                    version = Regex.Match(data, "db version v([^,]+),").Groups[1].Value;
                }
            }
            
            using (var admin = new MongoAdmin(ConnectionString()))
            {
                var info = admin.BuildInfo(); 
                Assert.Equal(1d, info.OK);
                Assert.Equal(gitVersion, info.GitVersion);
                Assert.Equal(version, info.Version);
            }            
        }
        [Fact]
        public void BuildInfoThrowsExceptionIfNotConnectedToAdmin()
        {            
            using (var admin = new MongoAdmin(ConnectionString("temp")))
            {
                var ex = Assert.Throws<MongoException>(() => admin.BuildInfo());
                Assert.Equal("This command is only valid when connected to admin", ex.Message);
            }
        }

        [Fact]
        public void ForceSyncDoesSomethingOk()
        {
            using (var admin = new MongoAdmin(ConnectionString()))
            {
                var response = admin.ForceSync(true);
                Assert.Equal(1d, response.OK);
                Assert.Equal(2, response.NumFiles); //don't know what this is
            }
        }
        [Fact]
        public void ForceSyncThrowsExceptionIfNotConnectedToAdmin()
        {
            using (var admin = new MongoAdmin(ConnectionString("temp")))
            {
                var ex = Assert.Throws<MongoException>(() => admin.ForceSync(true));
                Assert.Equal("This command is only valid when connected to admin", ex.Message);
            }
        }
        
        [Fact]
        public void DropsDatabase()
        {
            //create another database
            using (var mongo = new Mongo(ConnectionString("temp")))
            {
                mongo.GetCollection<FakeObject>().Insert(new FakeObject());                
            }           
            using (var admin = new MongoAdmin(ConnectionString("temp")))
            {
                admin.DropDatabase();
            }
            
            using (var admin = new MongoAdmin(ConnectionString()))
            {
                foreach (var db in admin.GetAllDatabases())
                {
                   Assert.NotEqual("temp", db.Name);
                }
            }
        }

        [Fact(Skip="todo")]
        public void GetsServerStatus()
        {                        
            using (var admin = new MongoAdmin(ConnectionString()))
            {
                var status = admin.ServerStatus();
            }            
        } 
    }
}