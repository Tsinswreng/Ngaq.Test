namespace Ngaq.Test.CsSqlHelper.Integration.Repo;

using Ngaq.Core.Model.Po.Kv;
using Ngaq.Core.Model.Po.Learn_;
using Ngaq.Core.Infra;
using Ngaq.Core.Shared.Base.Models.Po;
using Ngaq.Core.Shared.User.Models.Po.User;
using Ngaq.Core.Word.Models.Po.Word;
using Ngaq.Core.Shared.Word.Models;
using Ngaq.Core.Shared.Word.Models.Learn_;
using Ngaq.Core.Shared.Word.Models.Po.Kv;
using Ngaq.Core.Shared.Word.Models.Po.Learn;
using Ngaq.Core.Shared.Word.Models.Po.Word;
using Ngaq.Local.Word.Dao;
using Tsinswreng.CsSqlHelper;

public class TestBatSlctAggById(
	DaoWord DaoWord
	,IRepo<PoWord, IdWord> RepoWord
	,ITblMgr TblMgr
	,ISqlCmdMkr SqlCmdMkr
){
	protected DaoWord DaoWord{get;set;} = DaoWord;
	protected IRepo<PoWord, IdWord> RepoWord{get;set;} = RepoWord;
	protected ITblMgr TblMgr{get;set;} = TblMgr;
	protected ISqlCmdMkr SqlCmdMkr{get;set;} = SqlCmdMkr;

	protected JnWord MkJnWord(IdUser Owner){
		var head = new IdWord().ToString();
		var po = new PoWord{
			Id = new IdWord(),
			Owner = Owner,
			Head = head,
			Lang = "__AggTest__",
			StoredAt = Tempus.Now(),
		};
		var prop1 = new PoWordProp{
			Id = new IdWordProp(),
			KType = EKvType.Str,
			KStr = "test_k",
			VType = EKvType.Str,
			VStr = "test_v_" + head,
		};
		var learn1 = new PoWordLearn{
			Id = new IdWordLearn(),
			LearnResult = ELearn.Add,
		};
		return new JnWord(po, [prop1], [learn1]).EnsureForeignId();
	}

	protected async Task<nil> HardDelByWordIds(
		IDbFnCtx Ctx
		,IList<IdWord> ids
		,CT Ct
	){
		if(ids.Count == 0){
			return NIL;
		}

		async Task delByIn<TPo>(
			ITable<TPo> tbl
			,str codeCol
		) where TPo:new(){
			var ps = tbl.NumParams((u64)ids.Count);
			var sql = $"DELETE FROM {tbl.Qt(tbl.DbTblName)} WHERE {tbl.QtCol(codeCol)} IN ({str.Join(",", ps)})";
			var cmd = await SqlCmdMkr.Prepare(Ctx, sql, Ct);
			var arg = ArgDict.Mk(tbl).AddManyT(ps, ids, codeCol);
			await cmd.Args(arg).All1d(Ct);
		}

		var tp = TblMgr.GetTbl<PoWordProp>();
		var tl = TblMgr.GetTbl<PoWordLearn>();
		var tw = TblMgr.GetTbl<PoWord>();

		await delByIn(tp, nameof(I_WordId.WordId));
		await delByIn(tl, nameof(I_WordId.WordId));
		await delByIn(tw, tw.CodeIdName);
		return NIL;
	}

	public async Task<nil> Run(CT Ct){
		var insertedIds = new List<IdWord>();
		var Ctx = new DbFnCtx();
		try{
			var owner = new IdUser();
			var jn1 = MkJnWord(owner);
			var jn2 = MkJnWord(owner);
			insertedIds.Add(jn1.Word.Id);
			insertedIds.Add(jn2.Word.Id);

			var ins = await DaoWord.FnInsertJnWords(Ctx, Ct);
			await ins([jn1, jn2], Ct);

			var queryIds = new List<IdWord>{
				insertedIds[0],
				new IdWord(),
				insertedIds[1],
			};
			var gotAsy = await RepoWord.BatSlctAggById<JnWord>(Ctx, queryIds, Ct);
			var got = await gotAsy.ToListAsync(Ct);

			if(got.Count != queryIds.Count){
				throw new Exception($"count mismatch. expected={queryIds.Count}, got={got.Count}");
			}
			if(got[0] is null || got[0]!.Word.Id != insertedIds[0]){
				throw new Exception("first aggregate not matched");
			}
			if(got[1] is not null){
				throw new Exception("middle aggregate should be null");
			}
			if(got[2] is null || got[2]!.Word.Id != insertedIds[1]){
				throw new Exception("third aggregate not matched");
			}
			if(got[0]!.Props.Count == 0 || got[2]!.Learns.Count == 0){
				throw new Exception("aggregate assets not loaded");
			}

			System.Console.WriteLine("TestBatSlctAggById.Run OK");
			return NIL;
		}
		finally{
			await HardDelByWordIds(Ctx, insertedIds, Ct);
		}
	}
}

