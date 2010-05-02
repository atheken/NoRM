using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using System.IO.Compression;
using System.Resources;
using System.Reflection;
using System.IO;
using System.Security.Cryptography;
using Norm;
using Norm.Tests;
using Norm.BSON.DbTypes;

namespace NoRM.Tests.DBTypeTests
{
    public class GridFSTests
    {
        private MemoryStream _randomBytes = new MemoryStream(10 * 1024 * 1024);
        private MD5 _hasher = MD5.Create();
        private byte[] _randomByteHash;
        private Mongo _db;

        public GridFSTests()
        {
            //construct a random 10MB stream.
            Random r = new Random(DateTime.Now.Millisecond);
            for (int i = 0; i < 1024 * 1024 * 1.5; i++)
            {
                this._randomBytes.Write(BitConverter.GetBytes(r.NextDouble()), 0, 8);
            }
            this._randomBytes.Position = 0;
            this._randomByteHash = _hasher.ComputeHash(_randomBytes);
            this._randomBytes.Position = 0;
            this._db = Mongo.Create(TestHelper.ConnectionString());
            using (var admin = new MongoAdmin(TestHelper.ConnectionString()))
            {
                admin.DropDatabase();
            }
        }

        [Fact]
        public void StorageOfFileIsNotLossy()
        {
            //var fcoll = this._db.GetCollection<Object>("aCollection").GetChildCollection<GridFile>("files");
            //GridFile gf = new GridFile();
            //gf.uploadDate = DateTime.Now;
            //gf.length = this._randomBytes.Length;
            //gf.md5 = this._randomByteHash.Aggregate("", (seed, current) => seed += String.Format("{0:x2}", current));
            //gf.contentType = "application/x-octet-stream";
            //gf.filename = "Random.bin";
            //gf._id = Guid.NewGuid();
            //gf.WriteToServer(this._randomBytes, true);
        }

    }
}
