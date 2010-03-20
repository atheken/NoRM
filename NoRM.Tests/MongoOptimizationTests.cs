using System;
using Norm.BSON;
using Xunit;

namespace Norm.Tests
{
    public class MongoOptimizationTests
    {
        [Fact]
        public void MongoQueryExplainsExecutionPlansForFlyweightQueries()
        {
            using (var session = new Session())
            {
                session.Add(new Product
                                {
                                    Name = "ExplainProduct",
                                    Price = 10,
                                    Supplier = new Supplier { Name = "Supplier", CreatedOn = DateTime.Now }
                                });


                // NOTE: these unit tests aren't 100% valid since adding an index in code isn't
                // supported yet.  In order to see meaningful results you have to manually
                // add an index for now.

                // Run the following command in Mongo.exe against the Product collection
                // db.Product.ensureIndex({"Supplier.Name":1})

                // Then you can run this command to see a hydrated explain plan
                // db.Product.find({"Supplier.Name":"abc"})

                // The following is the same as runging:   db.Product.find({"Supplier.Name":"abc"}).explain()

                var query = new Flyweight();
                query["Supplier.Name"] = Q.Equals("Supplier");

                var result = session.Provider.DB.GetCollection<Product>().Explain(query);

                Assert.NotNull(result.cursor);
            }
        }

        [Fact]
        public void MongoQueryExplainsExecutionPlans()
        {
            using (var session = new Session())
            {
                session.Add(new Product
                {
                    Name = "ExplainProduct",
                    Price = 10,
                    Supplier = new Supplier { Name = "Supplier", CreatedOn = DateTime.Now }
                });


                var result = session.Provider.DB.GetCollection<Product>().Explain(new { Name = "ExplainProduct" });

                Assert.NotNull(result.cursor);
            }
        }
    }
}
