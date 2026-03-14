using System.Globalization;
using Ngaq.Core.Shared.User.UserCtx;
using Tsinswreng.CsPage;
using Ngaq.Core.Shared.Word.Models.Po.Word;
using Ngaq.Core.Infra.IF;
using Ngaq.Core.Shared.Word.Svc;

namespace Ngaq.Test.Word;

public class TestWord{
	public async Task TestSoftDelJnWordsByIds(){
		var SvcWord = NgaqTest.GetRSvc<ISvcWord>();
		var UserCtxMgr = NgaqTest.GetRSvc<IUserCtxMgr>();
		var User = UserCtxMgr.GetUserCtx();
		await SvcWord.SoftDelJnWordsByIds(User, [IdWord.FromLow64Base("1cQ0oCAGxMzXNui7hE-ff")], default);
	}

	public static i64 IsoToUnixMs(str Iso){
		return DateTimeOffset.ParseExact(
			Iso
			,"yyyy-MM-ddTHH:mm:ss.fffzzz", CultureInfo.InvariantCulture
		).ToUnixTimeMilliseconds();
	}

	public static str UnixMsToIso(i64 UnixMs){
		return DateTimeOffset.FromUnixTimeMilliseconds(UnixMs).ToString(
			"yyyy-MM-ddTHH:mm:ss.fffzzz", CultureInfo.InvariantCulture
		);
	}

	public async Task TestGetChangedWordsAfterTime(){
		var SvcWord = NgaqTest.GetRSvc<ISvcWord>()!;
		var UserCtxMgr = NgaqTest.GetRSvc<IUserCtxMgr>()!;
		var User = UserCtxMgr.GetUserCtx();
		var PageQry = new PageQry{
			PageIdx = 0,
			PageSize = 99999999,
		};

		var Tem = IsoToUnixMs("2025-10-04T18:16:41.197+08:00");

		var Page = await SvcWord.PageChangedWordsWithDelWordsAfterTime(User, PageQry, Tem, default);
		if(Page.Data is not null){
			foreach(var (i,word) in Page.Data.Index()){
				//System.Console.Write(i+": "+word.Head+": ");
				System.Console.WriteLine(
					//UnixMsToIso(word.BizUpdatedAt??word.StoredAt)
				);
				// System.Console.WriteLine(
				// 	JSON.stringify(words)
				// );
			}
			System.Console.WriteLine("Page.Data.Count: "+Page.Data.Count);
		}
	}
}


