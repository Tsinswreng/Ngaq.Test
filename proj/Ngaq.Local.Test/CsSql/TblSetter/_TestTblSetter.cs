using Ngaq.Core.Shared.Kv.Models;
using Ngaq.Core.Infra;
using Tsinswreng.CsSql;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Local.Test.CsSql.TblSetter;

public partial class TestTblSetter : ITester {

	public ITestNode RegisterTestsInto(ITestNode? Test) {
		Test ??= new TestNode();
		Test.Ordered = true;

		RegisterIdxApis(Test);
		RegisterColApis(Test);
		return Test;
	}

	static ITblSetter<PoKv> MkTblSetter(str TblName = "TblSetterSpec") {
		return Table.FnSetTbl<PoKv>(CoreDictMapper.Inst)(TblName);
	}

	static str NormLf(str s) {
		return s.Replace("\r\n", "\n");
	}
}
