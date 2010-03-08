using System;
using System.Linq;

namespace NoRM.Linq
{
    public class MongoTypedSession<T> : MongoSession where T : class, new()
    {
        private readonly string _connectionString;

        public MongoTypedSession(string connectionString) : base(connectionString)
        {
            _connectionString = connectionString;
        }

        public IQueryable<T> Query
        {
            get { return new MongoQuery<T>(Provider); }
        }

        public void Add(T item) 
        {
            Provider.Mongo.GetCollection<T>().Insert(item);
        }

        public void Add<TK>(TK item) where TK : class, new()
        {
            Provider.Mongo.GetCollection<TK>().Insert(item);
        }

        public void Update(T item)
        {
            Provider.Mongo.GetCollection<T>().UpdateOne(item, item);
        }

        public void Update<TK>(TK item) where TK : class, new()
        {
            Provider.Mongo.GetCollection<TK>().UpdateOne(item, item);
        }

        public void Drop<TK>()
        {
            Provider.Mongo.Database.DropCollection(typeof(TK).Name);
        }

        public void CreateCappedCollection(string name)
        {
            Provider.Mongo.Database.CreateCollection(new CreateCollectionOptions(name));
        }
    }
}
