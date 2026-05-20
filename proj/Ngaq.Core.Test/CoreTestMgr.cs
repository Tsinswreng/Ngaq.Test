using Ngaq.Core.Test.Tools.Json.IDictJsonSerializer;
using Ngaq.Core.Test.Tools.Json.AppJsonSerializerTests;
using Ngaq.Core.Test.Sync.IPacker;
using Tsinswreng.CsTreeTest;
using Ngaq.Core.Test.Lib.MsgFmt;
using Ngaq.Core.Test.Lang.ExprRefl;

namespace Ngaq.Core.Test;

public class CoreTestMgr:DiEtTestMgr{
	public static CoreTestMgr Inst = new();
	public override ITestNode RegisterTestsInto(ITestNode? Node){
		Node = this.TestNode;
		this.RegisterTester<TestAppJsonSerializer>();
		this.RegisterTester<TestIDictJsonSerializer>();
		this.RegisterTester<TestIPacker>();
		this.RegisterTester<TestMsgFmt>();
		this.RegisterTester<TestExprRefl>();
		return Node;
	}
}
