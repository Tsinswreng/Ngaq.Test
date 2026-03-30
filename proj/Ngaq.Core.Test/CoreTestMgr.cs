using Ngaq.Core.Test.Tools.Json.IDictJsonSerializer;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Core.Test;

public class CoreTestMgr:DiEtTestMgr{
	public static CoreTestMgr Inst = new();
	public override ITestNode RegisterTestsInto(ITestNode? Node){
		Node = this.TestNode;
		this.RegisterTester<TestIDictJsonSerializer>();
		return Node;
	}
}
