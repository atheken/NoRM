using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Norm.Tests;
using Norm;
using Norm.GridFS;
using System.IO;

namespace Norm.Tests.GridFS
{
    [TestFixture]
    public class GridFileCollectionTests
    {
		private Mongod _proc;

		[TestFixtureSetUp]
		public void SetUp ()
		{
			_proc = new Mongod ();
		}

		[TestFixtureTearDown]
		public void TearDown ()
		{
			_proc.Dispose ();
		}

        [SetUp]
        public void Setup()
        {
            using (var conn = Mongo.Create(TestHelper.ConnectionString("strict=false")))
            {
                var files = conn.Database.Files();
                files.Delete(null);												
            }		
        }

        [Test]
        public void Extension_Methods_Provide_Access_To_Collections()
        {
            using (var conn = Mongo.Create(TestHelper.ConnectionString("strict=false")))
            {
                var fileColl = conn.Database.Files();
                Assert.NotNull(fileColl);

                var fileColl2 = conn.GetCollection<TestClass>().Files();
            }
        }

        [Test]
        public void File_Save_Is_Not_Lossy()
        {
            using (var conn = Mongo.Create(TestHelper.ConnectionString()))
            {
                var ms = new MemoryStream(8000000);//about 8MB.
                for (int i = 0; i < 2000000; i++)
                {
                    ms.Write(BitConverter.GetBytes(i), 0, 4);
                }

                var gridFS = conn.Database.Files();
                var file = new GridFile();
                file.ContentType = "application/unknown";
                file.FileName = "Random_File_Test" + Guid.NewGuid().ToString();
                file.Aliases = new String[] { "Alpha", "Bravo", "Delta", "Echo" };
                file.Content = ms.ToArray();
                gridFS.Save(file);

                var file2 = gridFS.FindOne(new { _id = file.Id });


                Assert.AreEqual(file.Id, file2.Id);
                Assert.AreEqual(file.MD5Checksum, file2.MD5Checksum);
                Assert.AreEqual(file.ContentType, file2.ContentType);
                //Mongo stores dates as long, therefore, we have to use double->long rounding.
                Assert.AreEqual((long)((file.UploadDate - DateTime.MinValue)).TotalMilliseconds,
                    (long)(file2.UploadDate - DateTime.MinValue).TotalMilliseconds);
                Assert.True(file.Aliases.SequenceEqual(file2.Aliases));
                Assert.True(file.Content.SequenceEqual(file2.Content));
            }
        }

        [Test]
        public void File_Delete_Works()
        {
            using (var conn = Mongo.Create(TestHelper.ConnectionString()))
            {
                var ms = new MemoryStream(50000);
                for (int i = 0; i < 2000; i++)
                {
                    ms.Write(BitConverter.GetBytes(i), 0, 4);
                }

                var gridFS = conn.Database.Files();
                var file = new GridFile();
                file.ContentType = "application/unknown";
                file.FileName = "Random_File_Test" + Guid.NewGuid().ToString();
                file.Aliases = new String[] { "Alpha", "Bravo", "Delta", "Echo" };
                file.Content = ms.ToArray();
                gridFS.Save(file);
                var file2 = gridFS.FindOne(new { _id = file.Id });
                Assert.NotNull(file2);
                gridFS.Delete(file2.Id);
                file2 = gridFS.FindOne(new { _id = file.Id });
                Assert.Null(file2);
            }
        }

		[Fact]
		public void File_Save_Should_Replace_Old_Content()
		{
			using (var conn = Mongo.Create(TestHelper.ConnectionString()))
			{
				var gridFS = conn.Database.Files();
				var file = new GridFile
				{
					ContentType = "application/unknown",
					FileName = "test.raw",
					Content = new byte[] { 1, 2, 3 }
				};
				gridFS.Save(file);
				file.Content = new byte[] { 3, 2, 1 };
				gridFS.Save(file);

				Assert.Equal(new byte[] { 3, 2, 1 }, gridFS.FindOne(new { _id = file.Id }).Content.ToArray());
			}
		}

		[Fact]
		public void Delete_Should_Remove_FileChunks()
		{
			using (var conn = Mongo.Create(TestHelper.ConnectionString()))
			{
				var ms = new MemoryStream(50000);
				for (int i = 0; i < 2000; i++)
				{
					ms.Write(BitConverter.GetBytes(i), 0, 4);
				}

				var gridFS = conn.Database.Files();
				var file = new GridFile();
				file.ContentType = "application/unknown";
				file.FileName = "test.raw";
				file.Content = ms.ToArray();
				gridFS.Save(file);

				gridFS.Delete(file.Id);

				Assert.Equal(0, conn.Database.GetCollection<FileChunk>("chunks").GetCollectionStatistics().Count);
			}
		}
    }
}
