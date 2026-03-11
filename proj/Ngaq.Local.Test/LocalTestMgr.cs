using Ngaq.Local.Test.Domains.Word;
using Tsinswreng.CsTest;

namespace Ngaq.Local.Test;


public class LocalTestMgr:DiEtTestMgr{
	public static LocalTestMgr Inst = new();
	public override ITestNode RegisterTestsInto(ITestNode? Test){
		Test = this.TestNode;
		this.RegisterTesterType<TestISvcWord>();
		this.RegisterTesterType<TestDaoWord>();
		return Test;
	}
}
