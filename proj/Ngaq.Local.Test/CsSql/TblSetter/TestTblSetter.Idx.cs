using Ngaq.Core.Shared.Kv.Models;
using Ngaq.Local.Db.TswG;
using Tsinswreng.CsSql;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Local.Test.CsSql.TblSetter;

public partial class TestTblSetter {

	void RegisterIdx(ITestNode Node) {
		var register = Node.MkTestFnRegister(
			typeof(TestTblSetter)
			,[typeof(ITblSetter<PoKv>)]
			,[nameof(ITblSetter<PoKv>.FnSetIdx), nameof(ITblSetter<PoKv>.Idx)]
			,nameof(TestTblSetter)
		);
		var R = register.Register;

		R("FnSetIdx_Default_NullOpt_MultiColSets_ExactSqlList", async(o)=>{
			var s = MkTblSetter();
			AssertFnSetIdxPointsToDefault(s, "FnSetIdx_Default_NullOpt_MultiColSets_ExactSqlList");
			var t = s.Tbl;

			var actual = s.FnSetIdx(
				null, t,
				[nameof(PoKv.Owner), nameof(PoKv.KStr)],
				[nameof(PoKv.KI64)]
			);

			var expected = new List<str>{
$"""
CREATE INDEX "Idx_Kv_Owner_KStr"
ON "Kv" ("Owner", "KStr")
""",
$"""
CREATE INDEX "Idx_Kv_KI64"
ON "Kv" ("KI64")
"""
			};
			AssertSqlListExact(actual, expected, "FnSetIdx_Default_NullOpt_MultiColSets_ExactSqlList");
			return NIL;
		});

		R("FnSetIdx_Default_UniqueWhere_ExactSqlList", async(o)=>{
			var s = MkTblSetter();
			AssertFnSetIdxPointsToDefault(s, "FnSetIdx_Default_UniqueWhere_ExactSqlList");
			var t = s.Tbl;
			var where = t.SqlIsNonDel() + " AND " + t.QtCol(nameof(PoKv.KType)) + " = 'Str'";

			var actual = s.FnSetIdx(
				new OptMkIdx{
					Unique = true,
					Where = where
				},
				t,
				[nameof(PoKv.Owner), nameof(PoKv.KStr)]
			);

			var expected = new List<str>{
$"""
CREATE UNIQUE INDEX "Ux_Kv_Owner_KStr"
ON "Kv" ("Owner", "KStr")
WHERE ("DelAt" = 0) AND "KType" = 'Str'
"""
			};
			AssertSqlListExact(actual, expected, "FnSetIdx_Default_UniqueWhere_ExactSqlList");
			return NIL;
		});

		R("FnSetIdx_Default_EmptyColSets_ReturnsEmptyList", async(o)=>{
			var s = MkTblSetter();
			AssertFnSetIdxPointsToDefault(s, "FnSetIdx_Default_EmptyColSets_ReturnsEmptyList");
			var t = s.Tbl;

			var actual = s.FnSetIdx(null, t);
			AssertSqlListExact(actual, [], "FnSetIdx_Default_EmptyColSets_ReturnsEmptyList");
			return NIL;
		});

		R("Idx_Default_AppendsExactlyFnSetIdxOutput", async(o)=>{
			var s = MkTblSetter();
			AssertFnSetIdxPointsToDefault(s, "Idx_Default_AppendsExactlyFnSetIdxOutput");
			s.Tbl.OuterAdditionalSqls.Clear();
			var opt = new OptMkIdx{ Unique = true, Where = s.Tbl.SqlIsNonDel() };
			IEnumerable<str>[] cols = [[nameof(PoKv.Owner), nameof(PoKv.KStr)], [nameof(PoKv.KI64)]];
			s.Idx(opt, cols);
			var expected = new List<str>{
$"""
CREATE UNIQUE INDEX "Ux_Kv_Owner_KStr"
ON "Kv" ("Owner", "KStr")
WHERE ("DelAt" = 0)
""",
$"""
CREATE UNIQUE INDEX "Ux_Kv_KI64"
ON "Kv" ("KI64")
WHERE ("DelAt" = 0)
"""
			};

			AssertSqlListExact(s.Tbl.OuterAdditionalSqls, expected, "Idx_Default_AppendsExactlyFnSetIdxOutput");
			return NIL;
		});

		R("Idx_CustomFnSetIdx_AppendsExactList_ItemByItem", async(o)=>{
			var s = MkTblSetter();
			s.Tbl.OuterAdditionalSqls.Clear();
			var expected = new List<str>{
				"SQL_A",
				"SQL_B\nline2",
				"SQL_C"
			};

			s.FnSetIdx = (opt, tbl, cols) => expected;
			s.Idx(null, [nameof(PoKv.KStr)]);

			AssertSqlListExact(
				s.Tbl.OuterAdditionalSqls,
				expected,
				"Idx_CustomFnSetIdx_AppendsExactList_ItemByItem"
			);
			return NIL;
		});
	}
}
