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
			,[nameof(ITblSetter<PoKv>.Idx)]
			,nameof(TestTblSetter)
		);
		var R = register.Register;

		R("Idx_NullOpt_SingleColumn_AddsOneSql", async(o)=>{
			var s = MkTblSetter();
			s.Tbl.OuterAdditionalSqls.Clear();
			var t = s.Tbl;

			var returned = s.Idx(null, [nameof(PoKv.KStr)]);
			if(!object.ReferenceEquals(returned, t)){
				throw new Exception("Idx should return same table instance");
			}
			if(t.OuterAdditionalSqls.Count != 1){
				throw new Exception($"Expected 1 sql, got {t.OuterAdditionalSqls.Count}");
			}

			var sql = NormLf(t.OuterAdditionalSqls[0]);
			if(!sql.Contains("CREATE INDEX")){
				throw new Exception("Expected non-unique index SQL");
			}
			if(sql.Contains("UNIQUE INDEX")){
				throw new Exception("Null opt should not create unique index");
			}
			if(!sql.Contains(t.QtCol(nameof(PoKv.KStr)))){
				throw new Exception("Index SQL should contain KStr column");
			}
			return NIL;
		});

		R("Idx_UniqueAndWhere_OptionsApplied", async(o)=>{
			var s = MkTblSetter();
			s.Tbl.OuterAdditionalSqls.Clear();
			var t = s.Tbl;
			var where = t.SqlIsNonDel() + " AND " + t.QtCol(nameof(PoKv.KType)) + " = 'Str'";

			s.Idx(
				new OptMkIdx{
					Unique = true,
					Where = where
				},
				[nameof(PoKv.Owner), nameof(PoKv.KStr)]
			);

			if(t.OuterAdditionalSqls.Count != 1){
				throw new Exception($"Expected 1 sql, got {t.OuterAdditionalSqls.Count}");
			}
			var sql = NormLf(t.OuterAdditionalSqls[0]);
			if(!sql.Contains("CREATE UNIQUE INDEX")){
				throw new Exception("Expected unique index SQL");
			}
			if(!sql.Contains("\nWHERE " + where)){
				throw new Exception("WHERE condition from option was not applied");
			}
			return NIL;
		});

		R("Idx_MultiColumnSets_AppendsMultipleSqlInOrder", async(o)=>{
			var s = MkTblSetter();
			s.Tbl.OuterAdditionalSqls.Clear();
			var t = s.Tbl;

			s.Idx(
				null,
				[nameof(PoKv.Owner), nameof(PoKv.KStr)],
				[nameof(PoKv.KI64)]
			);
			if(t.OuterAdditionalSqls.Count != 2){
				throw new Exception($"Expected 2 sql, got {t.OuterAdditionalSqls.Count}");
			}

			var sql0 = NormLf(t.OuterAdditionalSqls[0]);
			var sql1 = NormLf(t.OuterAdditionalSqls[1]);
			if(!sql0.Contains(t.QtCol(nameof(PoKv.Owner))) || !sql0.Contains(t.QtCol(nameof(PoKv.KStr)))){
				throw new Exception("First SQL should index Owner+KStr");
			}
			if(!sql1.Contains(t.QtCol(nameof(PoKv.KI64)))){
				throw new Exception("Second SQL should index KI64");
			}
			return NIL;
		});

		R("Idx_CustomFnSetIdx_UsesReturnedSqls", async(o)=>{
			var s = MkTblSetter();
			s.Tbl.OuterAdditionalSqls.Clear();

			s.FnSetIdx = (opt, tbl, cols) => {
				return ["-- idx custom 1", "-- idx custom 2"];
			};
			s.Idx(null, [nameof(PoKv.KStr)]);

			if(s.Tbl.OuterAdditionalSqls.Count != 2){
				throw new Exception($"Expected 2 custom sql, got {s.Tbl.OuterAdditionalSqls.Count}");
			}
			if(s.Tbl.OuterAdditionalSqls[0] != "-- idx custom 1" || s.Tbl.OuterAdditionalSqls[1] != "-- idx custom 2"){
				throw new Exception("Custom FnSetIdx output order mismatch");
			}
			return NIL;
		});

		R("Idx_EmptyColSets_NoSqlAdded", async(o)=>{
			var s = MkTblSetter();
			s.Tbl.OuterAdditionalSqls.Clear();

			s.Idx(null);
			if(s.Tbl.OuterAdditionalSqls.Count != 0){
				throw new Exception($"Expected 0 sql for empty col sets, got {s.Tbl.OuterAdditionalSqls.Count}");
			}
			return NIL;
		});
	}
}
