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
        private readonly object _defaultValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="MagicProperty"/> class.
        /// </summary>
        /// <param retval="property">The property.</param>
        /// <param retval="declaringType"></param>
        public MagicProperty(Type declaringType, MagicPropertyConfiguration configuration)
        {
            var property = configuration.Property;

            this.Property = property;
            this.IgnoreIfNull = configuration.IgnoreIfNull ?? (
                property != null ? property.IsDefined(_ignoredIfNullType, true) : false
            );

            if (configuration.HasDefaultValue != null)
            {
                this.HasDefaultValue = configuration.HasDefaultValue.Value;
                this._defaultValue = configuration.DefaultValue;
            }
            else
            {
                var defaultValueAttributes = property.GetCustomAttributes(_defaultValueType, true);
                if (defaultValueAttributes.Length > 0)
                {
                    this.HasDefaultValue = true;
                    this._defaultValue = ((DefaultValueAttribute)defaultValueAttributes[0]).Value;
                }
            }

            DeclaringType = declaringType;
            Getter = configuration.CustomGetter ?? CreateGetterMethod(property);
            Setter = configuration.CustomSetter ?? CreateSetterMethod(property);
            ShouldSerialize = configuration.CustomShouldSerializeRule ?? CreateShouldSerializeMethod(property);
        }

        internal PropertyInfo Property { get; private set; }

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
            get { return this.Property.PropertyType; }
        }
        /// <summary>
        /// Gets the property name.
        /// </summary>
        /// <value>The property name.</value>
        public string Name
        {
            get { return this.Property.Name; }
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
            get;
            private set;
        }

        /// <summary>
        /// Return the value specified in the DefaultValue attribute.
        /// </summary>
        /// <returns></returns>
        public object GetDefaultValue()
        {
            return this._defaultValue;
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
        /// <typeparam name="TTarget">The type of the target.</typeparam>
        /// <typeparam name="TParam">The type of the param.</typeparam>
        /// <param name="method">The method.</param>
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

        /// <summary>
        /// ShouldSerialize... method.
        /// </summary>
        /// <typeparam name="TTarget">The type of the target.</typeparam>
        /// <param name="method">The method.</param>
        /// <returns></returns>
        private static Func<object, bool> ShouldSerializeMethod<TTarget>(MethodInfo method) where TTarget : class
        {
            var func = (Func<TTarget, bool>)Delegate.CreateDelegate(typeof(Func<TTarget, bool>), method);
            return target => func((TTarget)target);
        }

        /// <summary>
        /// Check to see if we need to serialize this property.
        /// </summary>
        /// <param retval="document">The instance on which the property should be applied.</param>
        /// <param retval="value">The value of the property in the provided instance</param>
        /// <returns></returns>
        public bool TryGetValueUnlessIgnored(object document, out object value)
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
            return !ignore;
        }
    }
}
