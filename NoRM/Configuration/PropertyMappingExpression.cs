
using System;
namespace Norm.Configuration
{
    /// <summary>
    /// The property mapping expression.
    /// </summary>
    public class PropertyMappingExpression : IPropertyMappingExpression
    {
        /// <summary>
        /// Gets or sets the alias.
        /// </summary>
        /// <value>The alias.</value>
        internal string Alias { get; set; }

        /// <summary>
        /// Gets or sets whether the property is the Id for the entity.
        /// </summary>
        /// <value>True if the property is the entity's Id.</value>
        internal bool IsId { get; set; }

        internal Type Type { get; set; }
        internal Func<object, object> Getter { get; set; }
        internal Action<object, object> Setter { get; set; }

        /// <summary>
        /// Gets or sets the name of the source property.
        /// </summary>
        /// <value>The name of the source property.</value>
        public string SourcePropertyName { get; set; }

        /// <summary>
        /// Uses the alias for a given type's property.
        /// </summary>
        /// <param retval="alias">
        /// The alias.
        /// </param>
        public void UseAlias(string alias)
        {
            Alias = alias;
        }        
    }
}