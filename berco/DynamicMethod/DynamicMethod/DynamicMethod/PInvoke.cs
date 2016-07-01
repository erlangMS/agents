﻿using System;
using System.Dynamic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Threading;
using System.Linq.Expressions;

class PInvokeMetaObject : DynamicMetaObject
{

	public PInvokeMetaObject(Expression parameter, object o) :
		base(parameter, BindingRestrictions.Empty, o)
	{ }

	public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args)
	{

		var self = this.Expression;
		var pinvoke = (PInvoke)base.Value;

		var arg_types = new Type[args.Length];
		var arg_exps = new Expression[args.Length];

		for (int i = 0; i < args.Length; ++i)
		{
			arg_types[i] = args[i].LimitType;
			arg_exps[i] = args[i].Expression;
		}

		var m = pinvoke.GetInvoke(binder.Name, arg_types);
		var target = Expression.Block(
				   Expression.Call(m, arg_exps),
				   Expression.Default(typeof(object)));
		var restrictions = BindingRestrictions.GetTypeRestriction(self, typeof(PInvoke));

		return new DynamicMetaObject(target, restrictions);
	}
}

public class PInvoke : DynamicObject
{
	string dll;

	AssemblyBuilder ab;
	ModuleBuilder moduleb;
	int id_gen;

	public PInvoke(string dll)
	{
		this.dll = dll;
	}

	public override DynamicMetaObject GetMetaObject(Expression parameter)
	{
		return new PInvokeMetaObject(parameter, this);
	}

	public MethodInfo GetInvoke(string entry_point, Type[] arg_types)
	{
		if (ab == null)
		{
			AssemblyName aname = new AssemblyName("ctype");
			ab = AppDomain.CurrentDomain.DefineDynamicAssembly(aname, AssemblyBuilderAccess.Run);
			moduleb = ab.DefineDynamicModule("ctype");
		}

		// Can't use DynamicMethod as they don't support custom attributes
		var tb = moduleb.DefineType("ctype_" + Interlocked.Increment(ref id_gen) + "_" + entry_point);

		tb.DefinePInvokeMethod("Invoke", dll, entry_point,
				   MethodAttributes.Static | MethodAttributes.PinvokeImpl,
				   CallingConventions.Standard, typeof
				   (void), arg_types,
				   CallingConvention.StdCall, CharSet.Auto);

		var t = tb.CreateType();
		var m = t.GetMethod("Invoke", BindingFlags.Static | BindingFlags.NonPublic);

		return m;
	}
}