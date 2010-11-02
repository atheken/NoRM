using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;

using NUnit.Framework;

namespace Norm.Tests
{

    public abstract class MongoFixture : IDisposable
    {
        private readonly Process _process;

        protected abstract string DataPath { get; }
        protected virtual int Port
        {
            get { return 27018; }
        }
        protected virtual string Arguments
        {
            get { return string.Empty; }
        }
        protected virtual string MongodPath {
            get { return ConfigurationManager.AppSettings["mongodPath"]; }
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
            var connectionString = string.Format("mongodb://localhost:{0}", Port);
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

        public MongoFixture()
        {
            _process = new Process
              {
                  StartInfo = new ProcessStartInfo
                          {
                              UseShellExecute = false,
                              FileName = Path.Combine(this.MongodPath, "mongo"),
                              Arguments = string.Format("--port {0} --dbpath {1} --smallfiles {2}",
                                this.Port, this.DataPath, this.Arguments)
                          }
              };
            _process.Start();            
        }
        
        public void Dispose()
        {
            _process.Kill();
        }
    }
}