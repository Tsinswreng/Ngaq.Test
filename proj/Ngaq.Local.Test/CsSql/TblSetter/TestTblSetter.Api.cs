using Ngaq.Core.Shared.Kv.Models;
using Ngaq.Local.Db.TswG;
using Tsinswreng.CsSql;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Local.Test.CsSql.TblSetter;

public partial class TestTblSetter {

	void RegisterIdxApis(ITestNode Node) {
		var register = Node.MkTestFnRegister(
			typeof(TestTblSetter)
			,[typeof(ITblSetter<PoKv>)]
			,[]
			,nameof(TestTblSetter)
		);
		var R = register.Register;

		register.TesteeFnNames = [nameof(TblSetter<PoKv>.MkIndexSqlByCodeCols)];
		R("TblSetter_MkIndexSqlByCodeCols_UniqueAndWhere", async(o)=>{
			var s = MkTblSetter("TblSetterMkIndex");
			var t = s.Tbl;
			var idxName = "Ux_CustomOwnerKStr";

			var sql = s switch {
				TblSetter<PoKv> impl => impl.MkIndexSqlByCodeCols(
					idxName
					,[nameof(PoKv.Owner), nameof(PoKv.KStr)]
					,IsUnique: true
					,WhereAnds: ["1 = 1", "  ", t.QtCol(nameof(PoKv.KType)) + " = 'Str'"]
				),
				_ => throw new Exception("Expected concrete TblSetter implementation")
			};
			sql = NormLf(sql);

			var expectedBody =
$"""
CREATE UNIQUE INDEX {t.Qt(idxName)}
ON {t.Qt(t.DbTblName)} ({t.QtCol(nameof(PoKv.Owner))}, {t.QtCol(nameof(PoKv.KStr))})
WHERE 1 = 1
AND {t.QtCol(nameof(PoKv.KType))} = 'Str'
""";
			expectedBody = NormLf(expectedBody);

			if(sql != expectedBody){
				throw new Exception($"SQL mismatch.\nExpected:\n{expectedBody}\nActual:\n{sql}");
			}
			return NIL;
		});

		register.TesteeFnNames = [nameof(ITblSetter<PoKv>.Idx)];
		R("TblSetter_Idx_DefaultFn_AppendsSql_AndReturnsTable", async(o)=>{
			var s = MkTblSetter("TblSetterIdxDefault");
			s.Tbl.OuterAdditionalSqls.Clear();
			var t = s.Tbl;

			var returned = s.Idx(
				new OptMkIdx{
					Unique = true
					,Where = t.SqlIsNonDel()
				},
				[nameof(PoKv.Owner), nameof(PoKv.KStr)],
				[nameof(PoKv.KI64)]
			);

			if(!object.ReferenceEquals(returned, t)){
				throw new Exception("Idx should return the same table instance");
			}
			if(t.OuterAdditionalSqls.Count != 2){
				throw new Exception($"Expected 2 SQL statements, got {t.OuterAdditionalSqls.Count}");
			}

			var sql0 = NormLf(t.OuterAdditionalSqls[0]);
			var sql1 = NormLf(t.OuterAdditionalSqls[1]);
			var expectedWhere = "\nWHERE " + t.SqlIsNonDel();
			if(!sql0.Contains("CREATE UNIQUE INDEX")){
				throw new Exception("First SQL should create unique index");
			}
			if(!sql0.Contains(expectedWhere)){
				throw new Exception("First SQL should contain WHERE clause from options");
			}
			if(!sql0.Contains(t.QtCol(nameof(PoKv.Owner))) || !sql0.Contains(t.QtCol(nameof(PoKv.KStr)))){
				throw new Exception("First SQL should include both Owner and KStr columns");
			}
			if(!sql1.Contains(t.QtCol(nameof(PoKv.KI64)))){
				throw new Exception("Second SQL should include KI64 column");
			}
			return NIL;
		});

		register.TesteeFnNames = [nameof(ITblSetter<PoKv>.IdxExpr)];
		R("TblSetter_IdxExpr_CompositeExpression_BuildsCompositeIndex", async(o)=>{
			var s = MkTblSetter("TblSetterIdxExpr");
			s.Tbl.OuterAdditionalSqls.Clear();

			s.IdxExpr(null, x => x.KStr, x => new {x.Owner, x.KI64});
			if(s.Tbl.OuterAdditionalSqls.Count != 2){
				throw new Exception($"Expected 2 SQL statements, got {s.Tbl.OuterAdditionalSqls.Count}");
			}

			var sqlComposite = NormLf(s.Tbl.OuterAdditionalSqls[1]);
			if(!sqlComposite.Contains(s.Tbl.QtCol(nameof(PoKv.Owner)))
				|| !sqlComposite.Contains(s.Tbl.QtCol(nameof(PoKv.KI64)))){
				throw new Exception("Composite expression index should include Owner and KI64");
			}
			return NIL;
		});

		register.TesteeFnNames = [nameof(ITblSetter<PoKv>.FnSetIdx), nameof(ITblSetter<PoKv>.Idx)];
		R("TblSetter_Idx_UsesCustomFnSetIdx_Output", async(o)=>{
			var s = MkTblSetter("TblSetterCustomFn");
			s.Tbl.OuterAdditionalSqls.Clear();

			s.FnSetIdx = (opt, tbl, cols) => {
				return ["-- custom index 1", "-- custom index 2"];
			};

			s.Idx(null, [nameof(PoKv.KStr)]);
			if(s.Tbl.OuterAdditionalSqls.Count != 2){
				throw new Exception($"Expected 2 custom SQL statements, got {s.Tbl.OuterAdditionalSqls.Count}");
			}
			if(s.Tbl.OuterAdditionalSqls[0] != "-- custom index 1" || s.Tbl.OuterAdditionalSqls[1] != "-- custom index 2"){
				throw new Exception("Idx should append custom SQL results in order");
			}
			return NIL;
		});

		register.TesteeFnNames = [nameof(TblSetter<PoKv>.AddIndexByCodeCols)];
		R("TblSetter_AddIndexByCodeCols_AppendsSql", async(o)=>{
			var s = MkTblSetter("TblSetterAddIndex");
			s.Tbl.OuterAdditionalSqls.Clear();
			var idxName = "Idx_TestAdd";
			var expected = s switch {
				TblSetter<PoKv> impl => NormLf(impl.MkIndexSqlByCodeCols(idxName, [nameof(PoKv.KStr)])),
				_ => throw new Exception("Expected concrete TblSetter implementation")
			};

			if(s is not TblSetter<PoKv> implAdd){
				throw new Exception("Expected concrete TblSetter implementation");
			}
			implAdd.AddIndexByCodeCols(idxName, [nameof(PoKv.KStr)]);
			if(s.Tbl.OuterAdditionalSqls.Count != 1){
				throw new Exception($"Expected 1 SQL statement, got {s.Tbl.OuterAdditionalSqls.Count}");
			}
			var actual = NormLf(s.Tbl.OuterAdditionalSqls[0]);
			if(actual != expected){
				throw new Exception($"AddIndexByCodeCols SQL mismatch.\nExpected:\n{expected}\nActual:\n{actual}");
			}
			return NIL;
		});
	}

	void RegisterColApis(ITestNode Node) {
		var register = Node.MkTestFnRegister(
			typeof(TestTblSetter)
			,[typeof(ITblSetter<PoKv>)]
			,[]
			,nameof(TestTblSetter)
		);
		var R = register.Register;

		register.TesteeFnNames = ["Col"];
		R("TblSetter_Col_ByName_And_ByExpression_PointToSameColumn", async(o)=>{
			var s = MkTblSetter("TblSetterColApi");
			var byName = s.Col(nameof(PoKv.KStr));
			var byExpr = s.Col(x => x.KStr);

			if(!object.ReferenceEquals(byName.Table, s.Tbl) || !object.ReferenceEquals(byExpr.Table, s.Tbl)){
				throw new Exception("Col builder should hold original table reference");
			}
			if(byName.Column.DbName != nameof(PoKv.KStr)){
				throw new Exception($"Expected DbName {nameof(PoKv.KStr)} but got {byName.Column.DbName}");
			}
			if(!object.ReferenceEquals(byName.Column, byExpr.Column)){
				throw new Exception("Col by name and by expression should resolve to same column object");
			}
			return NIL;
		});
	}
}
