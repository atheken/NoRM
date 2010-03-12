using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using NoRM.Attributes;
using NoRM.Linq;
using NoRM.Responses;
using NoRM.BSON.DbTypes;

namespace NoRM.Tests
{

    internal class ReduceProduct
    {
        public ObjectId Id { get; set; }
        public float Price { get; set; }

        public ReduceProduct()
        {
            Id = ObjectId.NewObjectId();
        }
    }

    public class ProductSum
    {
        public int Id { get; set; }
        public int Value { get; set; }
    }
    public class ProductSumObjectId
    {
        public ObjectId Id { get; set; }
        public int Value { get; set; }
    }

    public class TestClass
    {
        public TestClass()
        {
            Id = Guid.NewGuid();
        }

        [MongoIdentifier]
        public Guid? Id { get; set; }

        public double? ADouble { get; set; }
        public string AString { get; set; }
        public int? AInteger { get; set; }
        public List<String> AStringArray { get; set; }
    }

    internal class TestHelper
    {
        private static readonly string _connectionStringHost = ConfigurationManager.AppSettings["connectionStringHost"];

        public static string ConnectionString()
        {
            return ConnectionString(null);
        }

        public static string ConnectionString(string query)
        {
            return ConnectionString(query, null, null, null);
        }

        public static string ConnectionString(string userName, string password)
        {
            return ConnectionString(null, null, userName, password);
        }

        public static string ConnectionString(string query, string userName, string password)
        {
            return ConnectionString(query, null, userName, password);
        }

        public static string ConnectionString(string query, string database, string userName, string password)
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
            database = database ?? "NoRMTests";
            return string.Format("mongodb://{0}{1}/{2}{3}", authentication, host, database, query);
        }
    }


    internal class Session : IDisposable
    {
        private readonly MongoQueryProvider _provider;

        public Session()
        {
            _provider = new MongoQueryProvider("NoRMTests");
        }

        public MongoQueryProvider Provider
        {
            get { return _provider; }
        }

        public IQueryable<Product> Products
        {
            get { return new MongoQuery<Product>(_provider); }
        }

        #region IDisposable Members

        public void Dispose()
        {
            _provider.Server.Dispose();
        }

        #endregion

        public T MapReduce<T>(string map, string reduce)
        {
            T result = default(T);
            using (MapReduce mr = _provider.Server.CreateMapReduce())
            {
                MapReduceResponse response = mr.Execute(new MapReduceOptions(typeof(T).Name) { Map = map, Reduce = reduce });
                MongoCollection<MapReduceResult<T>> coll = response.GetCollection<MapReduceResult<T>>();
                MapReduceResult<T> r = coll.Find().FirstOrDefault();
                result = r.Value;
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
            _provider.DB.DropCollection(typeof(T).Name);
        }

        public void CreateCappedCollection(string name)
        {
            _provider.DB.CreateCollection(new CreateCollectionOptions(name));
        }
    }

    internal class Post
    {
        public Post()
        {
            Id = ObjectId.NewObjectId();
            Comments = new List<Comment>();
            Tags = new List<string>();
        }
        public ObjectId Id { get; set; }
        public string Title { get; set; }
        public int Score { get; set; }
        public IList<Comment> Comments { get; set; }
        public IList<string> Tags { get; set; }
    }

    internal class Comment
    {
        public string Text { get; set; }
    }

    internal class CheeseClubContact
    {
        public ObjectId Id { get; set; }
        public string Name { get; set; }
        public string FavoriteCheese { get; set; }

        public CheeseClubContact()
        {
            Id = ObjectId.NewObjectId();
        }
    }

    internal class ProductReference
    {
        public ProductReference()
        {
            Id = ObjectId.NewObjectId();
        }

        public ObjectId Id { get; set; }
        public string Name { get; set; }
        public DBReference[] ProductsOrdered { get; set; }
    }

    internal class Person
    {
        public ObjectId Id { get; set; }
        public string Name { get; set; }
        public Address Address { get; set; }
        public DateTime LastContact { get; set; }

        public Person()
        {
            Id = ObjectId.NewObjectId();
            Address = new Address();
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

    internal class InventoryChange
    {
        public int AmountChanged { get; set; }
        public DateTime CreatedOn { get; set; }
        public InventoryChange()
        {
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

    public class PrivateSetter
    {
        public int Id { get; private set; }

        public PrivateSetter() { }
        public PrivateSetter(int id)
        {
            Id = id;
        }
    }
    public class ReadOnlyList
    {
        private IList<string> _names;
        public IList<string> Names
        {
            get
            {
                if (_names == null)
                {
                    _names = new List<string>();
                }
                return _names;
            }
        }
    }
    public class DictionaryObject
    {
        private IDictionary<string, int> _lookup;
        public IDictionary<string, int> Names
        {
            get
            {
                if (_lookup == null)
                {
                    _lookup = new Dictionary<string, int>();
                }
                return _lookup;
            }
            set { _lookup = value; }
        }
    }
    public class ReadOnlyDictionary
    {
        private IDictionary<string, int> _lookup;
        public IDictionary<string, int> Names
        {
            get
            {
                if (_lookup == null)
                {
                    _lookup = new Dictionary<string, int>();
                }
                return _lookup;
            }
        }
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
        public List<string> AList { get; set; }
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