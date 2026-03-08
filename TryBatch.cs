
#if false //勿刪
发生异常: CLR/System.InvalidOperationException
“System.InvalidOperationException”类型的异常在 System.Private.CoreLib.dll 中发生，但未在用户代码中进行处理: 'SqliteConnection does not support nested transactions.'
   在 Microsoft.Data.Sqlite.SqliteConnection.BeginTransaction(IsolationLevel isolationLevel, Boolean deferred)
   在 Microsoft.Data.Sqlite.SqliteConnection.BeginTransaction(IsolationLevel isolationLevel)
   在 Microsoft.Data.Sqlite.SqliteConnection.BeginDbTransaction(IsolationLevel isolationLevel)
   在 Tsinswreng.CsSqlHelper.Sqlite.SqliteCmdMkr.<MkTxn>d__8.MoveNext() 在 E:\_code\CsNgaq\Tsinswreng.CsSqlHelper\proj\Tsinswreng.CsSqlHelper.Sqlite\SqliteCmdMkr.cs 中: 第 107 行
   在 Tsinswreng.CsSqlHelper.IMkrDbFnCtx.<MkTxnDbFnCtx>d__3.MoveNext() 在 E:\_code\CsNgaq\Tsinswreng.CsSqlHelper\proj\Tsinswreng.CsSqlHelper\IDbFnCtxMkr.cs 中: 第 12 行
   在 Ngaq.Test.TryBatch.<Try>d__1.MoveNext() 在 E:\_code\CsNgaq\Ngaq.Test\TryBatch.cs 中: 第 51 行
   在 Program.<<Main>$>d__0.MoveNext() 在 E:\_code\CsNgaq\Ngaq.Test\Ngaq.Test.cs 中: 第 37 行
#endif
using System.Diagnostics;
using Ngaq.Core.Infra.IF;
using Ngaq.Core.Shared.Word.Models.Po.Word;
using Ngaq.Local.Db.TswG;

using Tsinswreng.CsCore;
using Tsinswreng.CsPage;
using Tsinswreng.CsSqlHelper;

namespace Ngaq.Test;

public class TryBatch {
	public async Task TestNull(CT Ct) {
		var RepoWord = Program.GetRSvc<IRepo<PoWord, IdWord>>();
		//var Mkr = Program.GetRSvc<IMkrDbFnCtx>();
		// var Ctx = await Mkr.MkTxnDbFnCtx(Ct);
		var Ctx = new DbFnCtx();
		var IdStrs = new List<str>(){
			new IdWord().ToString(),
			"1ccGi7D8eI8QwLt1azp36",
			"1ccGi7BkopxbaRr_0amRO",
			new IdWord().ToString(),
			"1ccGi7C1AbrCSztW1DWub",
			new IdWord().ToString(),
		};
		var ids = IdStrs.Select(x=>IdWord.FromLow64Base(x));
		var R = await RepoWord.SlctManyInIdsWithDel(Ctx, ids, Ct);
		var RList = await R.ToListAsync();//預期有6個元素、但實際只有3個、new IdWord()的位置預期應爲null
		foreach (var r in RList) {
			System.Console.WriteLine(r==null?"null":r.Head);
		}
	}

	public async Task Try(CT Ct) {
		var RepoWord = Program.GetRSvc<IRepo<PoWord, IdWord>>();

		// 先用這個把所有PoWord查出來
		// var Mkr = Program.GetRSvc<IMkrDbFnCtx>();
		// var Ctx = await Mkr.MkTxnDbFnCtx(Ct);
		var Ctx = new DbFnCtx();
		var fnPageAll = await RepoWord.FnPageAll(Ctx, Ct);
		var pageQry = PageQry.SlctI64Max();
		var allWordsPage = await fnPageAll(pageQry, Ct);
		var allWords = await allWordsPage.DataAsyE.OrEmpty().ToListAsync(Ct);

		if (!allWords.Any()) {
			Console.WriteLine("No words found in database.");
			return;
		}

		System.Console.WriteLine("WordCnt:"+allWords.Count);//13304, BatchSize: 100
		var testIds = allWords.Select(x=>x.Id);
		// 測試 BatchSlctById 性能
		var sw = Stopwatch.StartNew();
		var inResult = await RepoWord.SlctManyInIdsWithDel(Ctx, testIds, Ct);
		var inResults = await inResult.ToListAsync();
		sw.Stop();
		var inTime = sw.ElapsedMilliseconds;
		Console.WriteLine($"SlctManyInIds completed in {inTime}ms");

		sw.Restart();
		var batResult =await RepoWord.BatSlctById(Ctx, testIds, Ct);
		var batResults = await batResult.ToListAsync();
		sw.Stop();
		var batTime = sw.ElapsedMilliseconds;
		System.Console.WriteLine($"BatchSlctById completed in {batTime}ms");

		// 測試 FnSlctOneById 性能
		sw.Restart();
		var fnSlctOneById = await RepoWord.FnSlctOneById(Ctx, Ct);
		var singleResults = new List<PoWord?>();

		foreach (var id in testIds) {
			var result = await fnSlctOneById(id, Ct);
			singleResults.Add(result);
		}

		sw.Stop();
		var singleTime = sw.ElapsedMilliseconds;
		Console.WriteLine($"FnSlctOneById completed in {singleTime}ms");


	}
}

/*
WordCnt:13304, BatchSize: 1
SlctManyInIds completed in 549ms
BatchSlctById completed in 535ms
FnSlctOneById completed in 485ms

WordCnt:13304, BatchSize: 50
SlctManyInIds completed in 117ms
BatchSlctById completed in 498ms
FnSlctOneById completed in 499ms

WordCnt:13304, BatchSize: 100
SlctManyInIds completed in 118ms
BatchSlctById completed in 560ms
FnSlctOneById completed in 509ms

WordCnt:13304, BatchSize: 500
SlctManyInIds completed in 125ms
BatchSlctById completed in 904ms
FnSlctOneById completed in 507ms
 */
//TODO 測 pg
