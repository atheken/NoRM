using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NoRM.BSON
{
    /// <summary>
    /// Provides a completely blank slate for which to query objects out of the DB.
    /// Arbitrary properties for all!
    /// </summary>
    /// <remarks>
    /// Ok, so this is an abuse of the term "flyweight" - sorry.
    /// </remarks>
    public class Flyweight : IFlyweight
    {        
        private Dictionary<String, int?> _intProps = new Dictionary<string, int?>(0, StringComparer.InvariantCultureIgnoreCase);
        private Dictionary<String, double?> _doubleProps = new Dictionary<string, double?>(0, StringComparer.InvariantCultureIgnoreCase);
        private Dictionary<String, long?> _longProps = new Dictionary<string, long?>(0, StringComparer.InvariantCultureIgnoreCase);
        private Dictionary<String, bool?> _booleanProps = new Dictionary<string, bool?>(0, StringComparer.InvariantCultureIgnoreCase);
        private Dictionary<String, String> _stringProps = new Dictionary<string, string>(0, StringComparer.InvariantCultureIgnoreCase);
        private Dictionary<String, object> _kitchenSinkProps = new Dictionary<string, object>(0, StringComparer.InvariantCultureIgnoreCase);
        public string TypeName { get; set; }
        public int Limit { get; set; }
        public int Skip { get; set; }
        public string MethodCall { get; set; }

        /// <summary>
        /// All the properties of this flyweight
        /// </summary>
        public IEnumerable<ExpandoProperty> AllProperties()
        {
            return this._intProps.Select(y => new ExpandoProperty(y.Key, y.Value)).Concat(
                this._doubleProps.Select(y => new ExpandoProperty(y.Key, y.Value))).Concat(
                this._longProps.Select(y => new ExpandoProperty(y.Key, y.Value))).Concat(
                this._booleanProps.Select(y => new ExpandoProperty(y.Key, y.Value))).Concat(
                this._stringProps.Select(y => new ExpandoProperty(y.Key, y.Value))).Concat(
                this._kitchenSinkProps.Select(y => new ExpandoProperty(y.Key, y.Value)));

        }

        /// <summary>
        /// Pulls the property of the specified type "T". You better know it's in there or you're going to get an exception.. just sayin'
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public T Get<T>(String propertyName)
        {
            object retval;

            this._kitchenSinkProps.TryGetValue(propertyName, out retval);
            return (T)retval;
        }

        public void Delete(String propertyName)
        {
            this._booleanProps.Remove(propertyName);
            this._doubleProps.Remove(propertyName);
            this._intProps.Remove(propertyName);
            this._kitchenSinkProps.Remove(propertyName);
            this._longProps.Remove(propertyName);
            this._stringProps.Remove(propertyName);
        }

        /// <summary>
        /// Get or set a property of this flyweight.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public object this[String propertyName]
        {
            get
            {
                return this._kitchenSinkProps[propertyName];
            }
            set
            {
                this.Delete(propertyName);
                this._kitchenSinkProps[propertyName] = value;
            }
        }

        /// <summary>
        /// Sets the value on the property name you specify.
        /// remember that this will destroy any other property of the same name 
        /// (culture and case-insensitive matching)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        public void Set<T>(String propertyName, T value)
        {
            this.Delete(propertyName);
            this._kitchenSinkProps[propertyName] = value;
        }

        /// <summary>
        /// Attempts to read the value out of the flyweight, if it's not here, 
        /// value is set to default(T) and the method returns false.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGet<T>(String propertyName, out T value)
        {
            bool retval = false;
            value = default(T);

            try
            {
                value = (T)this._kitchenSinkProps[propertyName];

                retval = true;
            }
            catch { 
                //it's fine, we don't care.
            }

            return retval;
        }
    }

    public class ExpandoProperty
    {
        public ExpandoProperty(String name, object value)
        {
            this.PropertyName = name;
            this.Value = value;
        }

        public String PropertyName { get; private set; }
        public Object Value { get; private set; }
    }

}
