using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Norm;
using Norm.Tests;
using Norm.BSON;

namespace NoRM.Tests.CollectionFindTests
{
    public class DocSizeTests
    {
        public const int FOUR_MEGS = 4 * 1024 * 1024;

        [Fact]
        public void Attempting_To_Insert_Document_Over_4MB_Throws_Exception()
        {
            using (var mongo = Mongo.Create(TestHelper.ConnectionString()))
            {
                Assert.Throws<DocumentExceedsSizeLimitsException<DocProto>>(() => mongo.GetCollection<DocProto>("toobig")
                    .Insert(new DocProto { Arr = new byte[FOUR_MEGS] }));
            }
        }

        [Fact]
        public void Attempting_To_Update_Document_Over_4MB_Throws_Exception()
        {
            using (var mongo = Mongo.Create(TestHelper.ConnectionString()))
            {
                Assert.Throws<DocumentExceedsSizeLimitsException<DocProto>>(() => mongo.GetCollection<DocProto>("toobig")
                    .Update(new DocProto { Arr = new byte[FOUR_MEGS] }, new { _id = Guid.Empty }, false, false));
            }
        }

        [Fact]
        public void Attempting_To_Update_Value_Document_Over_4MB_Throws_Exception()
        {
            using (var mongo = Mongo.Create(TestHelper.ConnectionString()))
            {
                Assert.Throws<DocumentExceedsSizeLimitsException<DocProto>>(() => mongo.GetCollection<DocProto>("toobig")
                    .Update(new DocProto{ _id = Guid.Empty }, new DocProto{ Arr = new byte[FOUR_MEGS] }, false, false));
            }
        }


        protected class DocProto
        {
            public Guid _id { get; set; }
            public byte[] Arr { get; set; }
        }
    }
}
