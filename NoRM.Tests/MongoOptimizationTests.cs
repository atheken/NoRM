using System;
using System.Linq;
using Norm.BSON;
using Norm.Linq;
using Norm.Protocol.Messages;
using Xunit;

namespace Norm.Tests
{
    public class MongoOptimizationTests
    {
        [Fact]
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

        [Fact]
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

                Assert.Equal("BtreeCursor TestIndex", result.Cursor);
            }
        }

        [Fact]
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

                Assert.Equal("BtreeCursor TestIndex", result.Cursor);
            }
        }

        [Fact]
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

                Assert.Equal("BtreeCursor TestIndex", result.Cursor);
            }
        }

        [Fact]
        public void MongoQuerySupportsHintsForLinqQueries()
        {
            using (var session = new Session())
            {
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

                Assert.Equal(1, result.Count());
            }
        }

        [Fact]
        public void MongoQuerySupportschainingHintsForLinqQueries()
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

                var query = new Expando();
                query["Supplier.Name"] = Q.Equals("Supplier");

                var result = session.DB.GetCollection<TestProduct>()
                    .Find(query)
                    .Hint(p => p.Name, IndexOption.Ascending)
                    .Hint(p => p.Supplier.Name, IndexOption.Descending);

                Assert.Equal(1, result.Count());
            }
        }
    }
}
