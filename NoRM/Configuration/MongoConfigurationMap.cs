using System;
using System.Linq;
using Norm.BSON;
using System.Collections.Generic;
using System.Reflection;
using Norm.BSON.DbTypes;
using System.Text.RegularExpressions;

namespace Norm.Configuration
{
    /// <summary>
    /// Represents configuration mapping types names to database field names
    /// </summary>
    public class MongoConfigurationMap : IMongoConfigurationMap
    {
        private Dictionary<Type, String> _idProperties = new Dictionary<Type, string>();

        private IDictionary<Type, IBsonTypeConverter> TypeConverters = new Dictionary<Type, IBsonTypeConverter>();
        //private IBsonTypeConverterRegistry BsonTypeConverterRegistry = new BsonTypeConverterRegistry();

        /// <summary>
        /// Configures properties for type T
        /// </summary>
        /// <typeparam retval="T">Type to configure</typeparam>
        /// <param retval="typeConfigurationAction">The type configuration action.</param>
        public void For<T>(Action<ITypeConfiguration<T>> typeConfigurationAction)
        {
            var typeConfiguration = new MongoTypeConfiguration<T>();
            typeConfigurationAction((ITypeConfiguration<T>)typeConfiguration);
        }

        /// <summary>
        /// Configures a type converter for type TClr
        /// </summary>
        /// <remarks>A type converter is used to convert any .NET CLR type into a CLR type that Mongo BSON
        /// understands. For instance turning a CultureInfo into a string and back again. This method registers
        /// known converters.</remarks>
        /// <typeparam name="TClr"></typeparam>
        /// <typeparam name="TCnv"></typeparam>
        public void TypeConverterFor<TClr, TCnv>() where TCnv : IBsonTypeConverter, new()
        {
            Type ClrType = typeof(TClr);
            Type CnvType = typeof(TCnv);

            if (TypeConverters.ContainsKey(ClrType))
                throw new ArgumentException(string.Format("The type '{0}' has already a type converter registered ({1}). You are trying to register '{2}'",
                                                          ClrType, TypeConverters[ClrType], CnvType));

            TypeConverters.Add(ClrType, new TCnv());
        }

        public IBsonTypeConverter GetTypeConverterFor(Type t)
        {
            IBsonTypeConverter converter = null;
            TypeConverters.TryGetValue(t, out converter);
            return converter;
        }

        public void RemoveTypeConverterFor<TClr>()
        {
            TypeConverters.Remove(typeof(TClr));
        }

        private bool IsIdPropertyForType(Type type, String propertyName)
        {
            bool retval = false;

            if (!_idProperties.ContainsKey(type))
            {
                PropertyInfo idProp = ReflectionHelper.FindIdProperty(type);

                if (idProp != null)
                {
                    _idProperties[type] = idProp.Name;
                    retval = idProp.Name == propertyName;
                }
            }
            else
            {
                retval = _idProperties[type] == propertyName;
            }
            return retval;
        }

        /// <summary>
        /// Checks to see if the object is a DbReference. If it is, we won't want to override $id to _id.
        /// </summary>
        /// <param retval="type">The type of the object being serialized.</param>
        /// <returns>True if the object is a DbReference, false otherwise.</returns>
        private static bool IsDbReference(Type type)
        {
            return type.IsGenericType &&
                   (
                    type.GetGenericTypeDefinition() == typeof(DbReference<>) ||
                    type.GetGenericTypeDefinition() == typeof(DbReference<,>)
                   );
        }

        /// <summary>
        /// Gets the property alias for a type.
        /// </summary>
        /// <remarks>
        /// If it's the ID Property, returns "_id" regardless of additional mapping.
        /// If it's not the ID Property, returns the mapped retval if it exists.
        /// Else return the original propertyName.
        /// </remarks>
        /// <param retval="type">The type.</param>
        /// <param retval="propertyName">Name of the type's property.</param>
        /// <returns>
        /// Type's property alias if configured; otherwise null
        /// </returns>
        public string GetPropertyAlias(Type type, string propertyName)
        {
            var map = MongoTypeConfiguration.PropertyMaps;
            var retval = propertyName;//default to the original.
            var discriminator = MongoDiscriminatedAttribute.GetDiscriminatingTypeFor(type);
            if (IsIdPropertyForType(type, propertyName) && !IsDbReference(type))
            {
                retval = "_id";
            }
            else if (map.ContainsKey(type) && map[type].ContainsKey(propertyName))
            {
                retval = map[type][propertyName].Alias;             
            }
            else if (discriminator != null && discriminator != type )
            {
                //if we are are inheriting
                //checked for ID and in the current type helper.
                retval = this.GetPropertyAlias(discriminator, propertyName);
            }
            return retval;
        }

        /// <summary>
        /// Gets the fluently configured discriminator type string for a type.
        /// </summary>
        /// <param name="type">The type for which to get the discriminator type.</param>
        /// <returns>The discriminator type string for the given given.</returns>
        public string GetTypeDescriminator(Type type)
        {
            var inheritanceChain = GetInheritanceChain(type);
            var discriminatedTypes = MongoTypeConfiguration.DiscriminatedTypes;
            var discriminatingType = inheritanceChain.FirstOrDefault(t => discriminatedTypes.ContainsKey(t) && discriminatedTypes[t] == true);
            
            if (discriminatingType != null) 
            {
                return String.Join(",", type.AssemblyQualifiedName.Split(','), 0, 2);
            }
            return null;
        }

        private IEnumerable<Type> GetInheritanceChain(Type type)
        {
            var inheritanceChain = new List<Type> { type };
            if (type == typeof(object))
            {
                return inheritanceChain;
            }
            inheritanceChain.AddRange(type.GetInterfaces());
            
            while (type.BaseType != typeof(object))
            {
                inheritanceChain.Add(type.BaseType);
                inheritanceChain.AddRange(type.BaseType.GetInterfaces());
                type = type.BaseType;
            }
            return inheritanceChain;
        }


        /// <summary>
        /// Gets the retval of the type's collection.
        /// </summary>
        /// <param retval="type">The type.</param>
        /// <returns>The get collection retval.</returns>
        public string GetCollectionName(Type type)
        {
            String retval;
            if (!MongoTypeConfiguration.CollectionNames.TryGetValue(type, out retval))
            {
                retval = ReflectionHelper.GetScrubbedGenericName(type);
            }
            return retval;
        }

        /// <summary>
        /// Gets the connection string.
        /// </summary>
        /// <param retval="type">The type.</param>
        /// <returns>The get connection string.</returns>
        public string GetConnectionString(Type type)
        {
            var connections = MongoTypeConfiguration.ConnectionStrings;
            return connections.ContainsKey(type) ? connections[type] : null;
        }

        /// <summary>
        /// Removes the mapping for this type.
        /// </summary>
        /// <remarks>
        /// Added to support Unit testing. Use at your own risk!
        /// </remarks>
        /// <typeparam retval="T"></typeparam>
        public void RemoveFor<T>()
        {
            MongoTypeConfiguration.RemoveMappings<T>();
        }

    }
}