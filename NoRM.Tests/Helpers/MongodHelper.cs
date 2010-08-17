using System;
using System.Diagnostics;
using System.Configuration;
using System.Reflection;
using System.IO;
using NUnit.Framework;
using System.Text.RegularExpressions;
using t = System.Threading;

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
			
			_server_process.StartInfo = new ProcessStartInfo { FileName = MongodPath + "mongod", 
				Arguments = string.Format ("--port {1} --dbpath {0} --smallfiles",
					dataDir,
					Int32.Parse(ConfigurationManager
						.AppSettings["testPort"] ?? "27701")), UseShellExecute = true};
			_server_process.Start();
		}

		public void Dispose ()
		{
		//	_server_process.Kill ();
		}
	}
}

