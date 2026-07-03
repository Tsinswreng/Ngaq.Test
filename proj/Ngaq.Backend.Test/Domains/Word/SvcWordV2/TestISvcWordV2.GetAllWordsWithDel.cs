using Ngaq.Core.Shared.User.Models.Po.User;
using Ngaq.Core.Shared.Word.Models.Po.Word;
using Ngaq.Core.Shared.Word.Svc;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Backend.Test.Domains.Word;

public partial class TestISvcWordV2{
	void RegisterGetAllWordsWithDel(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestISvcWordV2),
			[typeof(ISvcWordV2)],
			[]
		);
		var R = register.Register;
		register.TesteeFnNames = [nameof(ISvcWordV2.GetAllWordWithDel)];

		R("GetAllWordsWithDel_Should_ReturnAliveAndSoftDeletedWords", async(o)=>{
			var owner = new IdUser();
			var token = "ut_wv2_all_with_del_" + Guid.NewGuid().ToString("N");
			var alive = MkSyncInput(owner, token + "_alive", "en", token + "_d1");
			var deleted = MkSyncInput(owner, token + "_deleted", "en", token + "_d2");
			try{
				await RunNoTxn(async(Ctx)=>{
					await RepoWord.OrdAddAgg(Ctx, AsyE(alive, deleted), CT.None);
					await RepoWord.OrdSoftDelById(Ctx, AsyE(deleted.Word.Id), CT.None);
					return NIL;
				});

				var got = await ToList(SvcWordV2.GetAllWordWithDel(MkUserCtx(owner), CT.None));
				var gotHeads = got.Select(x=>x.Word.Head).ToHashSet();
				Assert.IsTrue(gotHeads.Contains(alive.Word.Head) && gotHeads.Contains(deleted.Word.Head), "GetAllWordsWithDel should include both alive and soft-deleted words.");
				return NIL;
			}
			finally{
				await TryCleanupByHeadOwner(owner, token);
			}
		});
	}
}
