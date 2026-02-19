//先dotent build編譯、後在vscode 調試界面 選TestWin㕥行、如是則既有斷點又可連數據庫
//直ᵈ dotnet run 則cwd不對、連不到數據庫
global using static Program;
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
using Ngaq.Core.Shared.Word.Models;
using Ngaq.Core.Shared.Word.Models.Dto;
using Ngaq.Core.Tools;
using Ngaq.Core.Word.Svc;
using Ngaq.Local;
using Ngaq.Local.Db.TswG;
using Ngaq.Local.Di;
using Ngaq.Local.Domains.Word.Dao;
using Ngaq.Local.Word.Dao;
using Ngaq.Test;
using Ngaq.Test.CsLang;
using Ngaq.Test.Tools;
using Ngaq.Test.Try;
using Ngaq.Test.Word;
using Tsinswreng.CsPage;
using Tsinswreng.CsSqlHelper;
using Tsinswreng.CsTools;
//dotnet publish -c Release -r win-x64
// ./bin/Release/net10.0/win-x64/publish/Ngaq.Test.exe
using Jn = System.Text.Json.Nodes.JsonNode;
#region Main

new TestToolYaml().TryYamlStrToDict();

throw new Exception("AOT");

#endregion Main


static async Task<nil> TryReadAllWords3(CT Ct){
	var daoWord = Program.GetRSvc<DaoWord>();
	var mkrDbFnCtx = Program.GetRSvc<IMkrDbFnCtx>();
	//var Ctx = await mkrDbFnCtx.MkTxnDbFnCtx(Ct);
	var Ctx = new DbFnCtx();//sqlite 純查詢 勿開事務
	var userCtxMgr = Program.GetRSvc<IFrontendUserCtxMgr>();
	var fnPage = await daoWord.FnPageWords(Ctx, new OptQry{
		IncludeDeleted = true,
	}, Ct);
	var sw = Stopwatch.StartNew();
	var r = await fnPage(userCtxMgr.GetUserCtx(), PageQry.SlctAll(), Ct);
	var listPage = await r.ToListPage(Ct);
	sw.Stop();
	//2026_0115_213956
	System.Console.WriteLine(listPage.Data?.Count); // 13304
	System.Console.WriteLine(sw.ElapsedMilliseconds); // 800多
	return NIL;
}

static async Task<nil> TryReadAllWords4(CT Ct){
	var daoWord = Program.GetRSvc<DaoWord>();
	var mkrDbFnCtx = Program.GetRSvc<IMkrDbFnCtx>();
	//var Ctx = await mkrDbFnCtx.MkTxnDbFnCtx(Ct);
	var Ctx = new DbFnCtx();//sqlite 純查詢 勿開事務
	var userCtxMgr = Program.GetRSvc<IFrontendUserCtxMgr>();
	var fnPage = await daoWord.FnPageWordsOld(Ctx, new OptQry{
		IncludeDeleted = true,
	}, Ct);
	var sw = Stopwatch.StartNew();
	var r = await fnPage(userCtxMgr.GetUserCtx(), PageQry.SlctAll(), Ct);
	var listPage = await r.ToListPage(Ct);
	sw.Stop();
	//2026_0115_213956
	System.Console.WriteLine(listPage.Data?.Count); // 13304
	System.Console.WriteLine(sw.ElapsedMilliseconds); // 800多
	return NIL;
}

static async Task<nil> TryReadAllWords2(CT Ct){
	var svcWord = Program.GetRSvc<ISvcWord>();
	var userCtxMgr = Program.GetRSvc<IFrontendUserCtxMgr>();
	var sw = Stopwatch.StartNew();
	var r = await svcWord.PageWord(userCtxMgr.GetUserCtx(), PageQry.SlctAll(), Ct);
	var listPage = await r.ToListPage(Ct);
	sw.Stop();
	//2026_0115_213956
	System.Console.WriteLine(listPage.Data?.Count); // 13258
	System.Console.WriteLine(sw.ElapsedMilliseconds); // 3352
	return NIL;
}

//
static async Task<nil> TryReadAllWords(CT Ct){
	var SqlCmdMkr = Program.GetRSvc<ISqlCmdMkr>();
	var DbCtxMkr = Program.GetRSvc<IMkrDbFnCtx>();
	var Ctx = await DbCtxMkr.MkTxnDbFnCtx(Ct);
var Sql =
"""
SELECT W.Id as WId, WP.Id as WpId, WL.Id as WlId from Word W
LEFT JOIN WordProp WP on W.Id = WP.WordId
LEFT JOIN WordLearn WL on W.Id = WL.WordId
""";
var Cmd = await Ctx.PrepareToDispose(SqlCmdMkr, Sql, Ct);
var sw = Stopwatch.StartNew();
var R = await Cmd.All1d(Ct);
sw.Stop();
System.Console.WriteLine(R.Count); // 2026_0115_094041 210285
System.Console.WriteLine(sw.ElapsedMilliseconds); // 922ms jit單次
return NIL;
}

internal partial class Program{
	public static str GetFullTypeName<T>(){
		return typeof(T).FullName!;
	}
	// static async Task Main(string[] args){

	// }
	static Program(){
		Di();
		InitApp();
	}
	public static ServiceProvider SvcProvider = null!;
	public static nil Di(){
		var svc = new ServiceCollection();
		svc
			.SetupCore()
			.SetupLocal()//TODO 改成按需API調用
			.SetupLocalFrontend()
		;
		SvcProvider = svc.BuildServiceProvider();
		return NIL;
	}
	public static nil InitApp(){
		AppIniter.Inst.Sp = SvcProvider;
		_ = AppIniter.Inst.Init(default).Result;
		return NIL;
	}
	public static T GetRSvc<T>()
		where T : class
	{
		return SvcProvider.GetRequiredService<T>();
	}

}
