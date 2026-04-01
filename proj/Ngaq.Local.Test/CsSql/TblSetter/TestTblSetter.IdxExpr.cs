using Ngaq.Core.Shared.Kv.Models;
using Ngaq.Local.Db.TswG;
using Tsinswreng.CsSql;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Local.Test.CsSql.TblSetter;

public partial class TestTblSetter {

	void RegisterIdxExpr(ITestNode Node) {
		var register = Node.MkTestFnRegister(
			typeof(TestTblSetter)
			,[typeof(ITblSetter<PoKv>)]
			,[nameof(ITblSetter<PoKv>.FnSetIdx), nameof(ITblSetter<PoKv>.IdxExpr)]
			,nameof(TestTblSetter)
		);
		var R = register.Register;

		R("IdxExpr_SingleMember_AppendsExactFnSetIdxSql", async(o)=>{
			var s = MkTblSetter();
			AssertFnSetIdxPointsToDefault(s, "IdxExpr_SingleMember_AppendsExactFnSetIdxSql");
			var t = s.Tbl;
			t.OuterAdditionalSqls.Clear();

			s.IdxExpr(null, x => x.KStr);
			var expected = new List<str>{
$"""
CREATE INDEX "Idx_Kv_KStr"
ON "Kv" ("KStr")
"""
			};

			AssertSqlListExact(t.OuterAdditionalSqls, expected, "IdxExpr_SingleMember_AppendsExactFnSetIdxSql");
			return NIL;
		});

		R("IdxExpr_CompositeExpression_AppendsExactFnSetIdxSql", async(o)=>{
			var s = MkTblSetter();
			AssertFnSetIdxPointsToDefault(s, "IdxExpr_CompositeExpression_AppendsExactFnSetIdxSql");
			var t = s.Tbl;
			t.OuterAdditionalSqls.Clear();

			s.IdxExpr(null, x => new {x.Owner, x.KI64});
			var expected = new List<str>{
$"""
CREATE INDEX "Idx_Kv_Owner_KI64"
ON "Kv" ("Owner", "KI64")
"""
			};

			AssertSqlListExact(t.OuterAdditionalSqls, expected, "IdxExpr_CompositeExpression_AppendsExactFnSetIdxSql");
			return NIL;
		});

		R("IdxExpr_UniqueWhere_MultiExpressions_ExactSqlList", async(o)=>{
			var s = MkTblSetter();
			AssertFnSetIdxPointsToDefault(s, "IdxExpr_UniqueWhere_MultiExpressions_ExactSqlList");
			var t = s.Tbl;
			t.OuterAdditionalSqls.Clear();

			s.IdxExpr(
				new OptMkIdx{
					Unique = true,
					Where = t.SqlIsNonDel()
				},
				x => x.KStr,
				x => new {x.Owner, x.KI64}
			);
			var expected = new List<str>{
$"""
CREATE UNIQUE INDEX "Ux_Kv_KStr"
ON "Kv" ("KStr")
WHERE ("DelAt" = 0)
""",
$"""
CREATE UNIQUE INDEX "Ux_Kv_Owner_KI64"
ON "Kv" ("Owner", "KI64")
WHERE ("DelAt" = 0)
"""
			};

			AssertSqlListExact(t.OuterAdditionalSqls, expected, "IdxExpr_UniqueWhere_MultiExpressions_ExactSqlList");
			return NIL;
		});

		R("IdxExpr_CustomFnSetIdx_ReceivesParsedColsInOrder", async(o)=>{
			var s = MkTblSetter();
			var t = s.Tbl;
			t.OuterAdditionalSqls.Clear();
			List<List<str>> captured = [];
			var expected = new List<str>{"R1", "R2"};

			s.FnSetIdx = (opt, tbl, cols) => {
				foreach(var colSet in cols){
					captured.Add(colSet.ToList());
				}
				return expected;
			};

			s.IdxExpr(
				null,
				x => x.KStr,
				x => new {x.Owner, x.KI64}
			);

			AssertSqlListExact(t.OuterAdditionalSqls, expected, "IdxExpr_CustomFnSetIdx_ReceivesParsedColsInOrder");
			if(captured.Count != 2){
				throw new Exception($"Expected captured 2 col sets, got {captured.Count}");
			}
			if(captured[0].Count != 1 || captured[0][0] != nameof(PoKv.KStr)){
				throw new Exception("First expression columns mismatch");
			}
			if(captured[1].Count != 2 || captured[1][0] != nameof(PoKv.Owner) || captured[1][1] != nameof(PoKv.KI64)){
				throw new Exception("Second expression columns mismatch");
			}
			return NIL;
		});

		R("IdxExpr_EmptyExpressions_AppendsNoSql", async(o)=>{
			var s = MkTblSetter();
			AssertFnSetIdxPointsToDefault(s, "IdxExpr_EmptyExpressions_AppendsNoSql");
			var t = s.Tbl;
			t.OuterAdditionalSqls.Clear();

			s.IdxExpr(null);
			AssertSqlListExact(t.OuterAdditionalSqls, [], "IdxExpr_EmptyExpressions_AppendsNoSql");
			return NIL;
		});
	}
}
