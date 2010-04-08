using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Xunit;
using System.Linq;

namespace Norm.Tests
{
    public class MongoAdminTests
    {
        public MongoAdminTests()
        {
            using (var admin = new MongoAdmin(TestHelper.ConnectionString("strict=false")))
            {
                admin.DropDatabase();
            }
        }
        [Fact]
        public void ListsAllDatabases()
        {
            var expected = new List<string> { "admin", "NormTests", "local" };

            //create another database
            using (var mongo = Mongo.Create(TestHelper.ConnectionString()))
            {
                mongo.GetCollection<FakeObject>().Insert(new FakeObject());
            }

            using (var admin = new MongoAdmin(TestHelper.ConnectionString(null, "admin", null, null)))
            {            
                foreach (var db in admin.GetAllDatabases())
                {                   
                    expected.Remove(db.Name);
                }
            }
            Assert.Equal(0, expected.Count);
        }
        [Fact]
        public void ListsAllDatabasesThrowsExceptionIfNotConnectedToAdmin()
        {
            using (var admin = new MongoAdmin(TestHelper.ConnectionString()))
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

            using (var admin = new MongoAdmin(TestHelper.ConnectionString(null, "admin", null, null)))
            {
                var info = admin.BuildInfo(); 
                Assert.Equal(1d, info.Ok);
                Assert.Equal(gitVersion, info.GitVersion);
                Assert.Equal(version, info.Version);
            }            
        }
        [Fact]
        public void BuildInfoThrowsExceptionIfNotConnectedToAdmin()
        {
            using (var admin = new MongoAdmin(TestHelper.ConnectionString()))
            {
                var ex = Assert.Throws<MongoException>(() => admin.BuildInfo());
                Assert.Equal("This command is only valid when connected to admin", ex.Message);
            }
        }

        [Fact]
        public void ForceSyncDoesSomethingOk()
        {
            using (var admin = new MongoAdmin(TestHelper.ConnectionString(null, "admin", null, null)))
            {
                var response = admin.ForceSync(true);
                Assert.Equal(1d, response.Ok);
                Assert.True(response.NumberOfFiles > 0); //don't know what this is
            }
        }
        [Fact]
        public void ForceSyncThrowsExceptionIfNotConnectedToAdmin()
        {
            using (var admin = new MongoAdmin(TestHelper.ConnectionString()))
            {
                var ex = Assert.Throws<MongoException>(() => admin.ForceSync(true));
                Assert.Equal("This command is only valid when connected to admin", ex.Message);
            }
        }
        
        [Fact]
        public void DropsDatabase()
        {
            //create another database
            using (var mongo = Mongo.Create(TestHelper.ConnectionString()))
            {
                mongo.GetCollection<FakeObject>().Insert(new FakeObject());                
            }
            using (var admin = new MongoAdmin(TestHelper.ConnectionString()))
            {
                admin.DropDatabase();
            }

            using (var admin = new MongoAdmin(TestHelper.ConnectionString(null, "admin", null, null)))
            {
                foreach (var db in admin.GetAllDatabases())
                {
                   Assert.NotEqual("NormTests", db.Name);
                }
            }
        }

        [Fact(Skip="todo")]
        public void GetsServerStatus()
        {
            using (var admin = new MongoAdmin(TestHelper.ConnectionString()))
            {
                var status = admin.ServerStatus();
            }            
        } 
        
        [Fact]
        public void ReturnsEmptyInProcessResponse()
        {
            using (var admin = new MongoAdmin(TestHelper.ConnectionString(null, "admin", null, null)))
            {
                var response = admin.GetCurrentOperations();
                Assert.Equal(0, response.Count());
            }
        }

    }
}