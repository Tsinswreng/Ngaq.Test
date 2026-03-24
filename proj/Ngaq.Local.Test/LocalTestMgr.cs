using Ngaq.Local.Test.Domains.Word;
using Ngaq.Local.Test.Domains.StudyPlan;
using Ngaq.Local.Test.CsSql.Repo;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Local.Test;


public class LocalTestMgr:DiEtTestMgr{
	public static LocalTestMgr Inst = new();
	public override ITestNode RegisterTestsInto(ITestNode? Test){
		Test = this.TestNode;
		this.RegisterTester<TestISvcStudyPlan>();
		this.RegisterTester<TestISvcWord>();
		this.RegisterTester<TestISvcWordV2>();
		this.RegisterTester<TestDaoWord>();
		this.RegisterTester<TestRepo>();
		return Test;
	}
}
