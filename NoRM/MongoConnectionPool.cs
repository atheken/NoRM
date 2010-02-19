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
        private static Queue<MongoConnection> _connectionPool;
        private static bool _initialized;
        private static int _connectionCount = 0;

        private MongoConnectionPool()
        {
        }


        // TODO add in replication support here for left/right/slave
        public static void Initialize()
        {
            if (_initialized) return;
            if (_connectionPool == null) _connectionPool = new Queue<MongoConnection>();


            for (int i = 0; i < MIN_POOL_SIZE; i++)
            {
                MongoConnection conn = new MongoConnection();

                PutConnection(conn);
            }

            _initialized = true;
        }


        public static MongoConnection GetConnection()
        {
            if (_connectionPool.Count > 0)
            {
                lock (_connectionPool)
                {
                    MongoConnection conn = null;
                    while (_connectionPool.Count > 0)
                    {
                        conn = _connectionPool.Dequeue();

                        if (conn.Connected)
                        {
                            return conn;
                        }
                        else
                        {
                            conn.Close();
                            Interlocked.Decrement(ref _connectionCount);
                        }
                    }
                }
            }
            return MongoConnectionPool.OpenConnection();
        }

        private static void PutConnection(MongoConnection conn)
        {
            lock (_connectionPool)
            {
                if (_connectionPool.Count < MAX_POOL_SIZE)
                {
                    if (conn != null)
                    {
                        if (conn.Connected)
                        {
                            _connectionPool.Enqueue(conn);
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
            if (_connectionCount < MAX_POOL_SIZE)
            {
                MongoConnection conn = new MongoConnection();

                Interlocked.Increment(ref _connectionCount);

                PutConnection(conn);

                return conn;
            }

            throw new Exception("Connection pool has reached it's limit: " + MAX_POOL_SIZE);
        }

    }
}
