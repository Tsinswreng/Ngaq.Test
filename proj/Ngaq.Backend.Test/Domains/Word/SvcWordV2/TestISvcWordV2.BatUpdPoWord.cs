using Ngaq.Core.Infra.IF;
using Ngaq.Core.Infra;
using Ngaq.Core.Shared.Base.Models.Po;
using Ngaq.Core.Shared.User.Models.Po.User;
using Ngaq.Core.Shared.Word.Models.Po.Word;
using Ngaq.Core.Shared.Word.Svc;
using Tsinswreng.CsSql;
using Tsinswreng.CsTreeTest;
using Tsinswreng.CsTempus;

namespace Ngaq.Backend.Test.Domains.Word;

public partial class TestISvcWordV2{
	void RegisterBatUpdPoWord(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestISvcWordV2),
			[typeof(ISvcWordV2)],
			[]
		);
		var R = register.Register;
		register.TesteeFnNames = [nameof(ISvcWordV2.BatUpdPoWord)];

		R("BatUpdPoWord_WhenHeadLangUnchanged_Should_UpdateOtherFields_AndReturnOriginalId", async(o)=>{
			var owner = new IdUser();
			var token = "ut_wv2_updpo_same_" + Guid.NewGuid().ToString("N");
			var word = new PoWord{
				Id = new IdWord(),
				Owner = owner,
				Head = token + "_h1",
				Lang = "en",
				StoredAt = UnixMs.FromUnixMs(1000),
			};
			try{
				await RunNoTxn(async(Ctx)=>{
					await RepoWord.OrdAdd(Ctx, AsyE(word), CT.None);
					return NIL;
				});

				var upd = new PoWord{
					Id = word.Id,
					Owner = owner,
					Head = word.Head,
					Lang = word.Lang,
					StoredAt = UnixMs.FromUnixMs(2000),
				};
				var rtn = await SvcWordV2.BatUpdPoWord(MkUserCtx(owner), AsyE(upd), CT.None);
				var rows = await ToList(rtn);
				Assert.IsTrue(rows.Count == 1 && rows[0] is not null && rows[0]!.FinalId == word.Id, "BatUpdPoWord should return original id when (Id,Head,Lang) unchanged");

				await RunNoTxn(async(Ctx)=>{
					var got = await ToList(RepoWord.OrdGetByIdWithDel(Ctx, AsyE(word.Id), CT.None));
					Assert.IsTrue(got.Count == 1 && got[0] is not null && got[0]!.StoredAt == upd.StoredAt, "BatUpdPoWord should update other fields when (Id,Head,Lang) unchanged");
					return NIL;
				});
				return NIL;
			}
			finally{
				await RunNoTxn(async(Ctx)=>{
					await RepoWord.OrdHardDelById(Ctx, AsyE(word.Id), CT.None);
					return NIL;
				});
			}
		});

		R("BatUpdPoWord_WhenHeadLangChangedToExisting_Should_ReturnMergedTargetId", async(o)=>{
			var owner = new IdUser();
			var token = "ut_wv2_updpo_merge_" + Guid.NewGuid().ToString("N");
			var src = new PoWord{
				Id = new IdWord(),
				Owner = owner,
				Head = token + "_src",
				Lang = "en",
				StoredAt = UnixMs.FromUnixMs(1000),
			};
			var dst = new PoWord{
				Id = new IdWord(),
				Owner = owner,
				Head = token + "_dst",
				Lang = "en",
				StoredAt = UnixMs.FromUnixMs(1200),
			};
			try{
				await RunNoTxn(async(Ctx)=>{
					await RepoWord.OrdAdd(Ctx, AsyE(src, dst), CT.None);
					return NIL;
				});

				var upd = new PoWord{
					Id = src.Id,
					Owner = owner,
					Head = dst.Head,
					Lang = dst.Lang,
					StoredAt = UnixMs.FromUnixMs(3000),
				};
				var rows = await ToList(await SvcWordV2.BatUpdPoWord(MkUserCtx(owner), AsyE(upd), CT.None));
				Assert.IsTrue(rows.Count == 1 && rows[0] is not null && rows[0]!.FinalId == dst.Id, "BatUpdPoWord should return merged target id when (Head,Lang) conflicts");

				await RunNoTxn(async(Ctx)=>{
					var srcGot = await ToList(RepoWord.OrdGetByIdWithDel(Ctx, AsyE(src.Id), CT.None));
					var dstGot = await ToList(RepoWord.OrdGetByIdWithDel(Ctx, AsyE(dst.Id), CT.None));
					Assert.IsTrue(srcGot.Count == 1 && srcGot[0] is not null && srcGot[0]!.IsDeleted(), "BatUpdPoWord should soft-delete source after merge");
					Assert.IsTrue(dstGot.Count == 1 && dstGot[0] is not null && !dstGot[0]!.IsDeleted(), "BatUpdPoWord should keep target after merge");
					return NIL;
				});
				return NIL;
			}
			finally{
				await RunNoTxn(async(Ctx)=>{
					await RepoWord.OrdHardDelById(Ctx, AsyE(src.Id, dst.Id), CT.None);
					return NIL;
				});
			}
		});
	}
}
