using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Norm.Configuration;

namespace Norm.Protocol
{
    public class ReplicaSetMember
    {
        static ReplicaSetMember()
        {
            MongoConfiguration.Initialize(j => j.For<ReplicaSetMember>(
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

        public ReplicaSetMember()
        {
            Health = 1;
            LastHeartbeat = DateTime.Now;
        }

        private int _id { get; set; }

        public ReplicaSetState State { get; set; }

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
