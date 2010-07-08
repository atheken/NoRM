using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security;

namespace NorMan.Model
{
    /// <summary>
    /// Represents the components of a MongoConnection
    /// </summary>
    public struct ConnectionAttributes
    {
        public String Server { get; set; }
        public int Port { get; set; }
        public String User { get; set; }
        public String Password { get; set; }
        public String Database { get; set; }

        /// <summary>
        /// Produces the Mongo URI representation of this connection:
        /// "mongodb://server:port/database" 
        /// -or-
        /// "mongodb://user:password@server:port/database"
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            String retval = "";
            if (!String.IsNullOrWhiteSpace(this.User) && !String.IsNullOrWhiteSpace(this.Password))
            {
                String.Format("mongodb://{0}:{1}@{2}:{3}/{4}", this.User, 
                    this.Password, this.Server, this.Port, this.Database);
            }
            else
            {
                String.Format("mongodb://{0}:{1}/{2}", this.Server,
                    this.Port, this.Database);
            }
            return retval;
        }

        #region ICloneable Members

        public object Clone()
        {
            
        }

        #endregion
    }
}
