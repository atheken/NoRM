using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NoRM.Linq;

namespace NoRM
{
    public class MongoSession : IQuerySession
    {
        MongoQueryProvider _provider;

        /// <summary>
        /// A lightweight way to interact with a MongoDB database.
        /// </summary>
        /// <param name="databaseName"></param>
        /// <param name="server"></param>
        /// <param name="port"></param>
        /// <param name="enableExpandoProps"></param>
        public MongoSession(String databaseName, String server, int port, bool enableExpandoProps)
        {
            this._provider = new MongoQueryProvider(databaseName, server, port, enableExpandoProps);
            this.Database = databaseName;
        }

        /// <summary>
        /// A convenince overload that connects a seesion to mongodb on the default local port with expando props disabled.
        /// </summary>
        /// <param name="databaseName"></param>
        public MongoSession(String databaseName)
            : this(databaseName, "127.0.0.1", 27017, false)
        {
        }

        //public IQueryable<Product> Products
        //{
        //    get
        //    {
        //        return new MongoQuery<Product>(_provider);
        //    }
        //}

        /// <summary>
        /// The database with which this session is interacting.
        /// </summary>
        public String Database { get; private set; }

        /// <summary>
        /// Produces a way to query MongoCollections from the db.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IQueryable<T> GetCollection<T>()
        {
            return new MongoQuery<T>(_provider);
        }

        public IQueryable<T> GetCollection<T>(String collectionName)
        {
            return new MongoQuery<T>(_provider, collectionName);
        }

        /// <summary>
        /// Insert an element into the collection that will hold this type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        public void Add<T>(T item) where T : class, new()
        {
            this.Add(item, typeof(T).Name);
        }

        /// <summary>
        /// Insert an element into the specified collection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        public void Add<T>(T item, String collectionName) where T : class, new()
        {
            var coll = _provider.DB.GetCollection<T>(collectionName);
            //see if the item exists
            coll.Insert(item);
        }

        /// <summary>
        /// Pass in an updated version of T, "ID", or "_id" must be set in 
        /// order for Mongo to properly find the document you wish to update.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        public void Update<T>(T item) where T : class, new()
        {
            this.Update(item, typeof(T).Name);
        }

        public void Update<T>(T item, String collectionName) where T : class, new()
        {
            var coll = _provider.DB.GetCollection<T>(collectionName);
            //see if the item exists
            coll.UpdateOne(item, item);
        }

        /// <summary>
        /// Delete all elements and remove the collection having the specified type from the database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void Drop<T>()
        {
            _provider.DB.DropCollection(typeof(T).Name);
        }

        /// <summary>
        /// Delete all elements and remove the collection having the specified name from the database.
        /// </summary>
        /// <param name="name"></param>
        public void Drop(String name)
        {
            _provider.DB.DropCollection(name);
        }

        /// <summary>
        /// Cleans up the session.
        /// </summary>
        public void Dispose()
        {
            //clean up any resources if necessary.
        }

    }
}
