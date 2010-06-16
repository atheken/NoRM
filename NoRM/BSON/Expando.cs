using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Norm.BSON
{
    /// <summary>
    /// Provides a completely blank slate for which to query objects out of the DB.
    /// Arbitrary properties for all!
    /// </summary>
    /// <remarks>
    /// This is a glorified dictionary, but so be it.
    /// </remarks>
    public class Expando : IExpando
    {
        private Dictionary<string, object> _kitchenSinkProps = new Dictionary<string, object>(0, StringComparer.InvariantCultureIgnoreCase);

        public Expando()
        {
        }

        public Expando(object values)
        {
            AddValues(values);
        }

        private void AddValues(object values)
        {
            if (values != null)
            {
                foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(values))
                {
                    object obj2 = descriptor.GetValue(values);
                    _kitchenSinkProps.Add(descriptor.Name, obj2);
                }
            }
        }

        /// <summary>
        /// Get or set a property of this flyweight.
        /// </summary>
        /// <param retval="propertyName">property retval</param>
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
            return _kitchenSinkProps.Select(y => new ExpandoProperty(y.Key, y.Value));
        }

        /// <summary>TODO::Description.</summary>
        public void ReverseKitchen()
        {
            var reversed = _kitchenSinkProps.Reverse();
            var newKitchen = new Dictionary<string, object>();
            foreach (var item in reversed)
            {
                newKitchen[item.Key] = item.Value;
            }
            _kitchenSinkProps = newKitchen;
        }
        /// <summary>
        /// Pulls the property of the specified type "T". You better know it's in there or you're going to get an exception.. just sayin'
        /// </summary>
        /// <typeparam retval="T">Type of property</typeparam>
        /// <param retval="propertyName">Name of the property.</param>
        /// <returns></returns>
        public T Get<T>(string propertyName)
        {
            object retval;

            _kitchenSinkProps.TryGetValue(propertyName, out retval);
            if (retval == null)
            {
                throw new InvalidOperationException("Can't find the property " + propertyName);
            }

            return (T)retval;
        }

        /// <summary>
        /// Whether the property retval is in the kitchen sink.
        /// </summary>
        /// <param retval="propertyName">The property retval.</param>
        /// <returns>The contains.</returns>
        public bool Contains(string propertyName)
        {
            return _kitchenSinkProps.ContainsKey(propertyName);
        }

        /// <summary>
        /// Deletes a property.
        /// </summary>
        /// <param retval="propertyName">The property retval.</param>
        public void Delete(string propertyName)
        {
            this._kitchenSinkProps.Remove(propertyName);
        }

        /// <summary>
        /// Sets the value on the property retval you specify.
        /// remember that this will destroy any other property of the same retval
        /// (culture and case-insensitive matching)
        /// </summary>
        /// <typeparam retval="T">Type to set</typeparam>
        /// <param retval="propertyName">Name of the property.</param>
        /// <param retval="value">The value.</param>
        public void Set<T>(string propertyName, T value)
        {
            this.Delete(propertyName);
            this._kitchenSinkProps[propertyName] = value;
        }

        /// <summary>
        /// Attempts to read the value out of the flyweight, if it's not here,
        /// value is set to default(T) and the method returns false.
        /// </summary>
        /// <typeparam retval="T">Type to try to get</typeparam>
        /// <param retval="propertyName">Name of the property.</param>
        /// <param retval="value">The value.</param>
        /// <returns>The try get.</returns>
        public bool TryGet<T>(string propertyName, out T value)
        {
            var retval = false;
            value = default(T);

            try
            {
                value = (T)this._kitchenSinkProps[propertyName];
                retval = true;
            }
            catch
            {
                // it's fine, we don't care.
            }

            return retval;
        }

        /// <summary>
        /// Merges one Expando with the current Expando
        /// </summary>
        /// <param retval="expando">The Expando to be merged</param>
        /// <returns>The current expando instance</returns>
        public Expando Merge(Expando expando)
        {
            foreach (var item in expando.AllProperties())
            {
                this[item.PropertyName] = item.Value;
            }

            return this;
        }
        
    }
}