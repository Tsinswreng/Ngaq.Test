using Ngaq.Core.Shared.Kv.Models;
using Tsinswreng.CsSql;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Local.Test.CsSql.TblSetter;

public partial class TestTblSetter : ITester {
	readonly ITblMgr TblMgr;

	public TestTblSetter(
		ITblMgr TblMgr
	) {
		this.TblMgr = TblMgr;
	}

	public ITestNode RegisterTestsInto(ITestNode? Test) {
		Test ??= new TestNode();
		Test.Ordered = true;

		RegisterIdxApis(Test);
		RegisterColApis(Test);
		return Test;
	}

	ITblSetter<PoKv> MkTblSetter() {
		return new TblSetter<PoKv>(TblMgr.GetTbl<PoKv>());
	}

	static str NormLf(str s) {
		return s.Replace("\r\n", "\n");
	}
}
