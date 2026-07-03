using Ngaq.Core.Model.Po.Kv;
using Ngaq.Core.Model.Po.Learn_;
using Ngaq.Core.Shared.Base.Models.Po;
using Ngaq.Core.Shared.User.Models.Po.User;
using Ngaq.Core.Shared.Word.Models.Learn_;
using Ngaq.Core.Shared.Word.Models.Po.Kv;
using Ngaq.Core.Shared.Word.Models.Po.Learn;
using Ngaq.Core.Shared.Word.Models.Po.Word;
using Ngaq.Core.Shared.Word.Svc;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Backend.Test.Domains.Word;

public partial class TestISvcWordV2{
	void RegisterHardDelSoftDeleted(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestISvcWordV2),
			[typeof(ISvcWordV2)],
			[]
		);
		var R = register.Register;
		register.TesteeFnNames = [nameof(ISvcWordV2.HardDelSoftDeleted)];

		R("HardDelSoftDeleted_Should_Remove_SoftDeletedRoot_Only", async(o)=>{
			var owner = new IdUser();
			var token = "ut_wv2_hard_del_soft_" + Guid.NewGuid().ToString("N");
			var alive = new PoWord{
				Id = new IdWord(),
				Owner = owner,
				Head = token + "_alive",
				Lang = "en",
			};
			var deleted = new PoWord{
				Id = new IdWord(),
				Owner = owner,
				Head = token + "_deleted",
				Lang = "en",
			};
			var aliveProp = new PoWordProp{
				Id = new IdWordProp(),
				WordId = alive.Id,
				KType = EKvType.Str,
				KStr = KeysProp.Inst.description,
				VType = EKvType.Str,
				VStr = token + "_alive_d1",
			};
			var deletedProp = new PoWordProp{
				Id = new IdWordProp(),
				WordId = deleted.Id,
				KType = EKvType.Str,
				KStr = KeysProp.Inst.description,
				VType = EKvType.Str,
				VStr = token + "_deleted_d1",
			};
			var aliveLearn = new PoWordLearn{
				Id = new IdWordLearn(),
				WordId = alive.Id,
				LearnResult = ELearn.Add,
			};
			var deletedLearn = new PoWordLearn{
				Id = new IdWordLearn(),
				WordId = deleted.Id,
				LearnResult = ELearn.Add,
			};

			try{
				await RunNoTxn(async(Ctx)=>{
					await RepoWord.OrdAdd(Ctx, AsyE(alive, deleted), CT.None);
					await RepoProp.OrdAdd(Ctx, AsyE(aliveProp, deletedProp), CT.None);
					await RepoLearn.OrdAdd(Ctx, AsyE(aliveLearn, deletedLearn), CT.None);
					await RepoWord.OrdSoftDelById(Ctx, AsyE(deleted.Id), CT.None);
					return NIL;
				});

				await SvcWordV2.HardDelSoftDeleted(MkUserCtx(owner), CT.None);

				await RunNoTxn(async(Ctx)=>{
					var aliveWord = await ToList(RepoWord.OrdGetByIdWithDel(Ctx, AsyE(alive.Id), CT.None));
					Assert.IsTrue(aliveWord.Count == 1 && aliveWord[0] is not null && !aliveWord[0]!.IsDeleted(), "alive root should remain");

					var deletedWord = await ToList(RepoWord.OrdGetByIdWithDel(Ctx, AsyE(deleted.Id), CT.None));
					Assert.IsTrue(deletedWord.Count == 1 && deletedWord[0] is null, "soft-deleted root should be hard-deleted");

					var alivePropGot = await ToList(RepoProp.GetInIdsWithDel(Ctx, AsyE(aliveProp.Id), CT.None));
					Assert.IsTrue(alivePropGot.Count == 1 && alivePropGot[0] is not null, "alive word prop should remain");
					var deletedPropGot = await ToList(RepoProp.GetInIdsWithDel(Ctx, AsyE(deletedProp.Id), CT.None));
					Assert.IsTrue(deletedPropGot.Count == 1 && deletedPropGot[0] is not null, "non-soft-deleted prop of deleted word should remain (orphan is allowed)");

					var aliveLearnGot = await ToList(RepoLearn.GetInIdsWithDel(Ctx, AsyE(aliveLearn.Id), CT.None));
					Assert.IsTrue(aliveLearnGot.Count == 1 && aliveLearnGot[0] is not null, "alive word learn should remain");
					var deletedLearnGot = await ToList(RepoLearn.GetInIdsWithDel(Ctx, AsyE(deletedLearn.Id), CT.None));
					Assert.IsTrue(deletedLearnGot.Count == 1 && deletedLearnGot[0] is not null, "non-soft-deleted learn of deleted word should remain (orphan is allowed)");
					return NIL;
				});
				return NIL;
			}
			finally{
				await RunNoTxn(async(Ctx)=>{
					await RepoProp.OrdHardDelById(Ctx, AsyE(aliveProp.Id, deletedProp.Id), CT.None);
					await RepoLearn.OrdHardDelById(Ctx, AsyE(aliveLearn.Id, deletedLearn.Id), CT.None);
					await RepoWord.OrdHardDelById(Ctx, AsyE(alive.Id, deleted.Id), CT.None);
					return NIL;
				});
			}
		});

		R("HardDelSoftDeleted_Should_Remove_Only_SoftDeletedAssets_Of_AliveRoot", async(o)=>{
			var owner = new IdUser();
			var token = "ut_wv2_hard_del_soft_asset_" + Guid.NewGuid().ToString("N");
			var word = new PoWord{
				Id = new IdWord(),
				Owner = owner,
				Head = token + "_w",
				Lang = "en",
			};
			var aliveProp = new PoWordProp{
				Id = new IdWordProp(),
				WordId = word.Id,
				KType = EKvType.Str,
				KStr = KeysProp.Inst.description,
				VType = EKvType.Str,
				VStr = token + "_alive_d1",
			};
			var softProp = new PoWordProp{
				Id = new IdWordProp(),
				WordId = word.Id,
				KType = EKvType.Str,
				KStr = KeysProp.Inst.note,
				VType = EKvType.Str,
				VStr = token + "_soft_d1",
			};
			var aliveLearn = new PoWordLearn{
				Id = new IdWordLearn(),
				WordId = word.Id,
				LearnResult = ELearn.Add,
			};
			var softLearn = new PoWordLearn{
				Id = new IdWordLearn(),
				WordId = word.Id,
				LearnResult = ELearn.Rmb,
			};

			try{
				await RunNoTxn(async(Ctx)=>{
					await RepoWord.OrdAdd(Ctx, AsyE(word), CT.None);
					await RepoProp.OrdAdd(Ctx, AsyE(aliveProp, softProp), CT.None);
					await RepoLearn.OrdAdd(Ctx, AsyE(aliveLearn, softLearn), CT.None);
					await RepoProp.SoftDelInId(Ctx, AsyE(softProp.Id), CT.None);
					await RepoLearn.SoftDelInId(Ctx, AsyE(softLearn.Id), CT.None);
					return NIL;
				});

				await SvcWordV2.HardDelSoftDeleted(MkUserCtx(owner), CT.None);

				await RunNoTxn(async(Ctx)=>{
					var wordGot = await ToList(RepoWord.OrdGetByIdWithDel(Ctx, AsyE(word.Id), CT.None));
					Assert.IsTrue(wordGot.Count == 1 && wordGot[0] is not null && !wordGot[0]!.IsDeleted(), "alive root should remain");

					var alivePropGot = await ToList(RepoProp.GetInIdsWithDel(Ctx, AsyE(aliveProp.Id), CT.None));
					Assert.IsTrue(alivePropGot.Count == 1 && alivePropGot[0] is not null, "alive prop should remain");
					var softPropGot = await ToList(RepoProp.GetInIdsWithDel(Ctx, AsyE(softProp.Id), CT.None));
					Assert.IsTrue(softPropGot.Count == 0 || softPropGot.All(x=>x is null), "soft-deleted prop should be hard-deleted");

					var aliveLearnGot = await ToList(RepoLearn.GetInIdsWithDel(Ctx, AsyE(aliveLearn.Id), CT.None));
					Assert.IsTrue(aliveLearnGot.Count == 1 && aliveLearnGot[0] is not null, "alive learn should remain");
					var softLearnGot = await ToList(RepoLearn.GetInIdsWithDel(Ctx, AsyE(softLearn.Id), CT.None));
					Assert.IsTrue(softLearnGot.Count == 0 || softLearnGot.All(x=>x is null), "soft-deleted learn should be hard-deleted");
					return NIL;
				});
				return NIL;
			}
			finally{
				await RunNoTxn(async(Ctx)=>{
					await RepoProp.OrdHardDelById(Ctx, AsyE(aliveProp.Id, softProp.Id), CT.None);
					await RepoLearn.OrdHardDelById(Ctx, AsyE(aliveLearn.Id, softLearn.Id), CT.None);
					await RepoWord.OrdHardDelById(Ctx, AsyE(word.Id), CT.None);
					return NIL;
				});
			}
		});
	}
}
