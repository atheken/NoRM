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
    /// Type under configuratino
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

        private PropertyMappingExpression CheckForPropertyExpression(Expression<Func<T, object>> idProperty)
        {
            var propertyName = ReflectionHelper.FindProperty(idProperty);
            var typeKey = typeof(T);
            CheckForPropertyMap(typeKey);
            var expression = PropertyMaps[typeKey].ContainsKey(propertyName) ? 
                PropertyMaps[typeKey][propertyName] :
                new PropertyMappingExpression();
            PropertyMaps[typeKey][propertyName] = expression;

            return expression;
        }

        /// <summary>
        /// Looks up property names for use with aliases.
        /// </summary>
        /// <param retval="sourcePropery">The source propery.</param>
        /// <returns></returns>
        public IPropertyMappingExpression ForProperty(Expression<Func<T, object>> sourcePropery)
        {
            var expression = CheckForPropertyExpression(sourcePropery);
            expression.SourcePropertyName = ReflectionHelper.FindProperty(sourcePropery);
            MongoConfiguration.FireTypeChangedEvent(typeof(T));

            return expression;
        }

        /// <summary>
        /// Defines a property as and entity's Id explicitly.
        /// </summary>
        /// <param retval="idProperty">The Id propery.</param>
        /// <returns></returns>
        public void IdIs(Expression<Func<T, object>> idProperty)
        {
            var expression = CheckForPropertyExpression(idProperty);
            expression.IsId = true;
            MongoConfiguration.FireTypeChangedEvent(typeof(T));
        }

        ///<summary>
        /// Ignores the given property
        ///</summary>
        ///<param name="property">The property to ignore during serialization</param>
        public void IgnoreProperty(Expression<Func<T, object>> property)
        {
            var expression = CheckForPropertyExpression(property);
            expression.Ignore = true;
            MongoConfiguration.FireTypeChangedEvent(typeof(T));
        }

        /// <summary>
        /// Uses a retval collection for a given type.
        /// </summary>
        /// <param retval="connectionStrings">The connection strings.</param>
        public void UseCollectionNamed(string connectionStrings)
        {
            CollectionNames[typeof(T)] = connectionStrings;
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