using System;
using System.Diagnostics;
using System.Configuration;
using System.Reflection;
using System.IO;
using NUnit.Framework;
using System.Text.RegularExpressions;
using t = System.Threading;
using System.Net.Sockets;

namespace Norm.Tests
{
	[TestFixture]
	public class StartupHelperHarness
	{
		private Mongod _proc;
		
		[TestFixtureSetUp]
		public void SetUp ()
		{
			_proc = new Mongod ();
		}

		[TestFixtureTearDown]
		public void TearDown ()
		{
			_proc.Dispose ();
		}

		[Test]
		public void TestServerHarness ()
		{
			using (var db = new MongoAdmin ("mongodb://localhost:27701"))
			{
				var d = db.GetCurrentOperations ();
				Assert.IsNotNull (d);
			}
		}
	}

	public class Mongod : IDisposable
	{
		/// <summary>
		/// The path to the MongoDB "stuff"
		/// </summary>
		private static String MongodPath {
			get { return ConfigurationManager.AppSettings["mongodPath"]; }
		}

		private static String TestAssemblyPath {
			get {
				var path = Assembly.GetAssembly (typeof(Mongod)).Location;
				return Regex.Match (path, "(?<directoryPart>.+[\\/]{1}).+?").Groups["directoryPart"].Value;
			}
		}

		private static void CreateTestDataDir (String path)
		{
			if (Directory.Exists (path)) {
				Directory.Delete (path, true);
			}
			Directory.CreateDirectory (path);
		}

		private Process _server_process;

		public Mongod ()
		{
			_server_process = new Process ();
			var dataDir =TestAssemblyPath + "/data/";
			CreateTestDataDir (dataDir);

            string arguments = string.Format ("--port {1} --dbpath {0} --smallfiles",
					dataDir,
					Int32.Parse(ConfigurationManager
						.AppSettings["testPort"] ?? "27701"));

            string executableName = Path.Combine(MongodPath, "mongod");
			
			_server_process.StartInfo = new ProcessStartInfo { FileName = executableName, Arguments = arguments, UseShellExecute = false, CreateNoWindow=true };
			_server_process.Start();
		}

		public void Dispose ()
		{
            try
            {
                _server_process.Kill();
                _server_process.WaitForExit(200);
                _server_process.Close();
            }
            catch
            {
            }
		}
	}
}

