using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NoRM.Attributes;
using NoRM.BSON.DbTypes;
using System.Text.RegularExpressions;
using NoRM.Linq;

namespace NoRM.Tests
{
    internal class Session : MongoSession
    {
        public Session()
            : base("mongodb://localhost/Northwind?pooling=false&strict=false")
        {
        }

        public IQueryable<Product> Products
        {
            get { return new MongoQuery<Product>(Provider); }
        }

        public void Add<T>(T item) where T : class, new()
        {
            Provider.Mongo.GetCollection<T>().Insert(item);
        }

        public void Update<T>(T item) where T : class, new()
        {
            Provider.Mongo.GetCollection<T>().UpdateOne(item, item);
        }

        public void Drop<T>()
        {
            Provider.Mongo.Database.DropCollection(typeof(T).Name);
        }

        public void CreateCappedCollection(string name)
        {
            Provider.Mongo.Database.CreateCollection(new CreateCollectionOptions(name));
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

    internal class Product
    {
        public Product()
        {
            Supplier = new Supplier();
            Id = ObjectId.NewOID();
        }


        public ObjectId Id { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public Supplier Supplier { get; set; }
        public DateTime Available { get; set; }
    }

    public class FakeObject
    {
        public ObjectId Id { get; set; }

        public FakeObject()
        {
            Id = ObjectId.NewOID();
        }
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
        public Flags32? Flags32 { get; set; }
        public Flags64? Flags64 { get; set; }

        [MongoIgnore]
        public int IgnoredProperty { get; set; }
    }
}
