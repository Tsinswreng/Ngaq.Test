using Tsinswreng.Srefl.Test.IDictSerializer;
using Tsinswreng.CsTreeTest;
using Ngaq.Core.Test.Tools.Json.IDictJsonSerializer;
using Ngaq.Core.Test.Tools.Json.AppJsonSerializerTests;
using Ngaq.Core.Test.Sync.IPacker;

namespace Ngaq.Core.Test;

public class NgaqCoreTestMgr:DiEtTestMgr{
	public static NgaqCoreTestMgr Inst = new();
	public override ITestNode RegisterTestsInto(ITestNode? Test){
		Test = this.TestNode;
		this.RegisterTester<TestIDictSerializer>();
		this.RegisterTester<TestAppJsonSerializer>();
		this.RegisterTester<TestIDictJsonSerializer>();
		this.RegisterTester<TestIPacker>();
		return Test;
	}

}
