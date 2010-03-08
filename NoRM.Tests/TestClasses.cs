namespace NoRM.Tests
{
    using System;
    using System.Configuration;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Attributes;
    using Linq;
    using System.Collections.Generic;

    internal class TestHelper
    {
        private static readonly string _connectionStringHost = ConfigurationManager.AppSettings["connectionStringHost"];    
        public static string ConnectionString()
        {
            return ConnectionString(null);            
        }
        public static string ConnectionString(string query)
        {
            return ConnectionString(query, null, null);
        }
        public static string ConnectionString(string userName, string password)
        {
            return ConnectionString(null, userName, password);
        }
        public static string ConnectionString(string query, string userName, string password)
        {
            var authentication = string.Empty;
            if (userName != null)
            {
                authentication = string.Concat(userName, ':', password, '@');
            }
            if (!string.IsNullOrEmpty(query) && !query.StartsWith("?"))
            {
                query = string.Concat('?', query);
            }
            var host = string.IsNullOrEmpty(_connectionStringHost) ? "localhost" : _connectionStringHost;

            return string.Format("mongodb://{0}{1}/NoRMTests{2}", authentication, host, query);
        }
    }
    

    internal class Session:IDisposable
    {
        
        MongoQueryProvider _provider;
        public Session()
        {
            _provider=new MongoQueryProvider("test");
        }
        public MongoQueryProvider Provider {
            get {
                return _provider;
            }
        }
        public IQueryable<Product> Products
        {
            get { return new MongoQuery<Product>(_provider); }
        }

        public T MapReduce<T>(string map, string reduce) {
            T result = default(T);
            using (var mr = _provider.Server.CreateMapReduce()) {
                var response = mr.Execute(new MapReduceOptions(typeof(T).Name) { Map = map, Reduce = reduce });
                var coll = response.GetCollection<MapReduceResult>();
                var r = coll.Find().FirstOrDefault();
                result = (T)r.Value;
            }
            return result;
        }

        public void Add<T>(T item) where T : class, new()
        {
            _provider.DB.GetCollection<T>().Insert(item);
        }

        public void Update<T>(T item) where T : class, new()
        {
            _provider.DB.GetCollection<T>().UpdateOne(item, item);
        }

        public void Drop<T>()
        {
            _provider.DB.DropCollection(typeof (T).Name);
        }

        public void CreateCappedCollection(string name)
        {
            _provider.DB.CreateCollection(new CreateCollectionOptions(name));
        }


        public void Dispose() {
            _provider.Server.Dispose();
        }

    }

    internal class Address
    {
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
    }

    internal class Supplier
    {
        public Supplier()
        {
            Address = new Address();
            CreatedOn = DateTime.Now;
        }

        public string Name { get; set; }
        public DateTime CreatedOn { get; set; }
        public Address Address { get; set; }
    }

    internal class InventoryChange {
        public int AmountChanged { get; set; }
        public DateTime CreatedOn { get; set; }
        public InventoryChange() {
            CreatedOn = DateTime.Now;
        }
    }

    internal class Product
    {
        public Product()
        {
            Supplier = new Supplier();
            Id = ObjectId.NewObjectId();
            Inventory = new List<InventoryChange>();
        }
        public List<InventoryChange> Inventory { get; set; }
        public ObjectId Id { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public Supplier Supplier { get; set; }
        public DateTime Available { get; set; }
    }

    public class FakeObject
    {
        public FakeObject()
        {
            Id = ObjectId.NewObjectId();
        }

        public ObjectId Id { get; set; }
    }

    public enum Flags32
    {
        FlagNone = 0,
        FlagOn = 1,
        FlagOff = 2
    }

    public enum Flags64 : long
    {
        FlagNone = 0,
        FlagOn = 1,
        FlagOff = 2
    }

    public class MiniObject
    {
        public ObjectId _id { get; set; }
    }

    public class GeneralDTO
    {
        public double? Pi { get; set; }
        public int? AnInt { get; set; }
        public String Title { get; set; }
        public bool? ABoolean { get; set; }
        public byte[] Bytes { get; set; }
        public String[] Strings { get; set; }
        public Guid? AGuid { get; set; }
        public Regex ARex { get; set; }
        public DateTime? ADateTime { get; set; }
        public GeneralDTO Nester { get; set; }
        public ScopedCode Code { get; set; }
        public float? AFloat { get; set; }
        public Flags32? Flags32 { get; set; }
        public Flags64? Flags64 { get; set; }

        [MongoIgnore]
        public int IgnoredProperty { get; set; }
    }


    public class ChildGeneralDTO : GeneralDTO
    {
        public bool IsOver9000 { get; set; }
    }
}