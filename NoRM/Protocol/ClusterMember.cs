using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Norm.Configuration;

namespace Norm.Protocol
{
    /// <summary>
    /// Defines a node of a cluster.
    /// </summary>
    public class ClusterMember
    {
        static ClusterMember()
        {
            MongoConfiguration.Initialize(j => j.For<ClusterMember>(
                r =>
                {
                    r.ForProperty(l => l.ServerName).UseAlias("name");
                    r.ForProperty(l => l.ErrorMessage).UseAlias("errormsg");
                    r.ForProperty(l => l.Health).UseAlias("health");
                    r.ForProperty(l => l.LastHeartbeat).UseAlias("lastHeartbeat");
                    r.ForProperty(l => l.Uptime).UseAlias("uptime");
                    r.ForProperty(l => l.State).UseAlias("state");
                }));
        }

        public ClusterMember()
        {
            Health = 1;
            LastHeartbeat = DateTime.Now;
        }

        private int _id { get; set; }

        private static int DEFAULT_PORT = 27017;

        private String[] SplitServerName()
        {
            return this.ServerName.Split(':');
        }

        /// <summary>
        /// Produce the "port" part of the server name, or the default MongoDB port (27017)
        /// </summary>
        /// <returns></returns>
        public int GetPort()
        {
            int outval = DEFAULT_PORT;
            var s = this.SplitServerName();
            if (s.Length == 2)
            {
                outval = Int32.Parse(s[1]);
            }

            return outval;
        }

        /// <summary>
        /// Produce the "host" part of the servername"
        /// </summary>
        /// <returns></returns>
        public string GetHost()
        {
            return this.SplitServerName()[0];
        }

        public MemberStatus State { get; set; }

        /// <summary>
        /// Is this the server from which the replica set information was pulled?
        /// </summary>
        /// <remarks>
        /// Just so that the deserializer is happy.
        /// </remarks>
        private bool self { get; set; }

        /// <summary>
        /// The host (and optionally, port) of the member server.
        /// </summary>
        public String ServerName { get; set; }
        /// <summary>
        /// An informational message about the condition of the server (not always an "error", based on the mongodb docs.)
        /// </summary>
        public String ErrorMessage { get; set; }

        /// <summary>
        /// The current state of the server "1" is good.
        /// </summary>
        public double Health { get; set; }

        /// <summary>
        /// If known, how long the server has been up in seconds.
        /// </summary>
        public int? Uptime { get; set; }

        /// <summary>
        /// The last moment when this server was contacted.
        /// </summary>
        public DateTime LastHeartbeat { get; set; }
    }
}
