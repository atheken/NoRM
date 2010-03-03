
namespace NoRM.Configuration
{
    public class PropertyMappingExpression : IPropertyMappingExpression
    {
        public string SourcePropertyName { get; set; }
        internal string Alias { get; set; }

        public void UseAlias(string alias)
        {
            Alias = alias;
        }
    }
}
