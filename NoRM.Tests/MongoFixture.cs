namespace NoRM.Tests
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;

    public abstract class MongoFixture : IDisposable
    {
        private readonly Process _process;
        
        protected virtual string DataPath
        {
            get{ return "c:/data/NoRMTesting/";}
        }
        protected virtual int Port
        {
            get { return 27018;}
        }
        protected virtual bool WipeDataDirectory
        {
            get { return true; }
        }
        protected virtual string Arguments
        {
            get { return string.Empty; }
        }
        
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
            return string.Concat(connectionString, '?', options);
        }

        protected MongoFixture()
        {
            if (WipeDataDirectory) { Directory.CreateDirectory(DataPath); }

            _process = new Process
                           {
                               StartInfo = new ProcessStartInfo
                                       {
                                           FileName = "mongod",
                                           Arguments = string.Format("--port {0} --dbpath {1} --smallfiles {2}", Port, DataPath, Arguments),
                                       },
            };
            _process.Start();                        
        }
        
        public void Dispose()
        {        
            _process.Kill();
            Thread.Sleep(500); //arrg, this isn't going to do, but mongod seems sensitive to being spun up and down really quick - acting quite strange without this delay
            if (WipeDataDirectory)
            {                
                Directory.Delete(DataPath, true);
            }
        }
    }
}