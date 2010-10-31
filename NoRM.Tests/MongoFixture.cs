using System;
using System.Diagnostics;
using System.Configuration;

using Xunit;

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

        protected MongoFixture()
        {
            _process = new Process
              {
                  StartInfo = new ProcessStartInfo
                          {
                              UseShellExecute = false,
                              FileName = this.MongodPath,
                              Arguments = string.Format("--port {0} --dbpath {1} --smallfiles {2}",
                                this.Port, this.DataPath, this.Arguments),
                              RedirectStandardError = true,
                              RedirectStandardOutput = true
                          }
              };
            _process.Exited += _process_Exited;
            _process.Start();            
        }
        
        private void _process_Exited(object sender, EventArgs e) {
            if (_process.ExitCode == 0)
            {
                return;
            }

            throw new InvalidOperationException(string.Format(
                "Mongod process exited unexpectedly.{0}Output:{0}{1}.{0}Errors:{0}{2}.",
                Environment.NewLine,
                _process.StandardOutput.ReadToEnd(),
                _process.StandardError.ReadToEnd()
            ));
        }

        public void Dispose()
        {
            if (_process.HasExited)
            {
                return;
            }

            _process.Exited -= _process_Exited;
            _process.Kill();
        }
    }
}