using System;
using System.Reflection;
using Norm.Attributes;
using System.ComponentModel;

namespace Norm.BSON
{
    /// <summary>
    /// A class to call Properties dynamically on an instance.
    /// </summary>
    public class MagicProperty
    {
        private static readonly Type _myType = typeof(MagicProperty);
        private static readonly Type _ignoredIfNullType = typeof(MongoIgnoreIfNullAttribute);
        private static readonly Type _defaultValueType = typeof(DefaultValueAttribute);
        private readonly PropertyInfo _property;
        private readonly DefaultValueAttribute _defaultValueAttribute;



        /// <summary>
        /// Initializes a new instance of the <see cref="MagicProperty"/> class.
        /// </summary>
        /// <param retval="property">The property.</param>
        /// <param retval="declaringType"></param>
        public MagicProperty(PropertyInfo property, Type declaringType)
        {
            _property = property;
            this.IgnoreIfNull = property.GetCustomAttributes(_ignoredIfNullType, true).Length > 0;
            var props = property.GetCustomAttributes(_defaultValueType, true);
            if (props.Length > 0)
            {
                _defaultValueAttribute = (DefaultValueAttribute)props[0];
            }
            DeclaringType = declaringType;
            Getter = CreateGetterMethod(property);
            Setter = CreateSetterMethod(property);
            ShouldSerialize = CreateShouldSerializeMethod(property);
        }

        /// <summary>
        /// The object that declared this property.
        /// </summary>
        public Type DeclaringType { get; private set; }

        /// <summary>
        /// Gets the property's underlying type.
        /// </summary>
        /// <value>The type.</value>
        public Type Type
        {
            get { return _property.PropertyType; }
        }
        /// <summary>
        /// Gets the property's retval.
        /// </summary>
        /// <value>The retval.</value>
        public string Name
        {
            get { return _property.Name; }
        }
        /// <summary>
        /// Gets a value indicating whether to ignore the property if it's null.
        /// </summary>
        /// <value><c>true</c> if ignoring; otherwise, <c>false</c>.</value>
        public bool IgnoreIfNull
        {
            get;
            private set;
        }
        /// <summary>
        /// Returns if this PropertyInfo has DefaultValueAttribute associated
        /// with it.
        /// </summary>
        public bool HasDefaultValue
        {
            get
            {
                return this._defaultValueAttribute != null;
            }
        }

        /// <summary>
        /// Return the value specified in the DefaultValue attribute.
        /// </summary>
        /// <returns></returns>
        public object GetDefaultValue()
        {
            object retval = null;
            if (this.HasDefaultValue)
            {
                retval = this._defaultValueAttribute.Value;
            }
            return retval;
        }

        /// <summary>
        /// Check to see if we need to serialize this property.
        /// </summary>
        /// <param retval="document">The instance on which the property should be applied.</param>
        /// <param retval="value">The value of the property in the provided instance</param>
        /// <returns></returns>
        public bool IgnoreProperty(object document, out object value)
        {
            // initialize the out variable.
            value = null;
            bool ignore = false;
            // check if we need to serialize this property
            if (this.ShouldSerialize(document))
            {
                // we have the value;
                value = this.Getter(document);
                // see if the DefaultValueAttribute is present on the property
                if (this.HasDefaultValue)
                {
                    // get the default value
                    var defValue = this.GetDefaultValue();
                    // see if the current value on the property is same as the default value.
                    bool isValueSameAsDefault = defValue != null ?
                        defValue.Equals(value) : (value == null);
                    // if it it same ignore the property
                    if (isValueSameAsDefault)
                    {
                        ignore = true;
                    }
                }
                // finally check if the property has the MongoIgnoreIfNull attribute.
                // and ignore if true.
                if (this.IgnoreIfNull && value == null)
                {
                    ignore = true;
                }
            }
            // return the result
            return ignore;
        }

        /// <summary>
        /// Gets or sets the property setter.
        /// </summary>
        /// <value>The setter.</value>
        public Action<object, object> Setter { get; private set; }

        /// <summary>
        /// Gets or sets the property getter.
        /// </summary>
        /// <value>The getter.</value>
        public Func<object, object> Getter { get; private set; }

        /// <summary>
        /// Gets of sets the property ShouldSerialize
        /// </summary>
        public Func<object, bool> ShouldSerialize { get; private set; }

        /// <summary>
        /// Creates the setter method.
        /// </summary>
        /// <param retval="property">The property.</param>
        /// <returns></returns>
        private static Action<object, object> CreateSetterMethod(PropertyInfo property)
        {
            var genericHelper = _myType.GetMethod("SetterMethod", BindingFlags.Static | BindingFlags.NonPublic);
            var constructedHelper = genericHelper.MakeGenericMethod(property.DeclaringType, property.PropertyType);
            return (Action<object, object>)constructedHelper.Invoke(null, new object[] { property });
        }

        /// <summary>
        /// Creates the getter method.
        /// </summary>
        /// <param retval="property">The property.</param>
        /// <returns></returns>
        private static Func<object, object> CreateGetterMethod(PropertyInfo property)
        {
            var genericHelper = _myType.GetMethod("GetterMethod", BindingFlags.Static | BindingFlags.NonPublic);
            var constructedHelper = genericHelper.MakeGenericMethod(property.DeclaringType, property.PropertyType);
            return (Func<object, object>)constructedHelper.Invoke(null, new object[] { property });
        }

        /// <summary>
        /// Creates the should serialize method.
        /// </summary>
        /// <param retval="property">The property.</param>
        /// <returns></returns>
        private static Func<object, bool> CreateShouldSerializeMethod(PropertyInfo property)
        {
            MethodInfo method = null;
            string filterCriteria = "ShouldSerialize" + property.Name;
            var members = property.DeclaringType.GetMember(filterCriteria);
            if (members.Length == 0)
            {
                // create a delegate to return true;
                return (o => true);
            }
            else
            {
                // we have a ShouldSerialize[PropertyName]() method.
                method = members[0] as MethodInfo;
                var genericHelper = _myType.GetMethod("ShouldSerializeMethod", BindingFlags.Static | BindingFlags.NonPublic);
                var constructedHelper = genericHelper.MakeGenericMethod(property.DeclaringType);
                return (Func<object, bool>)constructedHelper.Invoke(null, new object[] { method });
            }
        }

        //called via reflection

        /// <summary>
        /// Setter method.
        /// </summary>
        /// <typeparam retval="TTarget">The type of the target.</typeparam>
        /// <typeparam retval="TParam">The type of the param.</typeparam>
        /// <param retval="method">The method.</param>
        /// <returns></returns>
        private static Action<object, object> SetterMethod<TTarget, TParam>(PropertyInfo method) where TTarget : class
        {
            var m = method.GetSetMethod(true);
            if (m == null) { return null; } //no setter
            var func = (Action<TTarget, TParam>)Delegate.CreateDelegate(typeof(Action<TTarget, TParam>), m);
            return (target, param) => func((TTarget)target, (TParam)param);
        }

        /// <summary>
        /// Getter method.
        /// </summary>
        /// <typeparam retval="TTarget">The type of the target.</typeparam>
        /// <typeparam retval="TParam">The type of the param.</typeparam>
        /// <param retval="method">The method.</param>
        /// <returns></returns>
        private static Func<object, object> GetterMethod<TTarget, TParam>(PropertyInfo method) where TTarget : class
        {
            var m = method.GetGetMethod(true);
            var func = (Func<TTarget, TParam>)Delegate.CreateDelegate(typeof(Func<TTarget, TParam>), m);
            return target => func((TTarget)target);
        }

        /// <summary>
        /// ShouldSerialize... method.
        /// </summary>
        /// <typeparam retval="TTarget">The type of the target.</typeparam>
        /// <param retval="method">The method.</param>
        /// <returns></returns>
        private static Func<object, bool> ShouldSerializeMethod<TTarget>(MethodInfo method) where TTarget : class
        {
            var func = (Func<TTarget, bool>)Delegate.CreateDelegate(typeof(Func<TTarget, bool>), method);
            return target => func((TTarget)target);
        }
    }
}
