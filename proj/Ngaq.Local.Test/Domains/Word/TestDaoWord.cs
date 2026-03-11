using Ngaq.Core.Word.Svc;
using Ngaq.Local.Word.Dao;
using Tsinswreng.CsTest;

namespace Ngaq.Local.Test.Domains.Word;


public class TestDaoWord:I_RegisterTests{
	DaoWord DaoWord;
	public TestDaoWord(DaoWord DaoWord){
		this.DaoWord = DaoWord;
	}
	public ITestNode RegisterTests(ITestNode? Test){
		Test??=new TestNode();
		var R = Test.MkFnRegisterTest(typeof(TestDaoWord), typeof(DaoWord));
		return Test;
	}

}
