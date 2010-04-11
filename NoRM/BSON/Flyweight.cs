using System;
using System.Collections.Generic;
using System.Linq;

namespace Norm.BSON
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
        private readonly Dictionary<string, bool?> _booleanProps = new Dictionary<string, bool?>(0, StringComparer.InvariantCultureIgnoreCase);
        private readonly Dictionary<string, double?> _doubleProps = new Dictionary<string, double?>(0, StringComparer.InvariantCultureIgnoreCase);
        private readonly Dictionary<string, int?> _intProps = new Dictionary<string, int?>(0, StringComparer.InvariantCultureIgnoreCase);
        private Dictionary<string, object> _kitchenSinkProps = new Dictionary<string, object>(0, StringComparer.InvariantCultureIgnoreCase);
        private readonly Dictionary<string, long?> _longProps = new Dictionary<string, long?>(0, StringComparer.InvariantCultureIgnoreCase);
        private readonly Dictionary<string, string> _stringProps = new Dictionary<string, string>(0, StringComparer.InvariantCultureIgnoreCase);
        
        /// <summary>
        /// Get or set a property of this flyweight.
        /// </summary>
        /// <param name="propertyName">property name</param>
        /// <returns></returns>
        public object this[string propertyName]
        {
            get
            {
                return this._kitchenSinkProps[propertyName];
            }
            set
            {
                Delete(propertyName);
                _kitchenSinkProps[propertyName] = value;
            }
        }

        /// <summary>
        /// All the properties of this flyweight
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ExpandoProperty> AllProperties()
        {
            return _intProps.Select(y => new ExpandoProperty(y.Key, y.Value)).Concat(
                _doubleProps.Select(y => new ExpandoProperty(y.Key, y.Value))).Concat(
                _longProps.Select(y => new ExpandoProperty(y.Key, y.Value))).Concat(
                _booleanProps.Select(y => new ExpandoProperty(y.Key, y.Value))).Concat(
                _stringProps.Select(y => new ExpandoProperty(y.Key, y.Value))).Concat(
                _kitchenSinkProps.Select(y => new ExpandoProperty(y.Key, y.Value)));
        }

        /// <summary>TODO::Description.</summary>
        public void ReverseKitchen() {
            var reversed = _kitchenSinkProps.Reverse();
            var newKitchen = new Dictionary<string, object>();
            foreach (var item in reversed) {
                newKitchen[item.Key] = item.Value;
            }
            _kitchenSinkProps = newKitchen;
        }
        /// <summary>
        /// Pulls the property of the specified type "T". You better know it's in there or you're going to get an exception.. just sayin'
        /// </summary>
        /// <typeparam name="T">Type of property</typeparam>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        public T Get<T>(string propertyName)
        {
            object retval;

            _kitchenSinkProps.TryGetValue(propertyName, out retval);
            if (retval == null)
            {
                throw new InvalidOperationException("Can't find the property " + propertyName);
            }

            return (T) retval;
        }

        /// <summary>
        /// Whether the property name is in the kitchen sink.
        /// </summary>
        /// <param name="propertyName">The property name.</param>
        /// <returns>The contains.</returns>
        public bool Contains(string propertyName)
        {
            return _kitchenSinkProps.ContainsKey(propertyName);
        }

        /// <summary>
        /// Deletes a property.
        /// </summary>
        /// <param name="propertyName">The property name.</param>
        public void Delete(string propertyName)
        {
            _booleanProps.Remove(propertyName);
            _doubleProps.Remove(propertyName);
            _intProps.Remove(propertyName);
            _kitchenSinkProps.Remove(propertyName);
            _longProps.Remove(propertyName);
            _stringProps.Remove(propertyName);
        }

        /// <summary>
        /// Sets the value on the property name you specify.
        /// remember that this will destroy any other property of the same name
        /// (culture and case-insensitive matching)
        /// </summary>
        /// <typeparam name="T">Type to set</typeparam>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="value">The value.</param>
        public void Set<T>(string propertyName, T value)
        {
            Delete(propertyName);
            _kitchenSinkProps[propertyName] = value;
        }

        /// <summary>
        /// Attempts to read the value out of the flyweight, if it's not here,
        /// value is set to default(T) and the method returns false.
        /// </summary>
        /// <typeparam name="T">Type to try to get</typeparam>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="value">The value.</param>
        /// <returns>The try get.</returns>
        public bool TryGet<T>(string propertyName, out T value)
        {
            var retval = false;
            value = default(T);

            try
            {
                value = (T) _kitchenSinkProps[propertyName];

                retval = true;
            }
            catch
            {
                // it's fine, we don't care.
            }

            return retval;
        }
    }
}