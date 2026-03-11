using Ngaq.Core.Word.Svc;
using Tsinswreng.CsTest;

namespace Ngaq.Local.Test.Domains.Word;

public partial class TestISvcWord:I_RegisterTests{
	ISvcWord SvcWord;
	//ITestMgr Test;
	public TestISvcWord(
		ISvcWord SvcWord
		//,ITestMgr Test
	){
		this.SvcWord = SvcWord;
		//this.Test = Test;
	}
	
	public ITestNode RegisterTests(ITestNode? Test){
		Test??=new TestNode();
		var R = Test.MkFnRegisterTest(typeof(TestISvcWord), typeof(ISvcWord));
		
		R("FailedOnPurpose", async(o)=>{
			throw new Exception("Failed on purpose");
			return NIL;
		});
		return Test;
	}
}
