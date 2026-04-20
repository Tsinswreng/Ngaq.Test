using Ngaq.Core.Shared.User.Models.Po.User;
using Ngaq.Core.Shared.Word.Models.Po.Word;
using Ngaq.Core.Shared.Word.Svc;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Backend.Test.Domains.Word;

public partial class TestISvcWordV2{
	void RegisterPackAllWordsWithDel(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestISvcWordV2),
			[typeof(ISvcWordV2)],
			[]
		);
		var R = register.Register;
		register.TesteeFnNames = [nameof(ISvcWordV2.PackAllWordsWithDel)];

		R("PackAllWordsWithDel_Should_BeReadableByUnpackJnWords", async(o)=>{
			var owner = new IdUser();
			var token = "ut_wv2_pack_with_del_" + Guid.NewGuid().ToString("N");
			var alive = MkSyncInput(owner, token + "_alive", "en", token + "_d1");
			var deleted = MkSyncInput(owner, token + "_deleted", "en", token + "_d2");
			try{
				await RunNoTxn(async(Ctx)=>{
					await RepoWord.BatAddAgg(Ctx, AsyE(alive, deleted), CT.None);
					await RepoWord.BatSoftDelById(Ctx, AsyE(deleted.Word.Id), CT.None);
					return NIL;
				});

				using var stream = await SvcWordV2.PackAllWordsWithDel(MkUserCtx(owner), CT.None);
				var unpacked = await ToList(SvcWordV2.UnpackJnWords(stream, CT.None));
				var gotHeads = unpacked.Select(x=>x.Word.Head).ToHashSet();
				if(!gotHeads.Contains(alive.Word.Head) || !gotHeads.Contains(deleted.Word.Head)){
					throw new Exception("PackAllWordsWithDel result should include alive and soft-deleted words.");
				}
				return NIL;
			}
			finally{
				await TryCleanupByHeadOwner(owner, token);
			}
		});
	}
}
