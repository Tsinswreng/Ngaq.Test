namespace Ngaq.Test.Try;

using System.Diagnostics;
using System.Net.WebSockets;
using Microsoft.Extensions.DependencyInjection;
using Ngaq.Core;
using Ngaq.Core.Frontend.User;
using Ngaq.Core.Infra;
using Ngaq.Core.Infra.Errors;
using Ngaq.Core.Shared.Base.Models.Po;
using Ngaq.Core.Shared.Kv.Models;
using Ngaq.Core.Shared.User.Models.Po;
using Ngaq.Core.Shared.User.Models.Po.User;
using Ngaq.Core.Shared.User.UserCtx;
using Ngaq.Core.Shared.Word.Models;
using Ngaq.Core.Shared.Word.Models.Dto;
using Ngaq.Core.Shared.Word.Models.Po.Word;
using Ngaq.Core.Tools;
using Ngaq.Local;
using Ngaq.Local.Db.TswG;
using Ngaq.Local.Di;
using Ngaq.Local.Domains.Word.Dao;
using Ngaq.Local.Word.Dao;
using Ngaq.Test;
using Ngaq.Test.CsSql.Integration.Repo;
using Ngaq.Test.CsLang;
using Ngaq.Test.Tools;
using Ngaq.Test.Try;
using Ngaq.Test.Word;
using Tsinswreng.CsPage;
using Tsinswreng.CsSql;
using Tsinswreng.CsTools;
using Ngaq.Core.Shared.Word.Svc;

public class Try{
static async Task<nil> TryNeoBat(CT Ct){
	var DaoWord = NgaqTest.GetRSvc<DaoWord>();
	var Ctx = new DbFnCtx();
	var userIdU128 = UInt128.Parse("019ADCE46AB10B07CAA1F62B8C6EB306", System.Globalization.NumberStyles.HexNumber);
	var userCtxMgr = NgaqTest.GetRSvc<IFrontendUserCtxMgr>();
	var User = userCtxMgr.GetUserCtx();
	var HeadLangs = new List<Head_Lang>{
		new Head_Lang("peer", "english"),
		new Head_Lang("leak", "english"),
		new Head_Lang("_notExist", "english"),
		new Head_Lang("shaft", "english"),
		new Head_Lang("たのむ", "japanese"),
		new Head_Lang("くま", "japanese"),
		
	};
	var r = await DaoWord.BatSlctIdByOwnerHeadLangWithDel_New(Ctx, User, HeadLangs, Ct);
	await foreach(var e in r){
		var s = e?.ToString()?? "null";
		System.Console.WriteLine(s);
	}
	return NIL;
}

static async Task<nil> TryReadAllWords3(CT Ct){
	var daoWord = NgaqTest.GetRSvc<DaoWord>();
	var mkrDbFnCtx = NgaqTest.GetRSvc<IMkrDbFnCtx>();
	//var Ctx = await mkrDbFnCtx.MkTxnDbFnCtx(Ct);
	var Ctx = new DbFnCtx();//sqlite 純查詢 勿開事務
	var userCtxMgr = NgaqTest.GetRSvc<IFrontendUserCtxMgr>();
	var fnPage = await daoWord.FnPageWords(Ctx, new OptQry{
		IncludeDeleted = true,
	}, Ct);
	var sw = Stopwatch.StartNew();
	var r = await fnPage(userCtxMgr.GetUserCtx(), PageQry.SlctI64Max(), Ct);
	var listPage = await r.ToListPage(Ct);
	sw.Stop();
	//2026_0115_213956
	System.Console.WriteLine(listPage.Data?.Count); // 13304
	System.Console.WriteLine(sw.ElapsedMilliseconds); // 800多
	return NIL;
}

static async Task<nil> TryReadAllWords4(CT Ct){
	var daoWord = NgaqTest.GetRSvc<DaoWord>();
	var mkrDbFnCtx = NgaqTest.GetRSvc<IMkrDbFnCtx>();
	//var Ctx = await mkrDbFnCtx.MkTxnDbFnCtx(Ct);
	var Ctx = new DbFnCtx();//sqlite 純查詢 勿開事務
	var userCtxMgr = NgaqTest.GetRSvc<IFrontendUserCtxMgr>();
	var fnPage = await daoWord.FnPageWordsOld(Ctx, new OptQry{
		IncludeDeleted = true,
	}, Ct);
	var sw = Stopwatch.StartNew();
	var r = await fnPage(userCtxMgr.GetUserCtx(), PageQry.SlctI64Max(), Ct);
	var listPage = await r.ToListPage(Ct);
	sw.Stop();
	//2026_0115_213956
	System.Console.WriteLine(listPage.Data?.Count); // 13304
	System.Console.WriteLine(sw.ElapsedMilliseconds); // 800多
	return NIL;
}

static async Task<nil> TryReadAllWords2(CT Ct){
	var svcWord = NgaqTest.GetRSvc<ISvcWord>();
	var userCtxMgr = NgaqTest.GetRSvc<IFrontendUserCtxMgr>();
	var sw = Stopwatch.StartNew();
	var r = await svcWord.PageWord(userCtxMgr.GetUserCtx(), PageQry.SlctI64Max(), Ct);
	var listPage = await r.ToListPage(Ct);
	sw.Stop();
	//2026_0115_213956
	System.Console.WriteLine(listPage.Data?.Count); // 13258
	System.Console.WriteLine(sw.ElapsedMilliseconds); // 3352
	return NIL;
}

//
static async Task<nil> TryReadAllWords(CT Ct){
	var SqlCmdMkr = NgaqTest.GetRSvc<ISqlCmdMkr>();
	var DbCtxMkr = NgaqTest.GetRSvc<IMkrDbFnCtx>();
	var Ctx = await DbCtxMkr.MkTxnDbFnCtx(Ct);
var Sql =
"""
SELECT W.Id as WId, WP.Id as WpId, WL.Id as WlId from Word W
LEFT JOIN WordProp WP on W.Id = WP.WordId
LEFT JOIN WordLearn WL on W.Id = WL.WordId
""";
var Cmd = await SqlCmdMkr.Prepare(Ctx, Sql, Ct);
var sw = Stopwatch.StartNew();
var R = await Cmd.All1d(Ct);
sw.Stop();
System.Console.WriteLine(R.Count); // 2026_0115_094041 210285
System.Console.WriteLine(sw.ElapsedMilliseconds); // 922ms jit單次
return NIL;
}

}
