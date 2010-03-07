using System.Configuration;

namespace NoRM.Tests
{
    /// <summary>
    /// For Authentication tests, you'll need:
    ///   1- a data base with data at c:/data/NoRMAuth/
    ///   2- use admin; 
    ///   3- db.addUser('admin', 'admin');
    ///   4- use main; 
    ///   5- db.addUser('usr', pss');
    /// 
    /// This data isn't touched and is persisted from call to call
    /// This is largely necessary because users must first exist before the server can be lauched in with the --auth parameter
    /// </summary>
    public abstract class AuthenticatedFixture : MongoFixture
    {
        protected override string DataPath
        {
            get { return ConfigurationManager.AppSettings["authDbPath"]; }
        }
        protected override string Arguments
        {
            get { return " --auth"; }
        }
        
        protected string AuthenticatedConnectionString(string userName, string password)
        {
            return string.Format("mongodb://{0}:{1}@localhost:{2}/main", userName, password, Port);
        }
    }
}