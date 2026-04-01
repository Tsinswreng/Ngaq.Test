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
			,[nameof(ITblSetter<PoKv>.IdxExpr)]
			,nameof(TestTblSetter)
		);
		var R = register.Register;

		R("IdxExpr_SingleMember_AddsOneSql", async(o)=>{
			var s = MkTblSetter();
			s.Tbl.OuterAdditionalSqls.Clear();
			var t = s.Tbl;

			s.IdxExpr(null, x => x.KStr);
			if(t.OuterAdditionalSqls.Count != 1){
				throw new Exception($"Expected 1 sql, got {t.OuterAdditionalSqls.Count}");
			}
			var sql = NormLf(t.OuterAdditionalSqls[0]);
			if(!sql.Contains(t.QtCol(nameof(PoKv.KStr)))){
				throw new Exception("Index SQL should contain KStr");
			}
			return NIL;
		});

		R("IdxExpr_CompositeExpression_BuildsCompositeIndex", async(o)=>{
			var s = MkTblSetter();
			s.Tbl.OuterAdditionalSqls.Clear();
			var t = s.Tbl;

			s.IdxExpr(null, x => new {x.Owner, x.KI64});
			if(t.OuterAdditionalSqls.Count != 1){
				throw new Exception($"Expected 1 sql, got {t.OuterAdditionalSqls.Count}");
			}
			var sql = NormLf(t.OuterAdditionalSqls[0]);
			if(!sql.Contains(t.QtCol(nameof(PoKv.Owner))) || !sql.Contains(t.QtCol(nameof(PoKv.KI64)))){
				throw new Exception("Composite expression should include Owner and KI64");
			}
			return NIL;
		});

		R("IdxExpr_MultiExpressions_AppendsMultipleSql", async(o)=>{
			var s = MkTblSetter();
			s.Tbl.OuterAdditionalSqls.Clear();
			var t = s.Tbl;

			s.IdxExpr(null, x => x.KStr, x => x.KI64);
			if(t.OuterAdditionalSqls.Count != 2){
				throw new Exception($"Expected 2 sql, got {t.OuterAdditionalSqls.Count}");
			}
			if(!t.OuterAdditionalSqls[0].Contains(t.QtCol(nameof(PoKv.KStr)))){
				throw new Exception("First SQL should index KStr");
			}
			if(!t.OuterAdditionalSqls[1].Contains(t.QtCol(nameof(PoKv.KI64)))){
				throw new Exception("Second SQL should index KI64");
			}
			return NIL;
		});

		R("IdxExpr_UniqueAndWhere_OptionsApplied", async(o)=>{
			var s = MkTblSetter();
			s.Tbl.OuterAdditionalSqls.Clear();
			var t = s.Tbl;
			var where = t.SqlIsNonDel();

			s.IdxExpr(
				new OptMkIdx{
					Unique = true,
					Where = where
				},
				x => new {x.Owner, x.KStr}
			);

			if(t.OuterAdditionalSqls.Count != 1){
				throw new Exception($"Expected 1 sql, got {t.OuterAdditionalSqls.Count}");
			}
			var sql = NormLf(t.OuterAdditionalSqls[0]);
			if(!sql.Contains("CREATE UNIQUE INDEX")){
				throw new Exception("Expected unique index");
			}
			if(!sql.Contains("\nWHERE " + where)){
				throw new Exception("Expected WHERE from options");
			}
			return NIL;
		});

		R("IdxExpr_EmptyExpressions_NoSqlAdded", async(o)=>{
			var s = MkTblSetter();
			s.Tbl.OuterAdditionalSqls.Clear();

			s.IdxExpr(null);
			if(s.Tbl.OuterAdditionalSqls.Count != 0){
				throw new Exception($"Expected 0 sql for empty expressions, got {s.Tbl.OuterAdditionalSqls.Count}");
			}
			return NIL;
		});
	}
}
