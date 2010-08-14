using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Norm.Tests;
using System.Diagnostics;
using System.Configuration;
using System.IO;
using Xunit;
using Norm;
using t = System.Threading;
using Norm.Protocol.SystemMessages.Responses;
using System.Threading;

namespace NoRM.Tests.ConnectionsTests
{
    public class ReplicaSetTests : IDisposable
    {


        private Process _server1 = null;
        private Process _server2 = null;

        private void Configure()
        {
            StartProcess(ref _server1, 64300);
            StartProcess(ref _server2, 64301);

            var n = DateTime.Now;
            while ((DateTime.Now - n).TotalSeconds < 10)
            {
                try
                {
                    //give mongodb enough time to start.
                    var ma = new MongoAdmin("mongodb://localhost:64300/admin");
                    var result = ma.ConfigureReplicaSet(new ReplicaSet
                    {
                        ID = "testSet",
                        Members = new List<ReplicaSetNode> { 
                    new ReplicaSetNode{ Host = "localhost:"+64300.ToString(), Votes = 3, ID = 0},
                    new ReplicaSetNode{ Host = "localhost:"+64301.ToString(), Votes = 2, ID = 1}
                }
                    }, false);
                    break;
                }
                catch
                {
                    //swallow exception because we're just getting things started.
                }
                t.Thread.Sleep(1000);
            }
        }

        public ReplicaSetTests()
        {
            Configure();
        }

        private static void StartProcess(ref Process serverProcess, int i)
        {
            var pathname = ConfigurationManager.AppSettings["replicaSetTestPath"] + "/rs" + i.ToString() + "/";
            Directory.CreateDirectory(pathname);
            Directory.Delete(pathname, true);
            Directory.CreateDirectory(pathname);
            serverProcess = new Process()
            {
                StartInfo = new ProcessStartInfo
                    {
                        FileName = "mongod",
                        Arguments = string.Format("--port {1} --dbpath {0} --smallfiles --replSet testSet", pathname, i),
                    }
            };
            serverProcess.Start();
        }

        [Fact(DisplayName = "REPLICA SET TESTS: We run all the tests together because it takes up to a minute to spin up the replica set")]
        public void RunAllReplicaSetTests()
        {
            this.GetReplicaSetStatusReturnsCorrectInformation();

            //after status response come back, we have a good chance of these passing.
            this.ReplicaSet_ConnectionString_Dynamically_Finds_Nodes();
        }

        public void ReplicaSet_ConnectionString_Dynamically_Finds_Nodes()
        {
            var options = ConnectionOptions.Create("mongodbrs://localhost:64300/admin");
            Assert.Equal(2, options.Servers.Count);
        }

        /// <summary>
        /// Tests to see if the status comes back.
        /// </summary>
        public void GetReplicaSetStatusReturnsCorrectInformation()
        {
            var n = DateTime.Now;
            ReplicaSetStatusResponse result = null;
            while ((DateTime.Now - n).TotalSeconds < 65)
            {
                try
                {
                    using (var ma = new MongoAdmin("mongodb://localhost:64300/admin?pooling=false"))
                    {
                        result = ma.GetReplicaSetStatus();
                        break;
                    }
                }
                catch (Exception ex)
                {
                    //swallow this, since the replica set is not online yet.
                    t.Thread.Sleep(5000);
                }
                
            }

            Assert.NotNull(result);
        }

        public void Dispose()
        {
            _server1.Kill();
            _server2.Kill();
        }
    }
}
