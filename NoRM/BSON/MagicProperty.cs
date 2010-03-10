using System;
using System.Reflection;
using NoRM.Attributes;

namespace NoRM.BSON
{

    /// <summary>
    /// A 'magic' property.
    /// </summary>
    internal class MagicProperty
    {
        private static readonly Type _ignoredIfNullType = typeof(MongoIgnoreIfNullAttribute);
        private static readonly Type _myType = typeof(MagicProperty);
        private readonly bool _ignoreIfNull;
        private readonly PropertyInfo _property;

        /// <summary>
        /// Initializes a new instance of the <see cref="MagicProperty"/> class.
        /// </summary>
        /// <param name="property">The property.</param>
        public MagicProperty(PropertyInfo property)
        {
            _property = property;
            _ignoreIfNull = property.GetCustomAttributes(_ignoredIfNullType, true).Length > 0;
            Getter = CreateGetterMethod(property);
            Setter = CreateSetterMethod(property);
        }

        /// <summary>
        /// Gets a property type.
        /// </summary>
        public Type Type
        {
            get { return _property.PropertyType; }
        }

        /// <summary>
        /// Gets a property name.
        /// </summary>
        public string Name
        {
            get { return _property.Name; }
        }

        /// <summary>
        /// Gets a value indicating whether to ignore nulls.
        /// </summary>
        public bool IgnoreIfNull
        {
            get { return _ignoreIfNull; }
        }

        /// <summary>
        /// Gets an action for property setting.
        /// </summary>
        public Action<object, object> Setter { get; private set; }

        /// <summary>
        /// Gets an action for property getting.
        /// </summary>
        public Func<object, object> Getter { get; private set; }

        /// <summary>
        /// The create setter method.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns></returns>
        private static Action<object, object> CreateSetterMethod(PropertyInfo property)
        {
            var genericHelper = _myType.GetMethod("SetterMethod", BindingFlags.Static | BindingFlags.NonPublic);
            var constructedHelper = genericHelper.MakeGenericMethod(property.DeclaringType, property.PropertyType);
            return (Action<object, object>)constructedHelper.Invoke(null, new object[] { property });
        }

        /// <summary>
        /// The create getter method.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns></returns>
        private static Func<object, object> CreateGetterMethod(PropertyInfo property)
        {
            var genericHelper = _myType.GetMethod("GetterMethod", BindingFlags.Static | BindingFlags.NonPublic);
            var constructedHelper = genericHelper.MakeGenericMethod(property.DeclaringType, property.PropertyType);
            return (Func<object, object>)constructedHelper.Invoke(null, new object[] { property });
        }

        // called via reflection

        /// <summary>
        /// The setter method.
        /// </summary>
        /// <typeparam name="TTarget">The type of the target.</typeparam>
        /// <typeparam name="TParam">The type of the param.</typeparam>
        /// <param name="method">The method.</param>
        /// <returns></returns>
        private static Action<object, object> SetterMethod<TTarget, TParam>(PropertyInfo method) where TTarget : class
        {
            var m = method.GetSetMethod(true);
            if (m == null)
            {
                return null;
            }

            // no setter
            var func = (Action<TTarget, TParam>)Delegate.CreateDelegate(typeof(Action<TTarget, TParam>), m);
            return (target, param) => func((TTarget)target, (TParam)param);
        }

        /// <summary>
        /// The getter method.
        /// </summary>
        /// <typeparam name="TTarget">The type of the target.</typeparam>
        /// <typeparam name="TParam">The type of the param.</typeparam>
        /// <param name="method">The method.</param>
        /// <returns></returns>
        private static Func<object, object> GetterMethod<TTarget, TParam>(PropertyInfo method) where TTarget : class
        {
            var m = method.GetGetMethod(true);
            var func = (Func<TTarget, TParam>)Delegate.CreateDelegate(typeof(Func<TTarget, TParam>), m);
            return target => func((TTarget)target);
        }
    }
}
