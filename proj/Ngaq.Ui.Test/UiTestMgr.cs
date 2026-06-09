using Ngaq.Ui.Test.Views.Word.Learn;
using Ngaq.Ui.Test.Views.Word.WordEditV2;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Ui.Test;

public class UiTestMgr : DiEtTestMgr {
	public static UiTestMgr Inst = new();
	public override ITestNode RegisterTestsInto(ITestNode? Test) {
		Test = this.TestNode;
		Test.Ordered = true;
		Test.IsParallelRecursive = false;
		this.RegisterTester<TestIViewLearnWord>();
		this.RegisterTester<TestIViewWordEditV2>();
		return Test;
	}
}
