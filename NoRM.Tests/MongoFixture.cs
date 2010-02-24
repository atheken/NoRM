namespace NoRM.Tests
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;

    public class MongoFixture : IDisposable
    {
        private const string _dataPath = "c:/data/testing/";
        protected const int Port = 27018;
        private readonly Process _process;
        
        protected string ConnectionString()
        {
            return ConnectionString(null);
        }
        protected string ConnectionString(string database)
        {
            return ConnectionString(database, null);
        }       
        protected string ConnectionString(string database, string options)
        {
            var connectionString  = string.Format("mongodb://localhost:{0}", Port);
            if (string.IsNullOrEmpty(database))
            {
                database = "admin";
            }
            connectionString = string.Concat(connectionString, '/', database);
            
            if (string.IsNullOrEmpty(options))
            {
                options = "pooling=false";
                
            }
            connectionString = string.Concat(connectionString, '?', options);
            
            return connectionString;
        }
        
        public MongoFixture()
        {
            Directory.CreateDirectory(_dataPath);            
            _process = new Process 
            {
                StartInfo = new ProcessStartInfo
                                {
                                    FileName = "mongod",
                                    Arguments = string.Format("--port {0} --dbpath {1} --smallfiles", Port, _dataPath),
                                },
            };
            _process.Start();
        }
        
        public void Dispose()
        {        
            _process.Kill();            
            Thread.Sleep(500); //arrg, this isn't going to do
            Directory.Delete(_dataPath, true);         
        }
    }
}