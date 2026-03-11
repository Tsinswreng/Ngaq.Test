using Ngaq.Core.Word.Svc;
using Tsinswreng.CsTest;

namespace Ngaq.Local.Test.Domains.Word;

public partial class TestISvcWord:ITester{
	ISvcWord SvcWord;
	public TestISvcWord(
		ISvcWord SvcWord
	){
		this.SvcWord = SvcWord;
	}
	
	public ITestNode RegisterTestsInto(ITestNode? Test){
		Test??=new TestNode();
		var R = Test.MkFnRegisterTest(typeof(TestISvcWord), typeof(ISvcWord));
		
		R("FailedOnPurpose", async(o)=>{
			throw new Exception("Failed on purpose");
			return NIL;
		});
		return Test;
	}
}
