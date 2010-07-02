using System;
using Norm.Collections;
using Norm.Responses;
namespace Norm
{
    /// <summary>
    /// <see cref="Norm.Mongo"/>
    /// </summary>
    public interface IMongo : IDisposable
    {
        IConnectionProvider ConnectionProvider { get; }
        IMongoDatabase Database { get; }
        IMongoCollection<T> GetCollection<T>(string collectionName);
        IMongoCollection<T> GetCollection<T>();
        LastErrorResponse LastError();
    }
}
