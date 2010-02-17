using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Security.Cryptography;

namespace NoRM.BSON
{

    /// <summary>
    /// Shameless-ly ripped off, then slightly altered from samus' implementation on GitHub
    /// http://github.com/samus/mongodb-csharp/blob/f3bbb3cd6757898a19313b1af50eff627ae93c16/MongoDBDriver/OidGenerator.cs
    /// </summary>
    internal static class OidGenerator
    {
        private static int inc;
        private static object inclock = new object();

        private static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private static byte[] machineHash;
        private static byte[] procID;

        static OidGenerator()
        {
            GenerateConstants();
        }

        public static byte[] Generate()
        {
            byte[] oid = new byte[12];
            int copyidx = 0;

            Array.Copy(BitConverter.GetBytes(GenerateTime()), 0, oid, copyidx, 4);
            copyidx += 4;

            Array.Copy(machineHash, 0, oid, copyidx, 3);
            copyidx += 3;

            Array.Copy(OidGenerator.procID, 0, oid, copyidx, 2);
            copyidx += 2;

            Array.Copy(BitConverter.GetBytes(GenerateInc()), 0, oid, copyidx, 3);
            return oid;
        }

        private static int GenerateTime()
        {
            DateTime now = DateTime.Now.ToUniversalTime(); ;
            DateTime nowtime = new DateTime(epoch.Year, epoch.Month, epoch.Day, now.Hour,
                now.Minute, now.Second, now.Millisecond);
            TimeSpan diff = nowtime - epoch;
            return Convert.ToInt32(Math.Floor(diff.TotalMilliseconds));
        }

        private static int GenerateInc()
        {
            lock (OidGenerator.inclock)
            {
                return inc++;
            }
        }

        private static void GenerateConstants()
        {
            OidGenerator.machineHash = GenerateHostHash();
            OidGenerator.procID = BitConverter.GetBytes(GenerateProcId());
        }

        private static byte[] GenerateHostHash()
        {
            MD5 md5 = MD5.Create();
            string host = System.Net.Dns.GetHostName();
            return md5.ComputeHash(Encoding.Default.GetBytes(host));
        }

        private static int GenerateProcId()
        {
            Process proc = Process.GetCurrentProcess();
            return proc.Id;
        }

    }
}

