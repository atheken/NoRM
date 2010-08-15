using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Norm.Configuration;

namespace Norm
{
    /// <summary>
    /// One of the available replica sets on the current server.
    /// </summary>
    public class ReplicaSet
    {

        static ReplicaSet()
        {
            
            MongoConfiguration.Initialize(y=>y.For<ReplicaSet>(k=>k.ForProperty(f=>f.Members).UseAlias("members")));
        }

        /// <summary>
        /// The unique name of this replica set.
        /// </summary>
        public String ID { get; set; }

        /// <summary>
        /// This will always b
        /// </summary>
        public List<ReplicaSetNode> Members { get; set; }
    }

    /// <summary>
    /// One of the available nodes.
    /// </summary>
    public class ReplicaSetNode
    {

        static ReplicaSetNode()
        {
            MongoConfiguration.Initialize(y => y.For<ReplicaSetNode>(k =>
            {
                k.ForProperty(f => f.Votes).UseAlias("votes");
                k.ForProperty(f => f.Host).UseAlias("host");
            }));
        }

        public int ID { get; set; }

        /// <summary>
        /// The host name and port of the node on this replica set.
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// The number votes available to this node.
        /// </summary>
        public int Votes { get; set; }
    }

}
