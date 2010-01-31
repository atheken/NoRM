using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace System.Data.Mongo
{
    public class MongoContext
    {
        /// <summary>
        /// The ip/domain name of the server.
        /// </summary>
        protected String _serverName = "127.0.0.1";
        /// <summary>
        /// The port on which the server is accessible.
        /// </summary>
        protected int _serverPort = 27017;

        protected IPEndPoint _endPoint;

        public MongoContext()
        {
            var entry = Dns.GetHostEntry(this._serverName);
            var ipe = entry.AddressList.First();
            this._endPoint = new IPEndPoint(ipe, this._serverPort);
        }

        /// <summary>
        /// Will provide an object reference to a DB within the current context.
        /// </summary>
        /// <remarks>
        /// I would recommend adding extension methods, or subclassing MongoContext 
        /// to provide strongly-typed members for each database in your context, this
        /// will removed strings except for a localized places - which can should 
        /// reduce typo problems..
        /// </remarks>
        /// <param name="dbName"></param>
        /// <returns></returns>
        public MongoDatabase GetDatabase(String dbName)
        {
            var retval = new MongoDatabase(dbName, this);
            return retval;
        }

        /// <summary>
        /// Constructs a socket to the server.
        /// </summary>
        /// <returns></returns>
        protected TcpClient TCPSocket()
        {
            TcpClient client = new TcpClient(this._serverName, this._serverPort);
            
            return client;
        }

        /// <summary>
        /// Returns a list of databases that already exist on this context.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<String> GetAllDatabases()
        {


            yield break;
        }


    }
}
