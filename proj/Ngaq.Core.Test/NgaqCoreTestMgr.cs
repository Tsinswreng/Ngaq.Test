using Tsinswreng.Srefl.Test.IDictSerializer;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Core.Test;

public class NgaqCoreTestMgr:DiEtTestMgr{
	public static NgaqCoreTestMgr Inst = new();
	public override ITestNode RegisterTestsInto(ITestNode? Test){
		Test = this.TestNode;
		this.RegisterTester<TestIDictSerializer>();
		return Test;
	}

}
