using System;
using System.Linq;
using Norm;
using Norm.BSON;
using Norm.Linq;
using Norm.Protocol.Messages;
using NUnit.Framework;
using Norm.Configuration;

namespace Norm.Tests
{
    [TestFixture]
    public class MongoOptimizationTests
    {
        [Test]
        public void MongoCollectionEnsuresIndicies()
        {
            using (var session = new Session())
            {
                session.Drop<TestProduct>();

                session.Add(new TestProduct
                {
                    Name = "ExplainProduct",
                    Price = 10,
                    Supplier = new Supplier { Name = "Supplier", CreatedOn = DateTime.Now }
                });

                session.DB.GetCollection<TestProduct>().CreateIndex(p => p.Supplier.Name, "Test", true, IndexOption.Ascending);
            }
        }

        [Test]
        public void MongoQueryExplainsExecutionPlansForFlyweightQueries()
        {
            using (var session = new Session())
            {
                session.Drop<TestProduct>();

                session.DB.GetCollection<TestProduct>().CreateIndex(p => p.Supplier.Name, "TestIndex", true, IndexOption.Ascending);

                session.Add(new TestProduct
                                {
                                    Name = "ExplainProduct",
                                    Price = 10,
                                    Supplier = new Supplier { Name = "Supplier", CreatedOn = DateTime.Now }
                                });

                // To see this manually you can run the following command in Mongo.exe against 
                //the Product collection db.Product.ensureIndex({"Supplier.Name":1})

                // Then you can run this command to see a detailed explain plan
                // db.Product.find({"Supplier.Name":"abc"})

                // The following query is the same as running: db.Product.find({"Supplier.Name":"abc"}).explain()
                var query = new Expando();
                query["Supplier.Name"] = Q.Equals("Supplier");

                var result = session.DB.GetCollection<TestProduct>().Explain(query);

                Assert.AreEqual("BtreeCursor TestIndex", result.Cursor);
            }
        }

        [Test]
        public void MongoQueryExplainsExecutionPlans()
        {
            using (var session = new Session())
            {
                session.Drop<TestProduct>();

                session.DB.GetCollection<TestProduct>().CreateIndex(p => p.Name, "TestIndex", true, IndexOption.Ascending);

                session.Add(new TestProduct
                {
                    Name = "ExplainProduct",
                    Price = 10,
                    Supplier = new Supplier { Name = "Supplier", CreatedOn = DateTime.Now }
                });


                var result = session.DB.GetCollection<TestProduct>().Explain(new { Name = "ExplainProduct" });

                Assert.AreEqual("BtreeCursor TestIndex", result.Cursor);
            }
        }

        [Test]
        public void MongoQueryExplainsLinqExecutionPlans()
        {
            using (var session = new Session())
            {
                session.Drop<TestProduct>();
               
                session.DB.GetCollection<TestProduct>().CreateIndex(p => p.Supplier.Name, "TestIndex", true, IndexOption.Ascending);

                session.Add(new TestProduct
                {
                    Name = "ExplainProduct",
                    Price = 10,
                    Supplier = new Supplier { Name = "Supplier", CreatedOn = DateTime.Now }
                });

                var result = session.Products
                    .Where(x => x.Supplier.Name == "Supplier")
                    .Explain();

                Assert.AreEqual("BtreeCursor TestIndex", result.Cursor);
            }
        }

        [Test]
        public void MongoQuerySupportsHintsForLinqQueries()
        {
            using (var session = new Session())
            {
                MongoConfiguration.RemoveMapFor<TestProduct>();
                session.Drop<TestProduct>();

                session.Add(new TestProduct
                                {
                                    Name = "ExplainProduct",
                                    Price = 10,
                                    Supplier = new Supplier {Name = "Supplier", CreatedOn = DateTime.Now}
                                });

                var query = new Expando();
                query["Supplier.Name"] = Q.Equals("Supplier");

                var result = session.DB
                    .GetCollection<TestProduct>()
                    .Find(query)
                    .Hint(p => p.Name, IndexOption.Ascending);

                Assert.AreEqual(1, result.Count());
            }
        }

        [Test]
        public void MongoQuerySupportschainingHintsForLinqQueries()
        {
            using (var session = new Session())
            {
                MongoConfiguration.RemoveMapFor<TestProduct>();
                
                session.Drop<TestProduct>();

                session.Add(new TestProduct
                {
                    Name = "ExplainProduct",
                    Price = 10,
                    Supplier = new Supplier { Name = "Supplier", CreatedOn = DateTime.Now }
                });

                var query = new Expando();
                query["Supplier.Name"] = Q.Equals("Supplier");

                var result = session.DB.GetCollection<TestProduct>()
                    .Find(query)
                    .Hint(p => p.Name, IndexOption.Ascending)
                    .Hint(p => p.Supplier.Name, IndexOption.Descending);

                Assert.AreEqual(1, result.Count());
            }
        }
    }
}
