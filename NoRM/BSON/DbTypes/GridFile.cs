using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Norm.Collections;
using Norm.Configuration;

namespace Norm.BSON.DbTypes
{
    /// <summary>
    /// Allows for the saving of filestreams into the DB with meta data.
    /// </summary>
    public class GridFile
    {
        /// <summary>
        /// Opens a file from the default namespace "fs"
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        public static GridFile OpenFile(Mongo db, ObjectId fileKey)
        {
            return GridFile.OpenFile(db.GetCollection<Object>("fs"), fileKey);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="db"></param>
        /// <param name="fileKey"></param>
        /// <returns></returns>
        public static GridFile OpenFile(IMongoCollection collection, ObjectId fileKey)
        {
            GridFile retval = new GridFile(collection, fileKey);
            return retval;
        }

        /// <summary>
        /// Construct a file from the db.
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        public static GridFile CreateFile(Mongo db)
        {
            GridFile retval = new GridFile(db.GetCollection<Object>("fs"));
            return retval;
        }

        /// <summary>
        /// Open a grid file from the collection
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="fileKey"></param>
        private GridFile(IMongoCollection collection, ObjectId fileKey)
        {

        }

        private GridFile(IMongoCollection collection)
        {

        }

        /// <summary>
        /// Writes the information to the file stream.
        /// </summary>
        public void Save()
        {

        }


        protected class FileChunk
        {
            /// <summary>
            /// Causes the property mapping to kick in.
            /// </summary>
            static FileChunk()
            {
                MongoConfiguration.Initialize(cfg =>
                    cfg.For<FileChunk>(j =>
                    {
                        j.UseCollectionNamed("chunks");
                        j.ForProperty(k => k.SequenceID).UseAlias("n");
                        j.ForProperty(k => k.FileID).UseAlias("files_id");
                        j.ForProperty(k => k.Payload).UseAlias("data");
                    })
                );
            }
            /// <summary>
            /// The unique id for this chunk.
            /// </summary>
            public ObjectId ID { get; set; }

            /// <summary>
            /// The order of this chunk with relation to it's siblings.
            /// </summary>
            public int SequenceID { get; set; }

            /// <summary>
            /// Indicates the file to which this chunk belongs.
            /// </summary>
            public ObjectId FileID { get; set; }

            /// <summary>
            /// The binary data in this chunk.
            /// </summary>
            public byte[] Payload { get; set; }
        }

        protected class FileMetadata
        {
            static FileMetadata()
            {
                MongoConfiguration.Initialize(cfg =>
                   cfg.For<FileMetadata>(f =>
                   {
                       f.UseCollectionNamed("files");
                       f.ForProperty(j => j.FileName).UseAlias("filename");
                       f.ForProperty(j => j.ContentType).UseAlias("contentType");
                       f.ForProperty(j => j.Length).UseAlias("length");
                       f.ForProperty(j => j.ChunkSize).UseAlias("chunkSize");
                       f.ForProperty(j => j.UploadDate).UseAlias("uploadDate");
                       f.ForProperty(j => j.Aliases).UseAlias("aliases");
                       f.ForProperty(j => j.MetaData).UseAlias("metadata");
                       f.ForProperty(j => j.MD5Checksum).UseAlias("md5");
                   }));
            }

            public ObjectId ID { get; set; }
            public String FileName { get; set; }
            public String ContentType { get; set; }
            public long Length { get; set; }
            public int ChunkSize { get; set; }
            public DateTime UploadDate { get; set; }
            public List<String> Aliases { get; set; }
            public Object MetaData { get; set; }
            public String MD5Checksum { get; set; }
        }
    }
}
