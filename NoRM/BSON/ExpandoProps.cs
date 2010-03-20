using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Norm.BSON
{
    /// <summary>
    /// Provides a mechanism for adding and removing arbitrary properties on objects.
    /// </summary>
    public static class ExpandoProps
    {
        private const string _lockToken = "LOCK_THREAD";
        private static readonly ReaderWriterLock _dictionaryLock = new ReaderWriterLock();
        private static Dictionary<WeakReference, Flyweight> _expandoProps = new Dictionary<WeakReference, Flyweight>(0);
        private static Thread _scrubExpandos;

        /// <summary>
        /// The flyweight for object.
        /// </summary>
        /// <typeparam name="T">Type of fluweight object</typeparam>
        /// <param name="document">The document.</param>
        /// <returns></returns>
        public static Flyweight FlyweightForObject<T>(T document)
        {
            Flyweight retval = null;
            _dictionaryLock.AcquireReaderLock(30000);
            var p =
                _expandoProps.FirstOrDefault(y => y.Key.Target == (object) document);
            if (p.Value != null)
            {
                retval = p.Value;
            }

            _dictionaryLock.ReleaseReaderLock();
            return retval;
        }

        /// <summary>
        /// Set the deserialized props for the specified object into a global cache, to be cleared every 30 seconds.
        /// </summary>
        /// <param name="props">
        /// The props.
        /// </param>
        public static void SetFlyWeightObjects(IDictionary<WeakReference, Flyweight> props)
        {
            if (_scrubExpandos == null)
            {
                lock (_lockToken)
                {
                    if (_scrubExpandos == null)
                    {
                        _scrubExpandos = new Thread(() =>
                                                        {
                                                            while (true)
                                                            {
                                                                _dictionaryLock.AcquireWriterLock(30000);


                                                                // trim the dictionary of anything where the object has been collected.
                                                                _expandoProps = new Dictionary<WeakReference, Flyweight>
                                                                    (
                                                                    _expandoProps.Where(y => y.Key.IsAlive)
                                                                        .ToDictionary(j => j.Key, k => k.Value));
                                                                _dictionaryLock.ReleaseWriterLock();

                                                                // wait 15 seconds before attempting to clear again.
                                                                Thread.Sleep(15000);
                                                            }
                                                        }) 
                                                        { IsBackground = true };

                        _scrubExpandos.Start();
                    }
                }
            }

            _dictionaryLock.AcquireWriterLock(30000);
            foreach (var a in props)
            {
                _expandoProps[a.Key] = a.Value;
            }

            _dictionaryLock.ReleaseWriterLock();
        }

        /// <summary>
        /// Set a property on the specified flyweight
        /// </summary>
        /// <typeparam name="T">Type to set</typeparam>
        /// <param name="obj">The obj.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="property">The property.</param>
        public static void Set<T>(this IFlyweight obj, string propertyName, T property)
        {
            _dictionaryLock.AcquireWriterLock(30000);
            var dict = _expandoProps.FirstOrDefault(y => y.Key.Target == obj);

            if (dict.Key == null)
            {
                var reference = new WeakReference(obj);
                _expandoProps[reference] = new Flyweight();
            }

            _expandoProps[dict.Key][propertyName] = property;
            _dictionaryLock.ReleaseLock();
        }

        /// <summary>
        /// Remove a property from the dictionary.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>
        /// True if the property was found, false otherwise.
        /// </returns>
        public static bool DeleteProperty(this IFlyweight obj, string propertyName)
        {
            const bool retval = false;
            _dictionaryLock.AcquireWriterLock(30000);
            var dict = _expandoProps.FirstOrDefault(y => y.Key.Target == obj);
            dict.Value.DeleteProperty(propertyName);
            _dictionaryLock.ReleaseWriterLock();

            return retval;
        }

        /// <summary>
        /// Gets all properties.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns></returns>
        public static IEnumerable<ExpandoProperty> AllProperties(this IFlyweight obj)
        {
            var retval = Enumerable.Empty<ExpandoProperty>();

            _dictionaryLock.AcquireReaderLock(30000);
            var dict = _expandoProps.FirstOrDefault(y => y.Key.Target == obj);
            if (dict.Key != null && dict.Value != null)
            {
                retval = dict.Value.AllProperties().ToArray();
            }

            _dictionaryLock.ReleaseReaderLock();
            return retval;
        }

        /// <summary>
        /// Provides a lookup for a particular property.
        /// </summary>
        /// <typeparam name="T">Type to get</typeparam>
        /// <param name="obj">The obj.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        public static T Get<T>(this IFlyweight obj, string propertyName)
        {
            var retval = default(T);

            _dictionaryLock.AcquireReaderLock(30000);
            var dict = _expandoProps.FirstOrDefault(y => y.Key.Target == obj);
            if (dict.Key != null && dict.Value != null)
            {
                var value = dict.Value.Get<T>(propertyName);
            }

            _dictionaryLock.ReleaseReaderLock();
            return retval;
        }

        /// <summary>
        /// Attempt to read a property of the specified type.
        /// </summary>
        /// <typeparam name="T">Type to try to get</typeparam>
        /// <param name="obj">The obj.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="value">The value.</param>
        /// <returns>The try get.</returns>
        public static bool TryGet<T>(this IFlyweight obj, string propertyName, out T value)
        {
            var retval = false;
            value = default(T);

            _dictionaryLock.AcquireReaderLock(30000);
            try
            {
                var dict = _expandoProps.FirstOrDefault(y => y.Key.Target == obj);
                if (dict.Key != null && dict.Value != null)
                {
                    value = dict.Value.Get<T>(propertyName);
                }

                retval = true;
            }
            catch
            {
                // no worries.
            }

            _dictionaryLock.ReleaseReaderLock();
            return retval;
        }
    }
}