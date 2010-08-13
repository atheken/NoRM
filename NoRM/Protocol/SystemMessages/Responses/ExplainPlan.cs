using System.Collections.Generic;
using System.Linq;
using Norm.BSON;
using Norm.Configuration;
using System;

namespace Norm.Responses
{
    /// <summary>
    /// ExplainPlan plan.
    /// </summary>
    public class ExplainPlan
    {
        /// <summary>
        /// Initializes the <see cref="ExplainPlan"/> class.
        /// </summary>
        static ExplainPlan()
        {
            MongoConfiguration.Initialize(c => c.For<ExplainPlan>(a =>
                      {
                          a.ForProperty(auth => auth.Cursor).UseAlias("cursor");
                          a.ForProperty(auth => auth.StartKey).UseAlias("startKey");
                          a.ForProperty(auth => auth.IndexBounds).UseAlias("indexBounds");
                      })
                );
        }

        /// <summary>
        /// Gets or sets the cursor.
        /// </summary>
        /// <value>The cursor.</value>
        public string Cursor { get; set; }

        /// <summary>
        /// Gets or sets the start fieldSelectionExpando.
        /// </summary>
        /// <value>The start fieldSelectionExpando.</value>
        internal Expando StartKey { get; set; }

        /// <summary>
        /// Gets or sets the end fieldSelectionExpando.
        /// </summary>
        /// <value>The end fieldSelectionExpando.</value>
        internal Expando EndKey { get; set; }

        /// <summary>
        /// Gets or sets the index bounds.
        /// </summary>
        /// <value>The index bounds.</value>
        public Dictionary<String,List<object[]>> IndexBounds { get; set; }

        /// <summary>
        /// Gets the explain start fieldSelectionExpando list.
        /// </summary>
        /// <value>The explain start fieldSelectionExpando.</value>
        public Dictionary<string, string> ExplainStartKey
        {
            get
            {
                var keys = new Dictionary<string, string>();
                if (StartKey != null)
                {
                    var properties = StartKey.AllProperties();

                    properties.ToList().ForEach(p => keys.Add(p.PropertyName, p.Value.ToString()));
                }

                return keys;
            }
        }

        /// <summary>
        /// Gets the explain end fieldSelectionExpando list.
        /// </summary>
        /// <value>The explain end fieldSelectionExpando.</value>
        public Dictionary<string, string> ExplainEndKey
        {
            get
            {
                var keys = new Dictionary<string, string>();
                if (EndKey != null)
                {
                    var properties = EndKey.AllProperties();

                    properties.ToList().ForEach(p => keys.Add(p.PropertyName, p.Value.ToString()));
                }

                return keys;
            }
        }

    
    }
}
