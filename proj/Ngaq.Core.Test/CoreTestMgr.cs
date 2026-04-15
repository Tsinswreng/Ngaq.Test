using Ngaq.Core.Test.Tools.Json.IDictJsonSerializer;
using Ngaq.Core.Test.Sync.IPacker;
using Tsinswreng.CsTreeTest;
using Ngaq.Core.Test.Lib.MsgFmt;

namespace Ngaq.Core.Test;

public class CoreTestMgr:DiEtTestMgr{
	public static CoreTestMgr Inst = new();
	public override ITestNode RegisterTestsInto(ITestNode? Node){
		Node = this.TestNode;
		this.RegisterTester<TestIDictJsonSerializer>();
		this.RegisterTester<TestIPacker>();
		this.RegisterTester<TestMsgFmt>();
		return Node;
	}
}
