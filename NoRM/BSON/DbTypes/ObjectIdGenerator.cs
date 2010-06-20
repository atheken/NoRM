using System;
using System.Diagnostics;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace Norm.BSON.DbTypes
{
    /// <summary>
    /// Shameless-ly ripped off, then slightly altered from samus' implementation on GitHub
    /// http://github.com/samus/mongodb-csharp/blob/f3bbb3cd6757898a19313b1af50eff627ae93c16/MongoDBDriver/ObjectIdGenerator.cs
    /// </summary>
    internal static class ObjectIdGenerator
    {
        /// <summary>
        /// The epoch.
        /// </summary>
        private static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// The inclock.
        /// </summary>
        private static readonly object inclock = new object();

        /// <summary>
        /// The inc.
        /// </summary>
        private static int inc;

        /// <summary>
        /// The machine hash.
        /// </summary>
        private static byte[] machineHash;

        /// <summary>
        /// The proc id.
        /// </summary>
        private static byte[] procID;

        /// <summary>
        /// Initializes static members of the <see cref="ObjectIdGenerator"/> class. 
        /// </summary>
        static ObjectIdGenerator()
        {
            GenerateConstants();
        }

        /// <summary>
        /// Generates a byte array ObjectId.
        /// </summary>
        /// <returns>
        /// </returns>
        public static byte[] Generate()
        {
            var oid = new byte[12];
            var copyidx = 0;

            Array.Copy(BitConverter.GetBytes(GenerateTime()), 0, oid, copyidx, 4);
            copyidx += 4;

            Array.Copy(machineHash, 0, oid, copyidx, 3);
            copyidx += 3;

            Array.Copy(procID, 0, oid, copyidx, 2);
            copyidx += 2;

            Array.Copy(BitConverter.GetBytes(GenerateInc()), 0, oid, copyidx, 3);
            return oid;
        }

        /// <summary>
        /// Generates time.
        /// </summary>
        /// <returns>
        /// The time.
        /// </returns>
        private static int GenerateTime()
        {
            var now = DateTime.Now.ToUniversalTime();

            var nowtime = new DateTime(epoch.Year, epoch.Month, epoch.Day, now.Hour, now.Minute, now.Second, now.Millisecond);
            var diff = nowtime - epoch;
            return Convert.ToInt32(Math.Floor(diff.TotalMilliseconds));
        }

        /// <summary>
        /// Generate an increment.
        /// </summary>
        /// <returns>
        /// The increment.
        /// </returns>
        private static int GenerateInc()
        {
            lock (inclock)
            {
                return inc++;
            }
        }

        /// <summary>
        /// Generates constants.
        /// </summary>
        private static void GenerateConstants()
        {
            machineHash = GenerateHostHash();
            procID = BitConverter.GetBytes(GenerateProcId());
        }

        /// <summary>
        /// Generates a host hash.
        /// </summary>
        /// <returns>
        /// </returns>
        private static byte[] GenerateHostHash()
        {
            using (var md5 = MD5.Create())
            {
                var host = Dns.GetHostName();
                return md5.ComputeHash(Encoding.Default.GetBytes(host));
            }
        }

        /// <summary>
        /// Generates a proc id.
        /// </summary>
        /// <returns>
        /// Proc id.
        /// </returns>
        private static int GenerateProcId()
        {
            var proc = Process.GetCurrentProcess();
            return proc.Id;
        }
    }
}