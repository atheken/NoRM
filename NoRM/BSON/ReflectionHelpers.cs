using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace NoRM.BSON
{
    /// <summary>
    /// Provides some reflection methods to produce delegates rather than
    /// later-bound method calls on instance properties.
    /// </summary>
    /// <remarks>
    /// Many thanks to Jon Skeet, you rock!!
    /// http://msmvps.com/blogs/jon_skeet/archive/2008/08/09/making-reflection-fly-and-exploring-delegates.aspx
    /// </remarks>
    public class ReflectionHelpers
    {
        public static Func<object, object> GetterMethod(PropertyInfo method)
        {
            // First fetch the generic form
            MethodInfo genericHelper = typeof(ReflectionHelpers).GetMethod("GetterMethod",
                BindingFlags.Static | BindingFlags.NonPublic);

            // Now supply the type arguments
            MethodInfo constructedHelper = genericHelper.MakeGenericMethod
                (method.DeclaringType, method.PropertyType);

            // Now call it. The null argument is because it's a static method.
            object ret = constructedHelper.Invoke(null, new object[] { method });

            // Cast the result to the right kind of delegate and return it
            return (Func<object, object>)ret;
        }

        static Func<object, object> GetterMethod<TTarget, TParam>(PropertyInfo method)
            where TTarget : class
        {
            // Convert the slow MethodInfo into a fast, strongly typed, open delegate
            Func<TTarget, TParam> func = (Func<TTarget, TParam>)
                Delegate.CreateDelegate(typeof(Func<TTarget, TParam>), method.GetGetMethod());

            // Now create a more weakly typed delegate which will call the strongly typed one
            Func<object, object> ret = (object target) => func((TTarget)target);

            return ret;
        }



        public static Action<object, object> SetterMethod(PropertyInfo method)
        {
            // First fetch the generic form
            MethodInfo genericHelper = typeof(ReflectionHelpers).GetMethod("SetterMethod",
                BindingFlags.Static | BindingFlags.NonPublic);

            // Now supply the type arguments
            MethodInfo constructedHelper = genericHelper.MakeGenericMethod
                (method.DeclaringType, method.PropertyType);

            // Now call it. The null argument is because it's a static method.
            object ret = constructedHelper.Invoke(null, new object[] { method });

            // Cast the result to the right kind of delegate and return it
            return (Action<object, object>)ret;
        }



        static Action<object, object> SetterMethod<TTarget, TParam>(PropertyInfo method)
            where TTarget : class
        {
            // Convert the slow MethodInfo into a fast, strongly typed, open delegate
            Action<TTarget, TParam> func = (Action<TTarget, TParam>)
                Delegate.CreateDelegate(typeof(Action<TTarget, TParam>), method.GetSetMethod());

            // Now create a more weakly typed delegate which will call the strongly typed one
            Action<object, object> ret = (object target, object param) => func((TTarget)target, (TParam)param);
            
            return ret;
        }
    }
}
