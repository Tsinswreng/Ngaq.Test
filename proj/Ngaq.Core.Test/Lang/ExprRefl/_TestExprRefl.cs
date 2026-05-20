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
		R("PropInfo_Exception", async(o)=>{
			var ex = new Exception("test");
			var msg = Refl.Get(ex, x=>x.Message);
			if(msg is not str s || s != ex.Message){
				throw new Exception($"expected {ex.Message} but got {msg}");
			}
			Refl.Set(ex, x=>x.HelpLink, "help");
			if(ex.HelpLink != "help"){
				throw new Exception($"expected help but got {ex.HelpLink}");
			}
			return NIL;
		});
		R("CustomDefinedClass", async(o)=>{
			var c = new MyCls();
			Refl.Set(c, x=>x.MyInt, 123);
			if(Refl.Get(c, x=>x.MyInt) is not int i || i!= 123){
				throw new Exception($"expected 123 but got {Refl.Get(c, x=>x.MyInt)}");
			}
			Refl.Set(c, x=>x.MyStr, "abc");
			if(c.MyStr != "abc"){
				throw new Exception($"expected abc but got {Refl.Get(c, x=>x.MyStr)}");
			}
			return NIL;
		});
		R("GetInfo_PropertyInfo", async(o)=>{
			var info = Refl.GetInfo<MyCls>(x => x.MyStr);
			if(info is not PropertyInfo propInfo){
				throw new Exception($"expected {nameof(PropertyInfo)} but got {info.GetType()}");
			}
			if(propInfo.Name != nameof(MyCls.MyStr)){
				throw new Exception($"expected {nameof(MyCls.MyStr)} but got {propInfo.Name}");
			}
			return NIL;
		});
		R("GetInfo_PropertyMetadata", async(o)=>{
			var info = Refl.GetInfo<MyCls>(x => x.MyStr);
			if(info is not PropertyInfo propInfo){
				throw new Exception($"expected {nameof(PropertyInfo)} but got {info.GetType()}");
			}
			if(propInfo.PropertyType != typeof(str)){
				throw new Exception($"expected {typeof(str)} but got {propInfo.PropertyType}");
			}
			if(propInfo.DeclaringType != typeof(MyCls)){
				throw new Exception($"expected {typeof(MyCls)} but got {propInfo.DeclaringType}");
			}
			if(!propInfo.CanRead || !propInfo.CanWrite){
				throw new Exception("expected property to be readable and writable");
			}
			return NIL;
		});
		R("GetInfo_PropertyValue", async(o)=>{
			var c = new MyCls(){MyStr = "from-info"};
			var info = Refl.GetInfo<MyCls>(x => x.MyStr);
			if(info is not PropertyInfo propInfo){
				throw new Exception($"expected {nameof(PropertyInfo)} but got {info.GetType()}");
			}
			var value = propInfo.GetValue(c);
			if(value is not str s || s != "from-info"){
				throw new Exception($"expected from-info but got {value}");
			}
			return NIL;
		});
		R("GetInfo_PropertyAccessor", async(o)=>{
			var info = Refl.GetInfo<MyCls>(x => x.MyStr);
			if(info is not PropertyInfo propInfo){
				throw new Exception($"expected {nameof(PropertyInfo)} but got {info.GetType()}");
			}
			if(propInfo.GetMethod is null || propInfo.SetMethod is null){
				throw new Exception("expected getter and setter to exist");
			}
			if(propInfo.GetMethod.Name != "get_MyStr"){
				throw new Exception($"expected get_MyStr but got {propInfo.GetMethod.Name}");
			}
			if(propInfo.SetMethod.Name != "set_MyStr"){
				throw new Exception($"expected set_MyStr but got {propInfo.SetMethod.Name}");
			}
			return NIL;
		});
		R("GetInfo_SetPropertyValue", async(o)=>{
			var c = new MyCls();
			var info = Refl.GetInfo<MyCls>(x => x.MyStr);
			if(info is not PropertyInfo propInfo){
				throw new Exception($"expected {nameof(PropertyInfo)} but got {info.GetType()}");
			}
			propInfo.SetValue(c, "set-by-info");
			if(c.MyStr != "set-by-info"){
				throw new Exception($"expected set-by-info but got {c.MyStr}");
			}
			return NIL;
		});
		R("GetInfo_CustomAttr", async(o)=>{
			var info = Refl.GetInfo<MyCls>(x => x.MyStr);
			if(info is not PropertyInfo propInfo){
				throw new Exception($"expected {nameof(PropertyInfo)} but got {info.GetType()}");
			}
			var attr = propInfo.GetCustomAttribute<MyAttr>();
			if(attr is null){
				throw new Exception("expected MyAttr but got null");
			}
			if(attr.Name != "MyStrProp"){
				throw new Exception($"expected MyStrProp but got {attr.Name}");
			}
			if(attr.Type != typeof(TestExprRefl)){
				throw new Exception($"expected {typeof(TestExprRefl)} but got {attr.Type}");
			}
			return NIL;
		});
		R("GetInfo_StandardAttr", async(o)=>{
			var info = Refl.GetInfo<MyCls>(x => x.LegacyNum);
			if(info is not PropertyInfo propInfo){
				throw new Exception($"expected {nameof(PropertyInfo)} but got {info.GetType()}");
			}
			if(!propInfo.IsDefined(typeof(ObsoleteAttribute), false)){
				throw new Exception($"expected {nameof(ObsoleteAttribute)} on {propInfo.Name}");
			}
			var attr = propInfo.GetCustomAttribute<ObsoleteAttribute>();
			if(attr is null){
				throw new Exception($"expected {nameof(ObsoleteAttribute)} but got null");
			}
			if(attr.Message != "for-reflection-test"){
				throw new Exception($"expected for-reflection-test but got {attr.Message}");
			}
			return NIL;
		});
		R("GetInfo_FieldInfo", async(o)=>{
			var info = Refl.GetInfo<MyCls>(x => x.MyField);
			if(info is not FieldInfo fieldInfo){
				throw new Exception($"expected {nameof(FieldInfo)} but got {info.GetType()}");
			}
			if(fieldInfo.Name != nameof(MyCls.MyField)){
				throw new Exception($"expected {nameof(MyCls.MyField)} but got {fieldInfo.Name}");
			}
			if(fieldInfo.FieldType != typeof(int)){
				throw new Exception($"expected {typeof(int)} but got {fieldInfo.FieldType}");
			}
			return NIL;
		});
		R("GetInfo_FieldValue", async(o)=>{
			var c = new MyCls(){MyField = 7};
			var info = Refl.GetInfo<MyCls>(x => x.MyField);
			if(info is not FieldInfo fieldInfo){
				throw new Exception($"expected {nameof(FieldInfo)} but got {info.GetType()}");
			}
			var value = fieldInfo.GetValue(c);
			if(value is not int i || i != 7){
				throw new Exception($"expected 7 but got {value}");
			}
			fieldInfo.SetValue(c, 9);
			if(c.MyField != 9){
				throw new Exception($"expected 9 but got {c.MyField}");
			}
			return NIL;
		});
		R("GetInfo_FieldCustomAttrs", async(o)=>{
			var info = Refl.GetInfo<MyCls>(x => x.MyField);
			if(info is not FieldInfo fieldInfo){
				throw new Exception($"expected {nameof(FieldInfo)} but got {info.GetType()}");
			}
			if(!fieldInfo.IsDefined(typeof(MyAttr), false)){
				throw new Exception($"expected {nameof(MyAttr)} on {fieldInfo.Name}");
			}
			MyAttr? firstAttr = null;
			var attrCount = 0;
			foreach(var attr in fieldInfo.GetCustomAttributes<MyAttr>()){
				firstAttr ??= attr;
				attrCount++;
			}
			if(attrCount != 1){
				throw new Exception($"expected 1 attr but got {attrCount}");
			}
			if(firstAttr is null){
				throw new Exception($"expected {nameof(MyAttr)} but got null");
			}
			if(firstAttr.Name != "MyField"){
				throw new Exception($"expected MyField but got {firstAttr.Name}");
			}
			if(firstAttr.Type != typeof(int)){
				throw new Exception($"expected {typeof(int)} but got {firstAttr.Type}");
			}
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
