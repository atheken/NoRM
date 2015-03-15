using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Norm.Responses;
using Norm.Configuration;

namespace Norm.Responses
{
    public class ReplicaSetConfigReponse : BaseStatusMessage
    {
        static ReplicaSetConfigReponse()
        {
            MongoConfiguration.Initialize(j => j.For<ReplicaSetConfigReponse>(k =>
            {
                k.ForProperty(l => l.InformationMessage).UseAlias("info");
            }));
        }

        /// <summary>
        /// A friendly message on how/what is happening with the just configured ReplicaSet.
        /// </summary>
        public string InformationMessage { get; set; }
    }
}
