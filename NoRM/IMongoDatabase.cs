using System;
using Norm.Collections;
using Norm.Responses;
using Norm.Protocol.SystemMessages;
using System.Collections.Generic;
namespace Norm
{
    /// <summary>
    /// <see cref="Norm.MongoDatabase"/>
    /// </summary>
    public interface IMongoDatabase
    {
        bool CreateCollection(CreateCollectionOptions options);
        MapReduce CreateMapReduce();
        IConnection CurrentConnection { get; }
        string DatabaseName { get; }
        bool DropCollection(string collectionName);
        IEnumerable<CollectionInfo> GetAllCollections();
        IMongoCollection<T> GetCollection<T>(string collectionName);
        IMongoCollection GetCollection(string collectionName);
        IMongoCollection<T> GetCollection<T>();
        CollectionStatistics GetCollectionStatistics(string collectionName);
        IEnumerable<ProfilingInformationResponse> GetProfilingInformation();
        LastErrorResponse LastError();
        LastErrorResponse LastError(int verifyCount);
        LastErrorResponse LastError(int waitCount, int waitTimeout);
        SetProfileResponse SetProfileLevel(ProfileLevel level);
        ValidateCollectionResponse ValidateCollection(string collectionName, bool scanData);
    }
}
