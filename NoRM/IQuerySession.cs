using System;
using System.Linq;
namespace NoRM
{
    /// <summary>
    /// A simplified interface for interacting with a queryable persistence store.
    /// </summary>
    public interface IQuerySession : IDisposable
    {
        void Add<T>(T item) where T : class, new();
        
        void Add<T>(T item, string collectionName) where T : class, new();
        
        void Drop<T>();
        
        /// <summary>
        /// Remove a collection with the specified name from this session's DB.
        /// </summary>
        /// <param name="name"></param>
        void Drop(string name);
        
        IQueryable<T> GetCollection<T>();

        IQueryable<T> GetCollection<T>(string collectionName);

        void Update<T>(T item) where T : class, new();

        void Update<T>(T item, string collectionName) where T : class, new();
    }
}
