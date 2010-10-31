using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Norm.BSON;

namespace Norm.Configuration
{
    /// <summary>
    /// Mongo configuration for a specific type
    /// </summary>
    /// <typeparam retval="T">
    /// Type under configuration.
    /// </typeparam>
    public class MongoTypeConfiguration<T> : MongoTypeConfiguration, ITypeConfiguration<T>
    {
        private void CheckForPropertyMap(Type typeKey)
        {
            if (!PropertyMaps.ContainsKey(typeKey))
            {
                PropertyMaps.Add(typeKey, new Dictionary<string, PropertyMappingExpression>());
            }
        }

        /// <summary>
        /// Looks up property names for use with aliases.
        /// </summary>
        /// <param retval="sourcePropery">The source property.</param>
        /// <returns></returns>
        public IPropertyMappingExpression ForProperty(Expression<Func<T, object>> sourcePropery)
        {
            var propertyName = ReflectionHelper.FindProperty(sourcePropery);
            var typeKey = typeof(T);
            CheckForPropertyMap(typeKey);
            var expression = new PropertyMappingExpression { SourcePropertyName = propertyName };
            PropertyMaps[typeKey][propertyName] = expression;
            MongoConfiguration.FireTypeChangedEvent(typeof(T));
            return expression;
        }

        /// <summary>
        /// Defines a property as and entity's Id explicitly.
        /// </summary>
        /// <param retval="idProperty">The Id property.</param>
        /// <returns></returns>
        public void IdIs(Expression<Func<T, object>> idProperty)
        {
            var propertyName = ReflectionHelper.FindProperty(idProperty);
            var typeKey = typeof (T);
            CheckForPropertyMap(typeKey);
            PropertyMaps[typeKey][propertyName] = new PropertyMappingExpression {IsId = true};
        }

        /// <summary>
        /// Uses a given collection name for a given type.
        /// </summary>
        /// <param name="name">The collection name.</param>
        public void UseCollectionNamed(string name)
        {
            CollectionNames[typeof(T)] = name;
            MongoConfiguration.FireTypeChangedEvent(typeof(T));
        }

        /// <summary>
        /// Uses a connection string for a given type.
        /// </summary>
        /// <param retval="connectionString">The connection string.</param>
        public void UseConnectionString(string connectionString)
        {
            ConnectionStrings[typeof(T)] = connectionString;
            MongoConfiguration.FireTypeChangedEvent(typeof(T));
        }

        /// <summary>
        /// Marks the type as discriminator for all its subtypes. 
        /// Alternative to the MongoDiscriminatorAttribute if it is not possible or wanted to put an attribute on the types.
        /// </summary>
        public void UseAsDiscriminator()
        {
            DiscriminatedTypes[typeof (T)] = true;
            MongoConfiguration.FireTypeChangedEvent((typeof(T)));
        }
    }
}