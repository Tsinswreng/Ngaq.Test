using Ngaq.Core.Word.Svc;
using Ngaq.Backend.Word.Dao;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Backend.Test.Domains.Word;


public class TestDaoWord:ITester{
	DaoWord DaoWord;
	public TestDaoWord(DaoWord DaoWord){
		this.DaoWord = DaoWord;
	}
	public ITestNode RegisterTestsInto(ITestNode? Test){
		Test??=new TestNode();
		var R = Test.MkFnRegisterTest(typeof(TestDaoWord), typeof(DaoWord));
		R("2026_0312_095035", async(o)=>{
			return NIL;
		});
		return Test;
	}

}
