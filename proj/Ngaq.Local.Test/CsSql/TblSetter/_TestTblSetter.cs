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

		RegisterIdx(Test);
		RegisterIdxExpr(Test);
		return Test;
	}

	ITblSetter<PoKv> MkTblSetter() {
		return new TblSetter<PoKv>(TblMgr.GetTbl<PoKv>());
	}

	static str NormLf(str s) {
		return s.Replace("\r\n", "\n");
	}

	static void AssertSqlListExact(
		IList<str> Actual
		,IList<str> Expected
		,str CaseName
	) {
		if(Actual.Count != Expected.Count){
			throw new Exception($"{CaseName}: expected {Expected.Count} SQL rows, got {Actual.Count}");
		}
		for(var i = 0; i < Expected.Count; i++){
			var a = NormLf(Actual[i]);
			var e = NormLf(Expected[i]);
			if(a != e){
				throw new Exception(
					$"{CaseName}: SQL[{i}] mismatch.\nExpected:\n{e}\nActual:\n{a}"
				);
			}
		}
	}

	static void AssertFnSetIdxPointsToDefault(
		ITblSetter<PoKv> Setter
		,str CaseName
	) {
		if(Setter is not TblSetter<PoKv> impl){
			throw new Exception($"{CaseName}: expected concrete TblSetter<PoKv>.");
		}
		var d = Setter.FnSetIdx;
		if(d.Method.Name != nameof(TblSetter<PoKv>.DefaultFnSetIdx)){
			throw new Exception($"{CaseName}: FnSetIdx should point to DefaultFnSetIdx, got {d.Method.Name}.");
		}
		if(!object.ReferenceEquals(d.Target, impl)){
			throw new Exception($"{CaseName}: FnSetIdx target should be current TblSetter instance.");
		}
	}
}
