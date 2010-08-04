using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using Norm.Attributes;
using Norm.Configuration;
using Norm.Linq;
using Norm.Responses;
using Norm.BSON.DbTypes;
using Norm.Collections;
using System.ComponentModel;
using Norm.BSON;
using System.Collections;
using System.Globalization;

namespace Norm.Tests
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

    public class GenericSuperClass<T>
    {

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
            database = database ?? "NormTests";
            return string.Format("mongodb://{0}{1}/{2}{3}", authentication, host, database, query);
        }
    }


    internal class Session : IDisposable
    {
        
        private readonly IMongo _provider;

        public Session()
        {
            _provider = Mongo.Create("mongodb://127.0.0.1/NormTests?strict=false");
        }

        public IMongoDatabase DB { get { return this._provider.Database; } }

        public IQueryable<TestProduct> Products
        {
            get { return _provider.GetCollection<TestProduct>().AsQueryable(); }
        }
        public IQueryable<Thread> Threads
        {
            get { return _provider.GetCollection<Thread>().AsQueryable(); }
        }
        public IQueryable<Post> Posts
        {
            get { return _provider.GetCollection<Post>().AsQueryable(); }
        }

        #region IDisposable Members

        public void Dispose()
        {
            _provider.Dispose();
        }

        #endregion

        public T MapReduce<T>(string map, string reduce)
        {
            T result = default(T);
            MapReduce mr = _provider.Database.CreateMapReduce();

            MapReduceResponse response =
                mr.Execute(new MapReduceOptions(MongoConfiguration.GetCollectionName(typeof(T)))
                               {
                                   Map = map,
                                   Reduce = reduce
                               });
            IMongoCollection<MapReduceResult<T>> coll = response.GetCollection<MapReduceResult<T>>();
            MapReduceResult<T> r = coll.Find().FirstOrDefault();
            result = r.Value;

            return result;
        }

        public void Add<T>(T item) where T : class, new()
        {
            _provider.Database.GetCollection<T>().Insert(item);
        }

        public void Update<T>(T item) where T : class, new()
        {
            _provider.Database.GetCollection<T>().Save(item);
        }

        public void Drop<T>()
        {
            _provider.Database.DropCollection(MongoConfiguration.GetCollectionName(typeof(T)));
        }

        public void CreateCappedCollection(string name)
        {
            _provider.Database.CreateCollection(new CreateCollectionOptions(name));
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


    internal class Post2
    {
        public Post2()
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
        public string Name { get; set; }
        public bool IsOld { get; set; }
        public IList<Tag> CommentTags { get; set; }
        public IList<string> CommentTagsSimple { get; set; }
    }

    internal class Tag
    {
        public string TagName { get; set; }
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

    internal class CheeseClubContactWithNullableIntId
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public string FavoriteCheese { get; set; }
    }

    internal class ProductReference
    {
        public ProductReference()
        {
            Id = ObjectId.NewObjectId();
        }

        public ObjectId Id { get; set; }
        public string Name { get; set; }
        public DbReference<TestProduct>[] ProductsOrdered { get; set; }
    }

    internal class User3
    {
        public string Id { get; set; }
        public string EmailAddress { get; set; }
    }

    internal class Role
    {
        public string Id { get; set; }
        public List<DbReference<User3, string>> Users { get; set; }
    }

    internal class Person
    {
        public ObjectId Id { get; set; }
        public string Name { get; set; }
        public Address Address { get; set; }
        public DateTime LastContact { get; set; }
        public List<String> Relatives { get; set; }
        public DateTime Birthday { get; set; }
        public int Age { get; set; }
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

    internal class ExpandoAddress : IExpando
    {
        public string Street { get; set; }
        public string City { get; set; }
        private Dictionary<String, object> _properties = new Dictionary<string, object>(0);

        /// <summary>
        /// Additional, non-static properties of this message.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ExpandoProperty> AllProperties()
        {
            return this._properties.Select(j => new ExpandoProperty(j.Key, j.Value));
        }

        public void Delete(string propertyName)
        {
            this._properties.Remove(propertyName);
        }

        public object this[string propertyName]
        {
            get
            {
                return this._properties[propertyName];
            }
            set
            {
                this._properties[propertyName] = value;
            }
        }

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
        public int RefNum { get; set; }
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

    internal class TestProduct
    {
        public TestProduct()
        {
            Supplier = new Supplier();
            _id = ObjectId.NewObjectId();
            Inventory = new List<InventoryChange>();
            this.UniqueID = Guid.NewGuid();
        }
        public List<InventoryChange> Inventory { get; set; }
        public ObjectId _id { get; set; }
        public Guid UniqueID { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public Supplier Supplier { get; set; }
        public DateTime Available { get; set; }
        public int Quantity { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsStillAvailable { get; set; }
    }
    
    internal class TestProductSummary
    {
        public ObjectId _id { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
    }

    internal class TestIntGeneration
    {
        public int? _id { get; set; }
        public string Name { get; set; }
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
    public class HashSetList
    {
        private HashSet<string> _names;
        public ICollection<string> Names
        {
            get
            {
                if (_names == null)
                {
                    _names = new HashSet<string>();
                }
                return _names;
            }
        }
    }
    public class DictionaryObject
    {
        private Dictionary<string, int> _lookup;
        public Dictionary<string, int> Names
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
    public class IDictionaryObject
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

    public class SerializerTest
    {
        public int Id { get; set; }

        [DefaultValue("Test")]
        public string Message { get; set; }

        [DefaultValue(typeof(DateTime), "00:00:00.0000000, January 1, 0001")]
        public DateTime MagicDate { get; set; }
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
        internal IEnumerable<Person> AnIEnumerable { get; set; }

        [MongoIgnore]
        public int IgnoredProperty { get; set; }
    }

    public class ChildGeneralDTO : GeneralDTO
    {
        public bool IsOver9000 { get; set; }
    }

    public class CultureInfoDTO
    {
        public CultureInfo Culture { get; set; }
    }

    public class NonSerializableClass
    {
        public NonSerializableValueObject Value { get; set; }
        public string Text { get; set; }
    }

    public class NonSerializableValueObject
    {
        // Stuff a few properties in here that Norm normally cannot handle
        private ArgumentException ex { get; set; }
        private NonSerializableValueObject MakeNormCrashReference { get; set; }

        public string Number { get; private set; }

        public NonSerializableValueObject(string number)
        {
            Number = number;
            MakeNormCrashReference = this;
        }
    }

    public class NonSerializableValueObjectTypeConverter : IBsonTypeConverter
    {
        #region IBsonTypeConverter Members

        public Type SerializedType
        {
            get { return typeof(string); }
        }

        public object ConvertToBson(object data)
        {
            return ((NonSerializableValueObject)data).Number;
        }

        public object ConvertFromBson(object data)
        {
            return new NonSerializableValueObject((string)data);
        }

        #endregion
    }


    [MongoDiscriminated]
    public class SuperClassObject
    {
        public SuperClassObject()
        {
            Id = Guid.NewGuid();
        }

        [MongoIdentifier]
        public Guid Id { get; protected set; }
        public string Title { get; set; }
    }

    public class SubClassedObject : SuperClassObject
    {
        public bool ABool { get; set; }
    }

    [MongoDiscriminated]
    public class SuperClassObjectFluentMapped
    {
        public SuperClassObjectFluentMapped()
        {
            Id = ObjectId.NewObjectId();
        }

        [MongoIdentifier]
        public ObjectId Id { get; protected set; }
        public string Title { get; set; }

        static SuperClassObjectFluentMapped()
        {
            MongoConfiguration.Initialize(config =>
                config.For<SuperClassObjectFluentMapped>(c =>
                {
                    c.ForProperty(u => u.Id).UseAlias("_id");
                    c.ForProperty(u => u.Title).UseAlias("t");
                }));
        }
    }

    public class SubClassedObjectFluentMapped : SuperClassObjectFluentMapped
    {
        public bool ABool { get; set; }

        static SubClassedObjectFluentMapped()
        {
            MongoConfiguration.Initialize(config =>
                config.For<SubClassedObjectFluentMapped>(c =>
                {
                    c.ForProperty(u => u.ABool).UseAlias("b");
                }));
        }
    }

    [MongoDiscriminated]
    internal interface IDiscriminated
    {
        [MongoIdentifier]
        Guid Id { get; }
    }

    internal class InterfaceDiscriminatedClass : IDiscriminated
    {
        public InterfaceDiscriminatedClass()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; protected set; }
    }

    internal class InterfacePropertyContainingClass
    {
        public InterfacePropertyContainingClass()
        {
            Id = Guid.NewGuid();
            InterfaceProperty = new NotDiscriminatedClass();
        }

        [MongoIdentifier]
        public Guid Id { get; set; }
        public INotDiscriminated InterfaceProperty { get; set; }
    }

    internal interface INotDiscriminated
    {
        string Something { get; set; }
    }

    internal class NotDiscriminatedClass : INotDiscriminated
    {
        public string Something { get; set; }
    }

    public class DiscriminationMap : MongoConfigurationMap
    {
        public DiscriminationMap()
        {
            For<INotDiscriminated>(config => config.UseAsDiscriminator());
        }
    }

    internal interface IDTOWithNonDefaultId
    {
        [MongoIdentifier]
        Guid MyId { get; }
    }

    internal class DtoWithNonDefaultIdClass : IDTOWithNonDefaultId
    {
        public DtoWithNonDefaultIdClass()
        {
            MyId = Guid.NewGuid();
        }

        public Guid MyId { get; protected set; }
    }

    public class PrivateConstructor
    {
        public string Name { get; set; }
        private PrivateConstructor() { }

        public static PrivateConstructor Create(string name)
        {
            return new PrivateConstructor { Name = name };
        }
    }

    public class Forum
    {
        public ObjectId Id { get; set; }
    }

    public class Thread
    {
        public ObjectId ForumId { get; set; }
    }

    internal class Shoppers : IQueryable<Shopper>, IDisposable
    {
        private readonly IMongo _provider;

        public Shoppers(IMongo conn)
        {
            _provider = conn;
            this._queryable = conn.GetCollection<Shopper>().AsQueryable();
        }

        public T MapReduce<T>(string map, string reduce)
        {
            var result = default(T);
            var mr = _provider.Database.CreateMapReduce();

            var response = mr.Execute(new MapReduceOptions(typeof(T).Name) { Map = map, Reduce = reduce });
            var coll = response.GetCollection<MapReduceResult<T>>();
            var r = coll.Find().FirstOrDefault();
            result = r.Value;

            return result;
        }

        public void Add<T>(T item) where T : class, new()
        {
            _provider.Database.GetCollection<T>().Insert(item);
        }

        public void Update<T>(T item) where T : class, new()
        {
            _provider.Database.GetCollection<T>().UpdateOne(item, item);
        }

        public void Drop<T>()
        {
            try
            {
                _provider.Database.DropCollection(MongoConfiguration.GetCollectionName(typeof (T)));
            }
            catch (MongoException)
            {
            }
        }

        public void Dispose()
        {
            _provider.Dispose();
        }

        private IQueryable<Shopper> _queryable;

        public IEnumerator<Shopper> GetEnumerator()
        {
            return this._queryable.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public Type ElementType
        {
            get { return this._queryable.ElementType; }
        }

        public System.Linq.Expressions.Expression Expression
        {
            get { return this._queryable.Expression; }
        }

        public IQueryProvider Provider
        {
            get { return this._queryable.Provider; }
        }

    }

    internal class Shopper
    {
        public Shopper()
        {
            Id = ObjectId.NewObjectId();
        }

        public ObjectId Id { get; set; }
        public string Name { get; set; }
        public Cart Cart { get; set; }
    }

    internal class Cart
    {
        public Cart()
        {
            Id = ObjectId.NewObjectId();
        }
        public string Name { get; set; }
        public ObjectId Id { get; set; }
        public TestProduct Product { get; set; }
        public Supplier[] CartSuppliers { get; set; }
    }

    internal class User
    {
        public User()
        {
            Id = ObjectId.NewObjectId();
        }
        public ObjectId Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    internal class User2
    {
        public User2()
        {
            Id = ObjectId.NewObjectId();
        }
        public ObjectId Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class ShopperMap : MongoConfigurationMap
    {
        public ShopperMap()
        {
            For<Shopper>(config =>
            {
                config.UseCollectionNamed("MyProducts");
                config.ForProperty(u => u.Name).UseAlias("shopperName");
                config.ForProperty(u => u.Cart).UseAlias("MyCart");
            });

            For<Cart>(c =>
            {
                c.UseCollectionNamed("ListOfCarts");
                c.ForProperty(cart => cart.Product).UseAlias("ProductsGoHere");
                c.ForProperty(ca => ca.Name).UseAlias("ThisCartName");
            });

            For<TestProduct>(c => c.ForProperty(p => p.Price).UseAlias("DiscountPrice"));
        }
    }

    internal class IdMap0
    {
        public ObjectId _ID { get; set; }
    }

    internal class IdMap1
    {
        [MongoIdentifier]
        public ObjectId TheID { get; set; }
    }

    internal class IdMap2
    {
        public ObjectId ID { get; set; }
    }

    internal class IdMap3
    {
        public ObjectId id { get; set; }
    }

    internal class IdMap4
    {
        public ObjectId Id { get; set; }
    }

    public class OtherMap : MongoConfigurationMap
    {
        public OtherMap()
        {
            For<User>(cfg => cfg.ForProperty(u => u.LastName).UseAlias("last"));
        }
    }

    public class CustomMap : MongoConfigurationMap
    {
        public CustomMap()
        {
            this.For<User>(cfg =>
            {
                cfg.ForProperty(u => u.FirstName).UseAlias("first");
                cfg.ForProperty(u => u.LastName).UseAlias("last");
                cfg.UseCollectionNamed("UserBucket");
                cfg.UseConnectionString(TestHelper.ConnectionString());
            });

            this.For<TestProduct>(cfg => cfg.ForProperty(p => p.Name).UseAlias("productname"));
        }
    }

}