using System.Diagnostics;

using Microsoft.Extensions.DependencyInjection;
using Ngaq.Core.Shared.Kv.Models;
using Ngaq.Core.Shared.Kv.Svc;
using Ngaq.Core.Shared.User.UserCtx;
using Ngaq.Core.Shared.Word.Models;
using Ngaq.Core.Shared.Word.Models.Po.Kv;
using Ngaq.Core.Shared.Word.Models.Po.Word;
using Ngaq.Core.Word.Svc;
using Ngaq.Local.Db.TswG;
using Ngaq.Local.Word.Dao;
using Tsinswreng.CsCore;
using Tsinswreng.CsPage;
using Tsinswreng.CsSqlHelper;
namespace Ngaq.Test.Try;

public class TryRepoBat{
	public IServiceProvider SvcProvdr;
/* 
BatchSize=50{
Pg:
SlctManyInIds: 1507ms
BatSlctById: 4628ms
SlctOneById: 23890ms
allIds.Count: 113372

Sqlite:
SlctManyInIds: 158ms
BatSlctById: 741ms
SlctOneById: 568ms
allIds.Count: 13304
}

 */
public async Task Run(CT Ct){
	var RepoWord = SvcProvdr.GetRequiredService<IRepo<PoWord, IdWord>>();
	var Ctx = new DbFnCtx();
	var fnPageAll = await RepoWord.FnPageAll(Ctx, Ct);
	var all = await fnPageAll(PageQry.SlctI64Max(), Ct);
	var data = all.DataAsyE.OrEmpty();
	var allIds = await data.Select(x=>x.Id).ToListAsync(Ct);
	var sw = Stopwatch.StartNew();
	{
		sw.Restart();
		var r = await RepoWord.SlctManyInIdsWithDel(Ctx, allIds, Ct);
		var list = await r.ToListAsync(Ct);
		sw.Stop();
		Console.WriteLine($"SlctManyInIds: {sw.ElapsedMilliseconds}ms");
	}
	{
		sw.Restart();
		var r = await RepoWord.BatSlctById(Ctx, allIds, Ct);
		var list = await r.ToListAsync(Ct);
		sw.Stop();
		Console.WriteLine($"BatSlctById: {sw.ElapsedMilliseconds}ms");
	}
	{
		var slctOne = await RepoWord.FnSlctOneById(Ctx, Ct);
		sw.Restart();
		foreach(var id in allIds){
			var po = await slctOne(id, Ct);
		}
		sw.Stop();
		Console.WriteLine($"SlctOneById: {sw.ElapsedMilliseconds}ms");
	}
	System.Console.WriteLine("allIds.Count: "+ allIds.Count);
}

}
