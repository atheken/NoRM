using System;
using System.Collections.Generic;
using System.Collections;
namespace NoRM {
    public interface IMongoCollection {
        long Count();
        long Count(object template);
        bool DeleteIndex(string indexName, out int numberDeleted);
        bool DeleteIndices(out int numberDeleted);
        string FullyQualifiedName { get; }
        NoRM.Protocol.SystemMessages.Responses.CollectionStatistics GetCollectionStatistics();
        object FindOne(object template);
        IEnumerable Find(object template);
        IEnumerable Find();
        IEnumerable Find(object template, int limit, string fullyQualifiedName);
        IEnumerable Find(object template, int limit);

    }
    
    
    public interface IMongoCollection<T> :IMongoCollection {
        System.Collections.Generic.IEnumerable<T> Find<U>(U template);
        System.Collections.Generic.IEnumerable<T> Find();
        System.Collections.Generic.IEnumerable<T> Find<U>(U template, int limit, string fullyQualifiedName);
        System.Collections.Generic.IEnumerable<T> Find<U>(U template, int limit);
        T FindOne<U>(U template);
        MongoCollection<U> GetChildCollection<U>(string collectionName) where U : class, new();
        void Update<X, U>(X matchDocument, U valueDocument, bool updateMultiple, bool upsert);
        bool Updateable { get; }
        void UpdateMultiple<X, U>(X matchDocument, U valueDocument);
        void UpdateOne<X, U>(X matchDocument, U valueDocument);

    }
}
