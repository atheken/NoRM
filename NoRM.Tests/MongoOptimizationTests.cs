using System.Linq;
using NoRM.Linq;
using Xunit;

namespace NoRM.Tests
{
    public class MongoOptimizationTests
    {
        [Fact]
        public void MongoQueryExplainsExecutionPlans()
        {
            using(var session = new Session())
            {
                session.Add(new Product
                                {
                                    Name = "ExplainProduct",
                                    Price = 10, 
                                    Supplier = new Supplier { Name = "Supplier" }
                                });

                // CreateIndex doesn't work yet.  You'll have to add it manually if you don't want empty startKey and endKey values
                //session.Provider.DB.GetCollection<Product>().CreateIndex(new Product(), false, "productindex");

                var explainPlan = session.Products.Where(x => x.Supplier.Name == "test").Explain();
            }
        }
    }
}
