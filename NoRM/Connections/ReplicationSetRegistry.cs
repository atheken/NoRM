using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Configuration;

namespace Norm.Connections
{
    /// <summary>
    /// This class is not useful yet.... 
    /// </summary>
    internal static class ReplicaSetRegistry
    {
        private static object _cleanupLock = new object();
        private static readonly string _appSettingLookup = "NORM_REPLICA_REQUERY_INTERVAL";
        private static Timer _cleanupThread;
        
        static ReplicaSetRegistry()
        {
            int interval = 0;
            if (ConfigurationManager.AppSettings.AllKeys.Any(y => _appSettingLookup == y))
            {
                if (int.TryParse(ConfigurationManager.AppSettings[_appSettingLookup], out interval))
                {
                    interval *= 1000;
                }
                else
                {
                    interval = 120000;
                }
            }
            _cleanupThread = new Timer(interval);
            _cleanupThread.Elapsed += new ElapsedEventHandler(RequeryReplicas);
            _cleanupThread.AutoReset = true;
            _cleanupThread.Start();
        }

        static void RequeryReplicas(object sender, ElapsedEventArgs e)
        {
            lock (_cleanupLock)
            {
                //requery known replica sets.
            }
        }


        public static void Register(this ReplicaSet set)
        {
            throw new NotSupportedException();
        }

        public static ReplicaSet GetSet(String setName)
        {
            throw new NotSupportedException();
        }
    }
}
