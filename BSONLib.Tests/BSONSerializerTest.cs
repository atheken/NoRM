using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace BSONLib.Tests
{
    [TestFixture]
    public class BSONSerializerTest
    {
        protected class GeneralDTO
        {
            public String Title { get; set; }
        }

        [Test]
        public void Serializint_POCO_Generates_Bytes()
        {
            BSONSerializer serializer = new BSONSerializer();
            GeneralDTO dummy = new GeneralDTO(){Title ="Testing"};
            Assert.IsNotEmpty(serializer.Serialize(dummy));
        }
    }
}
