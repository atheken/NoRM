using System;
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
		public static Func<object, object> GetterMethod(PropertyInfo property)
		{
			var dynamicMethod = new DynamicMethod(GetAnonymousMethodName(), typeof(object), new[] { typeof(object) }, property.DeclaringType, true);
			MethodInfo getMethod = property.GetGetMethod();

			ILGenerator il = dynamicMethod.GetILGenerator();

			il.Emit(OpCodes.Ldarg_0);
			EmitUnboxOrCast(il, property.DeclaringType);

			EmitMethodCall(il, getMethod);
			EmitBoxOrCast(il, property.PropertyType);

			il.Emit(OpCodes.Ret);

			return (Func<object, object>)dynamicMethod.CreateDelegate(typeof(Func<object, object>));
		}

		public static Action<object, object> SetterMethod(PropertyInfo property)
		{
			var dynamicMethod = new DynamicMethod(GetAnonymousMethodName(), typeof(void), new[] { typeof(object), typeof(object) }, true);

			ILGenerator il = dynamicMethod.GetILGenerator();

			il.Emit(OpCodes.Ldarg_0);
			EmitUnboxOrCast(il, property.DeclaringType);

			il.Emit(OpCodes.Ldarg_1);
			EmitUnboxOrCast(il, property.PropertyType);

			EmitMethodCall(il, property.GetSetMethod());

			il.Emit(OpCodes.Ret);

			return (Action<object, object>)dynamicMethod.CreateDelegate(typeof(Action<object, object>));
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
