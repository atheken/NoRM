using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Norm.Linq
{
    /// <summary>
    /// MongoQueryResults
    /// </summary>
    public interface IMongoQueryResults
    {
        /// <summary>
        /// Returns the translation results from a linq query
        /// </summary>
        QueryTranslationResults TranslationResults { get; }
    }
}
