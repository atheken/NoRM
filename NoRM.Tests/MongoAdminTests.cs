using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using NUnit.Framework;
using System.Linq;
using System;

namespace Norm.Tests
{
    public class IMongoAdminTests
    {
		[SetUp]
        public void Setup()
        {
            using (var admin = new MongoAdmin(TestHelper.ConnectionString("strict=false")))
            {
                admin.DropDatabase();
            }
        }

        [Test]
        public void Set_Profile_Level_Changes_Profile_Level_And_Reports_Change()
        {
            using (var mAdmin = new MongoAdmin(TestHelper.ConnectionString()))
            {
                int prev;
                if (mAdmin.SetProfileLevel(2, out prev))
                {
                    mAdmin.SetProfileLevel(prev);
                }
            }

        }


        [Test]
        public void Get_Assert_Info_Returns_Results()
        {
            using (var mAdmin = new MongoAdmin(TestHelper.ConnectionString()))
            {
                var aInfo = mAdmin.AssertionInfo();
                Assert.NotNull(aInfo);
                Assert.AreEqual(true, aInfo.WasSuccessful);
            }

        }

        [Test]
        public void Get_CurrentOp_Returns()
        {
            using (var mAdmin = new MongoAdmin(TestHelper.ConnectionString()))
            {
                var currOps = mAdmin.GetCurrentOperations().ToArray();
            }

        }

        [Test]
        public void Get_CurrentOp_Returns_Results()
        {
            using (var mAdmin = new MongoAdmin(TestHelper.ConnectionString()))
            {
                var a = mAdmin.PreviousErrors();
                Assert.AreEqual(true, a.WasSuccessful);
            }
        }

        [Test]
        public void Get_Current_Profile_Level()
        {
            using (var mAdmin = new MongoAdmin(TestHelper.ConnectionString()))
            {
                var level = mAdmin.GetProfileLevel();
                if (level != 2d)
                {
                    mAdmin.SetProfileLevel(2);

                }
                Assert.AreEqual(2, mAdmin.GetProfileLevel());
                mAdmin.SetProfileLevel(level);
            }
        }

        [Test]
        public void Repair_Database_Returns()
        {
            using (var mAdmin = new MongoAdmin(TestHelper.ConnectionString()))
            {
                Assert.True(mAdmin.RepairDatabase(false, false));
            }
        }


        [Test]
        public void Kill_Operation_Returns()
        {
            //since we don't have any long-running ops, this is all we can test without mocks.
            using (var mAdmin = new MongoAdmin("mongodb://127.0.0.1/admin"))
            {
                var x = mAdmin.KillOperation(double.MaxValue);
                Assert.AreEqual(false,x.WasSuccessful);
                Assert.AreEqual("no op number field specified?",x["err"]);
            }
        }


        [Test]
        public void Reset_Last_Error_Returns()
        {
            using (var mAdmin = new MongoAdmin(TestHelper.ConnectionString()))
            {
                Assert.True(mAdmin.ResetLastError());
            }
        }


        [Test]
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
            Assert.AreEqual(0, expected.Count);
        }
        [Test]
        public void ListsAllDatabasesThrowsExceptionIfNotConnectedToAdmin()
        {
            using (var admin = new MongoAdmin(TestHelper.ConnectionString()))
            {
                var ex = Assert.Throws<MongoException>(() => admin.GetAllDatabases());
                Assert.AreEqual("This command is only valid when connected to admin", ex.Message);
            }
        }

        [Test]
        public void ReturnsBuildInfo()
        {
            string gitVersion;
            string version;
            using (var process = new Process())
            {
                process.StartInfo = new ProcessStartInfo("mongod", "--version") { RedirectStandardOutput = true, UseShellExecute = false };
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
                Assert.AreEqual(true, info.WasSuccessful);
                Assert.AreEqual(gitVersion, info.GitVersion);
                Assert.AreEqual(version, info.Version);
            }
        }
        [Test]
        public void BuildInfoThrowsExceptionIfNotConnectedToAdmin()
        {
            using (var admin = new MongoAdmin(TestHelper.ConnectionString()))
            {
                var ex = Assert.Throws<MongoException>(() => admin.BuildInfo());
                Assert.AreEqual("This command is only valid when connected to admin", ex.Message);
            }
        }

        [Test]
        public void ForceSyncDoesSomethingOk()
        {
            using (var admin = new MongoAdmin(TestHelper.ConnectionString(null, "admin", null, null)))
            {
                if (!admin.BuildInfo().SystemInformation.ToLower().Contains("windows"))
                {
                    var response = admin.ForceSync(true);
                    Assert.AreEqual(true, response.WasSuccessful);
                    Assert.True(response.NumberOfFiles > 0); //don't know what this is
                }
                else
                {
                    Assert.Throws(typeof(MongoException), () => admin.ForceSync(true));
                    Console.WriteLine("FSync is not supported on windows version of MongoDB and will throw an exception.");
                }
            }
        }
        [Test]
        public void ForceSyncThrowsExceptionIfNotConnectedToAdmin()
        {
            using (var admin = new MongoAdmin(TestHelper.ConnectionString()))
            {
                var ex = Assert.Throws<MongoException>(() => admin.ForceSync(true));
                Assert.AreEqual("This command is only valid when connected to admin", ex.Message);
            }
        }

        [Test]
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
                    Assert.AreNotEqual("NormTests", db.Name);
                }
            }
        }

        [Test]
        public void GetsServerStatus()
        {
            using (var admin = new MongoAdmin(TestHelper.ConnectionString()))
            {
                var status = admin.ServerStatus();
                Assert.AreEqual(true, status.WasSuccessful);
            }
        }

        [Test]
        public void ReturnsEmptyInProcessResponse()
        {
            using (var admin = new MongoAdmin(TestHelper.ConnectionString(null, "admin", null, null)))
            {
                var response = admin.GetCurrentOperations();
                Assert.AreEqual(0, response.Count());
            }
        }

    }
}