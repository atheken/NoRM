
namespace NoRM.Configuration
{
    public interface IPropertyMappingExpression : IHideObjectMembers
    {
        /// <summary>
        /// Gets or sets the name of the source property.
        /// </summary>
        /// <value>The name of the source property.</value>
        string SourcePropertyName { get; set; }
        /// <summary>
        /// Uses the alias for a given type's property.
        /// </summary>
        /// <param name="alias">The alias.</param>
        void UseAlias(string alias);
    }
}
