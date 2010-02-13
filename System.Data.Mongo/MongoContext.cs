using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Data.Mongo.Protocol.Messages;
using System.Security.Cryptography;
using System.Data.Mongo.Protocol.SystemMessages.Responses;
using System.Data.Mongo.Protocol.SystemMessages.Requests;

namespace System.Data.Mongo
{
    public class MongoContext
    {
        private static MD5 _md5 = MD5.Create();
        /// <summary>
        /// This indicates if the context should load properties 
        /// that are not part of a given class definition into a 
        /// special flyweight lookup. 
        /// 
        /// Disabled by default.
        /// </summary>
        /// <remarks>
        /// This is useful when the class definition you want to use doesn't support a particular property, but the database should 
        /// still maintain it, or you do not want to squash it on save.
        /// 
        /// Enabling this will cause additinal overhead when loading/saving, as well as more memory consumption during the lifetime of the object.
        /// </remarks>
        public bool EnableExpandoProperties
        {
            get;
            private set;
        }

        /// <summary>
        /// Number of seconds to wait for a response from the server before throwing a timeout exception.
        /// Defaults to 30.
        /// </summary>
        public int QueryTimeout
        {
            get;
            set;
        }
        /// <summary>
        /// The ip/domain name of the server.
        /// </summary>
        protected String _serverName = "127.0.0.1";
        /// <summary>
        /// The port on which the server is accessible.
        /// </summary>
        protected int _serverPort = 27017;

        protected IPEndPoint _endPoint;

        /// <summary>
        /// Specify the host and the port to connect to for the mongo db.
        /// </summary>
        /// <param name="server">The server IP or hostname (127.0.0.1 is the default)</param>
        /// <param name="port">The port on which mongo is running (27017 is the default)</param>
        /// <param name="enableExpandoProps">Should requests to this database push/pull props from the DB that are not part of the specified object?</param>
        public MongoContext(String server, int port, bool enableExpandoProps)
        {
            this.QueryTimeout = 30;
            this._serverName = server;
            this._serverPort = port;
            this.EnableExpandoProperties = enableExpandoProps;
        }


        /// <summary>
        /// Attempt to authenticate this user.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool Authenticate(String username, String password)
        {
            bool retval = false;

            //HACK: not sure how to send a query without selecting a database, so I am hooking admin. my bad.
            var qm = new QueryMessage<GetNonceResponse, GetNonceRequest>(this, "admin.$cmd");
            qm.NumberToSkip = 0;
            qm.NumberToTake = 1;
            qm.Query = new GetNonceRequest();
            var nonce = qm.Execute().Results.First();

            if (nonce.OK == 1)
            {
                //TODO: arg! the docs for this on Mongo's site are terrible!
            }

            return retval;
        }


        /// <summary>
        /// Creates a context that will connect to 127.0.0.1:27017 (MongoDB on the default port).
        /// </summary>
        /// <remarks>
        /// This also disabled Expando props for documents.
        /// </remarks>
        public MongoContext()
            : this("127.0.0.1", 27017, false)
        {

        }

        /// <summary>
        /// Will provide an object reference to a DB within the current context.
        /// </summary>
        /// <remarks>
        /// I would recommend adding extension methods, or subclassing MongoContext 
        /// to provide strongly-typed members for each database in your context, this
        /// will localize Strings to a few places - reducing typo issues.
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
        internal TcpClient ServerConnection()
        {
            return new TcpClient(this._serverName, this._serverPort);

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
