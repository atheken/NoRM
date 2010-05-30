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
        private readonly Dictionary<IdType, PropertyInfo> _idDictionary;
        private readonly Type _type;
        private PropertyInfo[] _properties;
        private PropertyInfo[] _interfaceProperties;

        ///<summary>
        /// Initializes new IdPropertyFinder.
        ///</summary>
        ///<param retval="type">The type for which an id property needs to be identified.</param>
        public IdPropertyFinder(Type type)
        {
            _type = type;
            _idDictionary = new Dictionary<IdType, PropertyInfo>(4)
                                {
                                    { IdType.MongoDefault, null },
                                    { IdType.MapDefined, null },
                                    { IdType.AttributeDefined, null },
                                    { IdType.Conventional, null }
                                };
        }

        ///<summary>
        /// Initializes new IdPropertyFinder.
        /// Use this constructor to limit the properties you want to test.
        ///</summary>
        ///<param retval="type">The type for which an id property needs to be identified.</param>
        ///<param retval="properties">The candidate properties fo the type.</param>
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
        /// <param retval="idPropertyCandidate">The property retval.</param>
        private bool PropertyIsExplicitlyMappedToId(string idPropertyCandidate)
        {
            var map = MongoTypeConfiguration.PropertyMaps;
            if (map.ContainsKey(_type))
            {
                if (map[_type].ContainsKey(idPropertyCandidate))
                {
                    return map[_type][idPropertyCandidate].IsId;
                }
            }
            return false;
        }

        private void CheckForConflictingCandidates()
        {
            //This could be written as one line (MongoDefault && (MapDefined || AttributeDefined)) but two lines is more clearer.
            //This has the potential to become cumbersome should we discover more conflicts.
            if (_idDictionary[IdType.MongoDefault] != null)
            {
                if (_idDictionary[IdType.MapDefined] != null || _idDictionary[IdType.AttributeDefined] != null)
                {
                    throw new MongoConfigurationMapException(_type.Name + " exposes a property called _id and defines a an Id using MongoIndentifier or by explicit mapping.");
                }
            }
        }

        private static bool HasMongoIdentifierAttribute(ICustomAttributeProvider idPropertyCandidate)
        {
            return idPropertyCandidate.GetCustomAttributes(BsonHelper.MongoIdentifierAttribute, true).Length > 0;
        }

        private bool PropertyIsAttributeDefinedId(MemberInfo idPropertyCandidate)
        {
            if (HasMongoIdentifierAttribute(idPropertyCandidate))
            {
                return true;
            }

            if (_interfaceProperties != null)
            {
                var interfacePropertiesWithSameNameAsCandidate = _interfaceProperties.Where(propertyInfo => propertyInfo.Name == idPropertyCandidate.Name);
                foreach (PropertyInfo nextProperty in interfacePropertiesWithSameNameAsCandidate)
                {
                    if (HasMongoIdentifierAttribute(nextProperty))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void AddCandidate(PropertyInfo property)
        {
            if (PropertyIsExplicitlyMappedToId(property.Name))
            {
                _idDictionary[IdType.MapDefined] = property;
            }
            else if (PropertyIsAttributeDefinedId(property))
            {
                _idDictionary[IdType.AttributeDefined] = property;
            }
            else if (property.Name.Equals("_id", StringComparison.InvariantCultureIgnoreCase))
            {
                _idDictionary[IdType.MongoDefault] = property;
            }
            else if (property.Name.Equals("Id", StringComparison.InvariantCultureIgnoreCase))
            {
                _idDictionary[IdType.Conventional] = property;
            }
        }

        private void AddCandidates()
        {
            if(_properties == null)
            {
                _properties = ReflectionHelper.GetProperties(_type);
            }

            _interfaceProperties = ReflectionHelper.GetInterfaceProperties(_type);

            foreach (var property in _properties)
            {
                AddCandidate(property);
            }
        }

        private enum IdType
        {
            MapDefined,
            AttributeDefined,
            MongoDefault,
            Conventional,
        }

    }
}