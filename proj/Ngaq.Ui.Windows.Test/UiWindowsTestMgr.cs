using Ngaq.Core.Test;
using Ngaq.Backend.Test;
using Ngaq.Ui.Test;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Ui.Windows.Test;

public class UiWindowsTestMgr:DiEtTestMgr{
	public static UiWindowsTestMgr Inst = new();

	public override ITestNode RegisterTestsInto(ITestNode? Test){
		Test = this.TestNode;
		this.RegisterSubMgr(LocalTestMgr.Inst);
		this.RegisterSubMgr(CoreTestMgr.Inst);
		this.RegisterSubMgr(UiTestMgr.Inst);
		return Test;
	}
}
