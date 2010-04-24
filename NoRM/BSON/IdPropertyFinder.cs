using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Norm.Configuration;

namespace Norm.BSON
{
    ///<summary>
    /// Determines the best property to be used as the identifier property.
    ///</summary>
    public class IdPropertyFinder
    {
        private const string MapDefined = "M";
        private const string AttributeDefined = "A";
        private const string MongoDefault = "MD";
        private const string Conventional = "C";
        private readonly Dictionary<string, PropertyInfo> _idDictionary;
        private readonly Type _type;
        private PropertyInfo[] _properties;

        ///<summary>
        /// Initializes new IdPropertyFinder.
        ///</summary>
        ///<param name="type">The type for which an id property needs to be identified.</param>
        public IdPropertyFinder(Type type)
        {
            _type = type;
            _idDictionary = new Dictionary<string, PropertyInfo>(4)
                                {
                                    { MongoDefault, null },
                                    { MapDefined, null },
                                    { AttributeDefined, null },
                                    { Conventional, null }
                                };
        }

        ///<summary>
        /// Initializes new IdPropertyFinder.
        /// Use this constructor to limit the properties you want to test.
        ///</summary>
        ///<param name="type">The type for which an id property needs to be identified.</param>
        ///<param name="properties">The candidate properties fo the type.</param>
        public IdPropertyFinder(Type type, PropertyInfo[] properties)
            : this(type)
        {
            _properties = properties;
        }

        ///<summary>
        /// Returns the property determined to be the Id with the following priority.
        /// Property named _id
        /// Explicitly mapped Id property
        /// Attribute defined Id property
        /// Property named Id
        /// Conflicts result in MongoConfigurationMapException.
        ///</summary>
        public PropertyInfo IdProperty
        {
            get
            {
                AddCandidates();
                CheckForConflictingCandidates();
                return _idDictionary.Values.FirstOrDefault(value => value != null);
            }
        }

        /// <summary>
        /// Determines if the Id has been explicitly defined in a MongoConfigurationMap <see cref="MongoConfigurationMap"/>.
        /// </summary>
        /// <param name="propertyName">The property name.</param>
        private bool PropertyIsExplicitlyMappedToId(string propertyName)
        {
            var map = MongoTypeConfiguration.PropertyMaps;
            if (map.ContainsKey(_type))
            {
                if (map[_type].ContainsKey(propertyName))
                {
                    return map[_type][propertyName].IsId;
                }
            }
            return false;
        }

        private void CheckForConflictingCandidates()
        {
            //This could be written as one line (MongoDefault && (MapDefined || AttributeDefined)) but two lines is more clearer.
            //This has the potential to become cumbersome should we discover more conflicts.
            if (_idDictionary[MongoDefault] != null)
            {
                if (_idDictionary[MapDefined] != null || _idDictionary[AttributeDefined] != null)
                {
                    throw new MongoConfigurationMapException(_type.Name + " exposes a property called _id and defines a an Id using MongoIndentifier or by explicit mapping.");
                }
            }
        }

        private void AddCandidate(PropertyInfo property)
        {
            if (PropertyIsExplicitlyMappedToId(property.Name))
            {
                _idDictionary[MapDefined] = property;
            }
            else if (property.GetCustomAttributes(BsonHelper.MongoIdentifierAttribute, true).Length > 0)
            {
                _idDictionary[AttributeDefined] = property;
            }
            else if (property.Name.Equals("_id", StringComparison.InvariantCultureIgnoreCase))
            {
                _idDictionary[MongoDefault] = property;
            }
            else if (property.Name.Equals("Id", StringComparison.InvariantCultureIgnoreCase))
            {
                _idDictionary[Conventional] = property;
            }
        }

        private void AddCandidates()
        {
            if(_properties == null)
            {
                _properties = TypeHelper.GetProperties(_type);
            }

            foreach (var property in _properties)
            {
                AddCandidate(property);
            }
        }
    }
}