using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace NoRM
{
    public class MongoConnectionPool
    {
        private static readonly int MAX_POOL_SIZE = 50;
        private static readonly int MIN_POOL_SIZE = 10;
        private static Queue<MongoConnection> _availableConnections;
        private static bool _initialized;
        private static int _checkedOut = 0;


        private MongoConnectionPool()
        {
        }


        // TODO add in replication support here for left/right/slave
        public static void Initialize()
        {
            if (_initialized) return;
            if (_availableConnections == null) _availableConnections = new Queue<MongoConnection>();


            for (int i = 0; i < MIN_POOL_SIZE; i++)
            {
                MongoConnection conn = new MongoConnection();

                AddConnection(conn);
            }

            _initialized = true;
        }


        public static MongoConnection GetConnection()
        {
            Interlocked.Increment(ref _checkedOut);
            if (_availableConnections.Count > 0)
            {
                lock (_availableConnections)
                {
                    MongoConnection conn = null;

                    while (_availableConnections.Count > 0)
                    {
                        conn = _availableConnections.Dequeue();

                        if (conn.Connected)
                        {
                            return conn;
                        }
                        else
                        {
                            conn.Close();
                        }
                    }
                }
            }

            return MongoConnectionPool.OpenConnection();
        }

        public static void AddConnection(MongoConnection conn)
        {
            lock (_availableConnections)
            {
                if (_availableConnections.Count < MAX_POOL_SIZE)
                {
                    if (conn != null)
                    {
                        if (conn.Connected)
                        {
                            _availableConnections.Enqueue(conn);
                        }
                        else
                        {
                            conn.Close();
                        }
                    }
                }
                else
                {
                    conn.Close();
                }
            }
        }

        private static MongoConnection OpenConnection()
        {
            if (_checkedOut < MAX_POOL_SIZE)
            {
                MongoConnection conn = new MongoConnection();

                AddConnection(conn);

                return conn;
            }

            throw new Exception("Connection pool has reached it's limit: " + MAX_POOL_SIZE);
        }

    }
}
