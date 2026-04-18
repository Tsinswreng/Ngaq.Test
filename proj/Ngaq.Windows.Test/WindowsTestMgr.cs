using Ngaq.Core.Test;
using Ngaq.Backend.Test;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Windows.Test;

public class WindowsTestMgr:DiEtTestMgr{
	public static WindowsTestMgr Inst = new();
	public override ITestNode RegisterTestsInto(ITestNode? Test){
		Test = this.TestNode;
		this.RegisterSubMgr(LocalTestMgr.Inst);
		this.RegisterSubMgr(CoreTestMgr.Inst);
		return Test;
	}

}
