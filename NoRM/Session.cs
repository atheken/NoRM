using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NoRM.Linq;

namespace NoRM
{
    public class Session
    {
        MongoQueryProvider _provider;

        public Session(String databaseName, String server, int port, bool enableExpandoProps)
        {
            _provider = new MongoQueryProvider(databaseName, server, port, enableExpandoProps);
        }

        public Session(String databaseName)
        {
            _provider = new MongoQueryProvider(databaseName);
        }

        //public IQueryable<Product> Products
        //{
        //    get
        //    {
        //        return new MongoQuery<Product>(_provider);
        //    }
        //}

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
    }
}
