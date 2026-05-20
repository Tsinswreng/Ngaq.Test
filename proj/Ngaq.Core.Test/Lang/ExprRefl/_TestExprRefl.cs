using Tsinswreng.CsTreeTest;
using Tsinswreng.CsCore;
using System.Linq.Expressions;
using System.Reflection;

namespace Ngaq.Core.Test.Lang.ExprRefl;

public class MyAttr: Attribute{
	public string? Name{get;set;}
	public Type? Type{get;set;}
	public MyAttr(string? Name = null, Type? Type = null){
		this.Name = Name;
		this.Type = Type;
	}
}

public class Refl
{
	/// <summary>
	/// 从成员表达式中提取成员信息。
	/// </summary>
	public static MemberInfo GetInfo<T>(Expression<Func<T, obj?>> ExprMemb)
	{
		var memberExpr = UnwrapMemberExpression(ExprMemb.Body);
		if (memberExpr == null){
			throw new Exception($"unsupported expr {ExprMemb}");
		}
		return memberExpr.Member;
	}

	public static obj? Get<T>(T Obj, Expression<Func<T, obj?>> ExprMemb)
	{
		var memberExpr = UnwrapMemberExpression(ExprMemb.Body);
		if (memberExpr == null)
			throw new Exception($"unsupported expr {ExprMemb}");

		// 支持属性和字段
		if (memberExpr.Member is PropertyInfo propInfo){
			return propInfo.GetValue(Obj);
		}
			
		if (memberExpr.Member is FieldInfo fieldInfo){
			return fieldInfo.GetValue(Obj);
		}
		throw new Exception($"unsupported member {memberExpr.Member}");
	}

	public static T Set<T>(T Obj, Expression<Func<T, obj?>> ExprMemb, obj? Value)
	{
		var memberExpr = UnwrapMemberExpression(ExprMemb.Body);
		if (memberExpr == null){
			throw new Exception($"unsupported expr {ExprMemb}");
		}
			

		if (memberExpr.Member is PropertyInfo propInfo){
			propInfo.SetValue(Obj, Value);
		}
			
		else if (memberExpr.Member is FieldInfo fieldInfo){
			fieldInfo.SetValue(Obj, Value);
		}
		else{
			throw new Exception($"unsupported member {memberExpr.Member}");
		}
		return Obj;
	}

	/// <summary>去除可能的 Convert 节点，提取最内层的 MemberExpression</summary>
	private static MemberExpression? UnwrapMemberExpression(Expression expr)
	{
		// 处理 Convert/ConvertChecked
		while (expr is UnaryExpression unary && 
			(unary.NodeType == ExpressionType.Convert || 
				unary.NodeType == ExpressionType.ConvertChecked))
		{
			expr = unary.Operand;
		}
		return expr as MemberExpression;
	}
}

public partial class TestExprRefl: ITester{
	
	class MyCls{
		public int MyInt{get;set;}
		[MyAttr(Name = "MyStrProp", Type = typeof(TestExprRefl))]
		public string MyStr{get;set;}
		[Obsolete("for-reflection-test")]
		public int LegacyNum{get;set;}
		[MyAttr(Name = "MyField", Type = typeof(int))]
		public int MyField;
	}
	
	
	[Doc(@$"
	#Params([src obj],[target type], [target obj])
	")]
	public Func<obj?, Type, obj?> Deserialize = null!;
	Func<obj?, Type, obj?> de=>Deserialize;
	public ITestNode RegisterTestsInto(ITestNode? Node){
		Node??=new TestNode();
		
		var register = Node.MkTestFnRegister(
			typeof(TestExprRefl),
			[],
			[],
			nameof(TestExprRefl)
		);
		var R = register.Register;
		var T = Assert.IsTrue;
		R("PropInfo_Exception", async(o)=>{
			var ex = new Exception("test");
			var msg = Refl.Get(ex, x=>x.Message);
			T(msg is str s && s == ex.Message);
			Refl.Set(ex, x=>x.HelpLink, "help");
			T(ex.HelpLink == "help");
			return NIL;
		});
		R("CustomDefinedClass", async(o)=>{
			var c = new MyCls();
			Refl.Set(c, x=>x.MyInt, 123);
			T(Refl.Get(c, x=>x.MyInt) is int i && i == 123);
			Refl.Set(c, x=>x.MyStr, "abc");
			T(c.MyStr == "abc");
			return NIL;
		});
		R("GetInfo_PropertyInfo", async(o)=>{
			var info = Refl.GetInfo<MyCls>(x => x.MyStr);
			T(info is PropertyInfo);
			var propInfo = (PropertyInfo)info;
			T(propInfo.Name == nameof(MyCls.MyStr));
			return NIL;
		});
		R("GetInfo_PropertyMetadata", async(o)=>{
			var info = Refl.GetInfo<MyCls>(x => x.MyStr);
			T(info is PropertyInfo);
			var propInfo = (PropertyInfo)info;
			T(propInfo.PropertyType == typeof(str));
			T(propInfo.DeclaringType == typeof(MyCls));
			T(propInfo.CanRead && propInfo.CanWrite);
			return NIL;
		});
		R("GetInfo_PropertyValue", async(o)=>{
			var c = new MyCls(){MyStr = "from-info"};
			var info = Refl.GetInfo<MyCls>(x => x.MyStr);
			T(info is PropertyInfo);
			var propInfo = (PropertyInfo)info;
			var value = propInfo.GetValue(c);
			T(value is str s && s == "from-info");
			return NIL;
		});
		R("GetInfo_PropertyAccessor", async(o)=>{
			var info = Refl.GetInfo<MyCls>(x => x.MyStr);
			T(info is PropertyInfo);
			var propInfo = (PropertyInfo)info;
			T(propInfo.GetMethod is not null && propInfo.SetMethod is not null);
			T(propInfo.GetMethod!.Name == "get_MyStr");
			T(propInfo.SetMethod!.Name == "set_MyStr");
			return NIL;
		});
		R("GetInfo_SetPropertyValue", async(o)=>{
			var c = new MyCls();
			var info = Refl.GetInfo<MyCls>(x => x.MyStr);
			T(info is PropertyInfo);
			var propInfo = (PropertyInfo)info;
			propInfo.SetValue(c, "set-by-info");
			T(c.MyStr == "set-by-info");
			return NIL;
		});
		R("GetInfo_CustomAttr", async(o)=>{
			var info = Refl.GetInfo<MyCls>(x => x.MyStr);
			T(info is PropertyInfo);
			var propInfo = (PropertyInfo)info;
			var attr = propInfo.GetCustomAttribute<MyAttr>();
			T(attr is not null);
			T(attr!.Name == "MyStrProp");
			T(attr.Type == typeof(TestExprRefl));
			return NIL;
		});
		R("GetInfo_StandardAttr", async(o)=>{
			var info = Refl.GetInfo<MyCls>(x => x.LegacyNum);
			T(info is PropertyInfo);
			var propInfo = (PropertyInfo)info;
			T(propInfo.IsDefined(typeof(ObsoleteAttribute), false));
			var attr = propInfo.GetCustomAttribute<ObsoleteAttribute>();
			T(attr is not null);
			T(attr!.Message == "for-reflection-test");
			return NIL;
		});
		R("GetInfo_FieldInfo", async(o)=>{
			var info = Refl.GetInfo<MyCls>(x => x.MyField);
			T(info is FieldInfo);
			var fieldInfo = (FieldInfo)info;
			T(fieldInfo.Name == nameof(MyCls.MyField));
			T(fieldInfo.FieldType == typeof(int));
			return NIL;
		});
		R("GetInfo_FieldValue", async(o)=>{
			var c = new MyCls(){MyField = 7};
			var info = Refl.GetInfo<MyCls>(x => x.MyField);
			T(info is FieldInfo);
			var fieldInfo = (FieldInfo)info;
			var value = fieldInfo.GetValue(c);
			T(value is int i && i == 7);
			fieldInfo.SetValue(c, 9);
			T(c.MyField == 9);
			return NIL;
		});
		R("GetInfo_FieldCustomAttrs", async(o)=>{
			var info = Refl.GetInfo<MyCls>(x => x.MyField);
			T(info is FieldInfo);
			var fieldInfo = (FieldInfo)info;
			T(fieldInfo.IsDefined(typeof(MyAttr), false));
			MyAttr? firstAttr = null;
			var attrCount = 0;
			foreach(var attr in fieldInfo.GetCustomAttributes<MyAttr>()){
				firstAttr ??= attr;
				attrCount++;
			}
			T(attrCount == 1);
			T(firstAttr is not null);
			T(firstAttr!.Name == "MyField");
			T(firstAttr.Type == typeof(int));
			
			
			return NIL;
		});
/* 
class A{
	public ESeason EnumSeason {get;set;}
	public EWeek EnumWeek {get;set;}
}
var a = new A();
fn(a, a.EnumSeason, x=>x.Spring) -> a.EnumSeason = ESeason.Spring
fn(a, a.EnumWeek, x=>x.Monday) -> a.EnumWeek = EWeek.Monday
 */

		return Node;
	}
}
