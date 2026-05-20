using Tsinswreng.CsTreeTest;
using Tsinswreng.CsCore;
using System.Linq.Expressions;
using System.Reflection;

namespace Ngaq.Core.Test.Lang.ExprRefl;

public class Refl
{
	public static obj? Get<T>(T Obj, Expression<Func<T, obj?>> ExprMemb)
	{
		var memberExpr = UnwrapMemberExpression(ExprMemb.Body);
		if (memberExpr == null)
			throw new Exception($"unsupported expr {ExprMemb}");

		// 支持属性和字段
		if (memberExpr.Member is PropertyInfo propInfo)
			return propInfo.GetValue(Obj);
		if (memberExpr.Member is FieldInfo fieldInfo)
			return fieldInfo.GetValue(Obj);
		
		throw new Exception($"unsupported member {memberExpr.Member}");
	}

	public static T Set<T>(T Obj, Expression<Func<T, obj?>> ExprMemb, obj? Value)
	{
		var memberExpr = UnwrapMemberExpression(ExprMemb.Body);
		if (memberExpr == null)
			throw new Exception($"unsupported expr {ExprMemb}");

		if (memberExpr.Member is PropertyInfo propInfo)
			propInfo.SetValue(Obj, Value);
		else if (memberExpr.Member is FieldInfo fieldInfo)
			fieldInfo.SetValue(Obj, Value);
		else
			throw new Exception($"unsupported member {memberExpr.Member}");
		
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
		public string MyStr{get;set;}
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
