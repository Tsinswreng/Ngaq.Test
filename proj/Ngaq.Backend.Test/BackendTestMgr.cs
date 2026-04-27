using Ngaq.Backend.Test.Domains.Word;
using Ngaq.Backend.Test.Domains.StudyPlan;
using Ngaq.Backend.Test.Domains.Kv;
using Ngaq.Backend.Test.Frontend.User.Svc;
using Ngaq.Backend.Test.CsSql.Repo;
using Ngaq.Backend.Test.CsSql.TblSetter;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Backend.Test;


public class LocalTestMgr:DiEtTestMgr{
	public static LocalTestMgr Inst = new();
	public override ITestNode RegisterTestsInto(ITestNode? Test){
		Test = this.TestNode;
		Test.Ordered = true;
		Test.IsParallelRecursive = true;  // Recursively disable parallel execution for all db-accessing tests
		this.RegisterTester<TestISvcStudyPlan>();
		this.RegisterTester<TestISvcWord>();
		this.RegisterTester<TestISvcWordV2>();
		this.RegisterTester<TestISvcUserLang>();
		this.RegisterTester<TestISvcNormLang>();
		this.RegisterTester<TestISvcNormLangToUserLang>();
		this.RegisterTester<TestISvcKv>();
		this.RegisterTester<TestSvcTokenStorage>();
		this.RegisterTester<TestDaoWord>();
		this.RegisterTester<TestRepo>();
		this.RegisterTester<TestTblSetter>();
		return Test;
	}
}
