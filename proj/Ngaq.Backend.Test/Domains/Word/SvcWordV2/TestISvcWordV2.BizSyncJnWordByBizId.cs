using Ngaq.Core.Shared.Sync;
using Ngaq.Core.Model.Po.Kv;
using Ngaq.Core.Shared.User.Models.Po.User;
using Ngaq.Core.Shared.Word.Models;
using Ngaq.Core.Shared.Word.Models.Po.Kv;
using Ngaq.Core.Shared.Word.Models.Po.Word;
using Ngaq.Core.Shared.Word.Svc;
using Tsinswreng.CsSql;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Backend.Test.Domains.Word;

public partial class TestISvcWordV2{
	void RegisterBizSyncJnWordByBizId(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestISvcWordV2),
			[typeof(ISvcWordV2)],
			[]
		);
		var R = register.Register;
		register.TesteeFnNames = [nameof(ISvcWordV2.BatSyncJnWordByBizId)];

		R("BizSyncJnWordByBizId_WhenLocalNotExist_Should_InsertAndReturnAuditDto", async(o)=>{
			var owner = new IdUser();
			var token = "ut_wv2_bizsync_add_" + Guid.NewGuid().ToString("N");
			var remote = MkSyncInput(owner, token + "_h1", "en", token + "_d1");
			try{
				var dtos = await ToList(SvcWordV2.BatSyncJnWordByBizId(MkUserCtx(owner), AsyE(remote), CT.None));
				Assert.IsTrue(dtos.Count == 1, "BizSyncJnWordByBizId should return one dto for one input item");
				Assert.IsTrue(ReferenceEquals(dtos[0].DiffResult, EDiffByBizIdResultForSync.LocalNotExist), "BizSyncJnWordByBizId should mark fresh remote as LocalNotExist");

				await RunNoTxn(async(Ctx)=>{
					var words = (await ToList(RepoWord.GetAll(Ctx, CT.None)))
						.Where(x=>x.Owner == owner && x.Head == remote.Head && x.Lang == remote.Lang)
						.ToList();
					Assert.IsTrue(words.Count == 1, "BizSyncJnWordByBizId should insert word when local does not exist");
					return NIL;
				});
				return NIL;
			}
			finally{
				await TryCleanupByHeadOwner(owner, token);
			}
		});

		R("BizSyncJnWordByBizId_ReSyncSameData_Should_NotDuplicateRows", async(o)=>{
			var owner = new IdUser();
			var token = "ut_wv2_bizsync_resync_" + Guid.NewGuid().ToString("N");
			var remote = MkSyncInput(owner, token + "_h1", "en", token + "_d1");
			try{
				_ = await ToList(SvcWordV2.BatSyncJnWordByBizId(MkUserCtx(owner), AsyE(remote), CT.None));
				var dtos2 = await ToList(SvcWordV2.BatSyncJnWordByBizId(MkUserCtx(owner), AsyE(remote), CT.None));
				Assert.IsTrue(dtos2.Count == 1, "BizSyncJnWordByBizId should still return one dto on re-sync");

				await RunNoTxn(async(Ctx)=>{
					var words = (await ToList(RepoWord.GetAll(Ctx, CT.None)))
						.Where(x=>x.Owner == owner && x.Head == remote.Head && x.Lang == remote.Lang)
						.ToList();
					Assert.IsTrue(words.Count == 1, "BizSyncJnWordByBizId should not duplicate root rows for same biz-id");
					return NIL;
				});
				return NIL;
			}
			finally{
				await TryCleanupByHeadOwner(owner, token);
			}
		});

		R("BizSyncJnWordByBizId_Should_KeepOwnerIsolation", async(o)=>{
			var ownerA = new IdUser();
			var ownerB = new IdUser();
			var token = "ut_wv2_bizsync_owner_" + Guid.NewGuid().ToString("N");
			var remoteA = MkSyncInput(ownerA, token + "_h1", "en", token + "_d1");
			var localB = new PoWord{Id = new IdWord(), Owner = ownerB, Head = remoteA.Head, Lang = remoteA.Lang};
			try{
				await RunNoTxn(async(Ctx)=>{
					await RepoWord.BatAdd(Ctx, AsyE(localB), CT.None);
					return NIL;
				});

				_ = await ToList(SvcWordV2.BatSyncJnWordByBizId(MkUserCtx(ownerA), AsyE(remoteA), CT.None));
				await RunNoTxn(async(Ctx)=>{
					var wordsA = (await ToList(RepoWord.GetAll(Ctx, CT.None)))
						.Where(x=>x.Owner == ownerA && x.Head == remoteA.Head && x.Lang == remoteA.Lang)
						.ToList();
					var wordsB = (await ToList(RepoWord.GetAll(Ctx, CT.None)))
						.Where(x=>x.Owner == ownerB && x.Head == remoteA.Head && x.Lang == remoteA.Lang)
						.ToList();
					Assert.IsTrue(wordsA.Count == 1 && wordsB.Count == 1, "BizSyncJnWordByBizId should isolate by owner");
					return NIL;
				});
				return NIL;
			}
			finally{
				await TryCleanupByHeadOwner(ownerA, token);
				await RunNoTxn(async(Ctx)=>{
					await RepoWord.BatHardDelById(Ctx, AsyE(localB.Id), CT.None);
					return NIL;
				});
			}
		});
	}

	static JnWord MkSyncInput(
		IdUser owner,
		str head,
		str lang,
		str desc
	){
		var id = new IdWord();
		return new JnWord{
			Word = new PoWord{
				Id = id,
				Owner = owner,
				Head = head,
				Lang = lang,
			},
			Props = [
				new PoWordProp{
					Id = new IdWordProp(),
					WordId = id,
					KType = EKvType.Str,
					KStr = KeysProp.Inst.description,
					VType = EKvType.Str,
					VStr = desc,
				}
			],
			Learns = [],
		};
	}
}
