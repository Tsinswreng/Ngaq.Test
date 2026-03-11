using Ngaq.Local.Test;
using Tsinswreng.CsTest;

namespace Ngaq.Windows.Test;

public class WindowsTestMgr:DiEtTestMgr{
	public static WindowsTestMgr Inst = new();
	public override ITestNode RegisterTestsInto(ITestNode? Test){
		Test = this.TestNode;
		this.RegisterSubMgr(LocalTestMgr.Inst);
		return Test;
	}

}
