using Tsinswreng.CsTreeTest;
using Tsinswreng.CsCore;
using Jeffijoe.MessageFormat;

namespace Ngaq.Core.Test.Lib.MsgFmt;

public partial class TestMsgFmt: ITester{
	MessageFormatter MsgFmt = new();
	public ITestNode RegisterTestsInto(ITestNode? Node){
		Node??=new TestNode();
		var register = Node.MkTestFnRegister(
			typeof(TestMsgFmt)
			,[typeof(TestMsgFmt)]
			,[nameof(MessageFormatter.FormatMessage)]
			,nameof(TestMsgFmt) + "."
		);
		var R = register.Register;
		R("SimpleCondition", async(o)=>{
			var template = "there are {0, plural, =0 {zero} =1 {one} =2 {two} other {#}}";
			{
				var got = MsgFmt.FormatMessage(template, new Dictionary<str, obj?>(){
					["0"]=4
				});
				//System.Console.WriteLine(got);
			}
			
			
			return NIL;
		});
		
		return Node;
	}
}
