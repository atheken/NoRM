using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Norm.BSON;

namespace Norm.Configuration
{
    /// <summary>
    /// Mongo configuration for a specific type
    /// </summary>
    /// <typeparam name="T">
    /// Type under configuratino
    /// </typeparam>
    public class MongoTypeConfiguration<T> : MongoTypeConfiguration, ITypeConfiguration<T>
    {
        /// <summary>
        /// Looks up property names for use with aliases.
        /// </summary>
        /// <param name="sourcePropery">The source propery.</param>
        /// <returns></returns>
        public IPropertyMappingExpression ForProperty(Expression<Func<T, object>> sourcePropery)
        {
            var propertyName = TypeHelper.FindProperty(sourcePropery);
            var typeKey = typeof(T);
            if (!PropertyMaps.ContainsKey(typeKey))
            {
                PropertyMaps.Add(typeKey, new Dictionary<string, PropertyMappingExpression>());
            }
            var expression = new PropertyMappingExpression { SourcePropertyName = propertyName };
            PropertyMaps[typeKey][propertyName] = expression;
            MongoConfiguration.FireTypeChangedEvent(typeof(T));
            return expression;
        }

        /// <summary>
        /// Uses a name collection for a given type.
        /// </summary>
        /// <param name="connectionStrings">The connection strings.</param>
        public void UseCollectionNamed(string connectionStrings)
        {
            CollectionNames[typeof(T)] = connectionStrings;
            MongoConfiguration.FireTypeChangedEvent(typeof(T));
        }

        /// <summary>
        /// Uses a connection string for a given type.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        public void UseConnectionString(string connectionString)
        {
            ConnectionStrings[typeof(T)] = connectionString;
            MongoConfiguration.FireTypeChangedEvent(typeof(T));
        }

        /// <summary>
        /// Marks a type as a summary of another type (partial get)
        /// </summary>  
        public void SummaryOf<S>()
        {
            SummaryTypes[typeof (T)] = typeof (S);
            MongoConfiguration.FireTypeChangedEvent(typeof(T));
        }
    }
}