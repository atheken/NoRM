namespace NoRM.BSON
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

	internal class ReflectionHelpers
	{
		public static Func<object, object> GetterMethod(PropertyInfo method)
		{
			// First fetch the generic form
			var genericHelper = typeof(ReflectionHelpers).GetMethod("GetterMethod", BindingFlags.Static | BindingFlags.NonPublic);

			// Now supply the type arguments
			var constructedHelper = genericHelper.MakeGenericMethod (method.DeclaringType, method.PropertyType);

			// Now call it. The null argument is because it's a static method.
			var ret = constructedHelper.Invoke(null, new object[] { method });

			// Cast the result to the right kind of delegate and return it
			return (Func<object, object>)ret;
		}
        static Func<object, object> GetterMethod<TTarget, TParam>(PropertyInfo method) where TTarget : class
        {
            // Convert the slow MethodInfo into a fast, strongly typed, open delegate
            var func = (Func<TTarget, TParam>) Delegate.CreateDelegate(typeof(Func<TTarget, TParam>), method.GetGetMethod());
            // Now create a more weakly typed delegate which will call the strongly typed one
            Func<object, object> ret = target => func((TTarget)target);
            return ret;
        }

        private static readonly IDictionary<Type, IList<PropertyInfo>> _cachedTypeLookup = new Dictionary<Type, IList<PropertyInfo>>();

	    public static PropertyInfo FindProperty(Type type, string name)
        {                
            if (!_cachedTypeLookup.ContainsKey(type))
            {
                _cachedTypeLookup[type] = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            }
            foreach(var property in _cachedTypeLookup[type])
            {
                if (string.Compare(property.Name, name, true) == 0)
                {
                    return property;
                }
            }
            throw new ArgumentException(type.FullName + " doesn't have a property named: " + name);
        }
	}
}