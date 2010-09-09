using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Norm.Responses;
using Norm.Configuration;

namespace Norm.Protocol.SystemMessages.Responses
{

    public class ReplicaSetStatusResponse : BaseStatusMessage
    {
        static ReplicaSetStatusResponse()
        {
            MongoConfiguration.Initialize(k =>
            k.For<ReplicaSetStatusResponse>(j =>
            {
                j.ForProperty(l => l.SetName).UseAlias("set");
                j.ForProperty(l => l.Date).UseAlias("date");
                j.ForProperty(l => l.Members).UseAlias("members");
                j.ForProperty(l => l.State).UseAlias("myState");

            }));
        }
        public String SetName { get; set; }
        public DateTime Date { get; set; }
        public MemberStatus State { get; set; }

        private IList<ClusterMember> _members;
        public IList<ClusterMember> Members
        {
            get
            {
                return this._members;
            }
            set
            {
                _members = (value ?? new List<ClusterMember>())
                    .ToList();
            }
        }
    }
}
