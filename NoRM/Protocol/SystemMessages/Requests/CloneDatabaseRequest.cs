using Norm.Configuration;

namespace Norm.Protocol.SystemMessages.Requests
{
	/// <summary>
	/// The clone database request.
	/// </summary>
	internal class CloneDatabaseRequest : ISystemQuery
	{
		/// <summary>
		/// Initializes the <see cref="CloneDatabaseRequest"/> class.
		/// </summary>
		static CloneDatabaseRequest()
		{
			MongoConfiguration.Initialize(c => c.For<CloneDatabaseRequest>(a =>
			{
				a.ForProperty(auth => auth.CloneDatabase).UseAlias("copydb");
				a.ForProperty(auth => auth.SourceDatabaseName).UseAlias("fromdb");
				a.ForProperty(auth => auth.DestinationDatabaseName).UseAlias("todb");
				a.ForProperty(auth => auth.Host).UseAlias("fromhost");
			})
										);
		}

		public int CloneDatabase
		{
			get { return 1; }
		}

		public string SourceDatabaseName { get; set; }
		public string DestinationDatabaseName { get; set; }

		private string _host = "";
		public string Host
		{
			get { return _host; }
			set { _host = value; }
		}
	}
}