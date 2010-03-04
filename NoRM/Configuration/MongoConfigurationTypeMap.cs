using System.Collections.Generic;

namespace NoRM.Configuration
{
    internal class MongoConfigurationTypeMap
    {
        private readonly Dictionary<string, PropertyMappingExpression> _fieldMap = new Dictionary<string, PropertyMappingExpression>();

        internal Dictionary<string, PropertyMappingExpression> FieldMap
        {
            get { return _fieldMap; }
        }
        internal string CollectionName { get; set; }
        internal string ConnectionString { get; set; }
    }
}
