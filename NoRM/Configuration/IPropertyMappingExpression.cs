
namespace NoRM.Configuration
{
    public interface IPropertyMappingExpression : IHideObjectMembers
    {
        string SourcePropertyName { get; set; }
        void UseAlias(string alias);
    }
}
