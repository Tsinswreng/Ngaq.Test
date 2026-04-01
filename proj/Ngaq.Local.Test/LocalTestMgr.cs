using Ngaq.Local.Test.Domains.Word;
using Ngaq.Local.Test.Domains.StudyPlan;
using Ngaq.Local.Test.CsSql.Repo;
using Ngaq.Local.Test.CsSql.TblSetter;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Local.Test;


public class LocalTestMgr:DiEtTestMgr{
	public static LocalTestMgr Inst = new();
	public override ITestNode RegisterTestsInto(ITestNode? Test){
		Test = this.TestNode;
		Test.Ordered = true;
		Test.IsParallelRecursive = true;  // Recursively disable parallel execution for all db-accessing tests
		this.RegisterTester<TestISvcStudyPlan>();
		this.RegisterTester<TestISvcWord>();
		this.RegisterTester<TestISvcWordV2>();
		this.RegisterTester<TestDaoWord>();
		this.RegisterTester<TestRepo>();
		this.RegisterTester<TestTblSetter>();
		return Test;
	}
}
