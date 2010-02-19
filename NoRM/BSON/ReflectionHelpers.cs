using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;

namespace NoRM.BSON
{
	/// <summary>
	/// Provides some reflection methods to produce delegates rather than
	/// later-bound method calls on instance properties.
	/// </summary>
	public class ReflectionHelpers
	{
		private static Dictionary<PropertyInfo, Func<object, object>> _getters = new Dictionary<PropertyInfo, Func<object, object>>();
		private static Dictionary<PropertyInfo, Action<object, object>> _setters = new Dictionary<PropertyInfo, Action<object, object>>();

		public static Func<object, object> GetterMethod(PropertyInfo property)
		{
			if (_getters.ContainsKey(property))
				return _getters[property];

			var dynamicMethod = new DynamicMethod(GetAnonymousMethodName(), typeof(object), new[] { typeof(object) }, property.DeclaringType, true);
			MethodInfo getMethod = property.GetGetMethod();

			ILGenerator il = dynamicMethod.GetILGenerator();

			il.Emit(OpCodes.Ldarg_0);
			EmitUnboxOrCast(il, property.DeclaringType);

			EmitMethodCall(il, getMethod);
			EmitBoxOrCast(il, property.PropertyType);

			il.Emit(OpCodes.Ret);

			var getter = (Func<object, object>)dynamicMethod.CreateDelegate(typeof(Func<object, object>));
			_getters.Add(property, getter);

			return getter;
		}

		public static Action<object, object> SetterMethod(PropertyInfo property)
		{
			if (_setters.ContainsKey(property))
				return _setters[property];

			var dynamicMethod = new DynamicMethod(GetAnonymousMethodName(), typeof(void), new[] { typeof(object), typeof(object) }, true);

			ILGenerator il = dynamicMethod.GetILGenerator();

			il.Emit(OpCodes.Ldarg_0);
			EmitUnboxOrCast(il, property.DeclaringType);

			il.Emit(OpCodes.Ldarg_1);
			EmitUnboxOrCast(il, property.PropertyType);

			EmitMethodCall(il, property.GetSetMethod());

			il.Emit(OpCodes.Ret);

			var setter = (Action<object, object>)dynamicMethod.CreateDelegate(typeof(Action<object, object>));
			_setters.Add(property, setter);

			return setter;
		}

		private static void EmitMethodCall(ILGenerator il, MethodInfo method)
		{
			OpCode opCode = method.IsFinal ? OpCodes.Call : OpCodes.Callvirt;
			il.Emit(opCode, method);
		}

		private static void EmitUnboxOrCast(ILGenerator il, Type type)
		{
			OpCode opCode = type.IsValueType ? OpCodes.Unbox_Any : OpCodes.Castclass;
			il.Emit(opCode, type);
		}

		private static void EmitBoxOrCast(ILGenerator il, Type type)
		{
			OpCode opCode = type.IsValueType ? OpCodes.Box : OpCodes.Castclass;
			il.Emit(opCode, type);
		}

		private static string GetAnonymousMethodName()
		{
			return "DynamicMethod" + Guid.NewGuid().ToString("N");
		}
	}
}
