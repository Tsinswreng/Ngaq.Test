using Ngaq.Core.Infra;
using Ngaq.Core.Model.Po.Learn_;
using Ngaq.Core.Shared.User.Models.Po.User;
using Ngaq.Core.Shared.Word.Models.Learn_;
using Ngaq.Core.Shared.Word.Models.Po.Learn;
using Ngaq.Core.Shared.Word.Models.Po.Word;
using Ngaq.Core.Shared.Word.Svc;
using Ngaq.Core.Tools;
using Tsinswreng.CsSql;
using Tsinswreng.CsTempus;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Backend.Test.Domains.Word;

public partial class TestISvcWordV2{
	void RegisterBatAddNewLearnRecord(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestISvcWordV2)
			,[typeof(ISvcWordV2)]
			,[]
		);
		var R = register.Register;
		register.TesteeFnNames = [nameof(ISvcWordV2.BatAddNewLearnRecord)];

		R("BatAddNewLearnRecord_Should_InsertLearns_And_TouchWordBizUpdatedAt", async(o)=>{
			var owner = new IdUser();
			var token = "ut_wv2_learn_" + Guid.NewGuid().ToString("N");
			var w1 = new PoWord{Id = new IdWord(), Owner = owner, Head = token + "_w1", Lang = "en", BizUpdatedAt = UnixMs.Zero};
			var w2 = new PoWord{Id = new IdWord(), Owner = owner, Head = token + "_w2", Lang = "en", BizUpdatedAt = UnixMs.Zero};
			var learns = new[]{
				new PoWordLearn{Id = new IdWordLearn(), WordId = w1.Id, LearnResult = ELearn.Add},
				new PoWordLearn{Id = new IdWordLearn(), WordId = w1.Id, LearnResult = ELearn.Rmb},
				new PoWordLearn{Id = new IdWordLearn(), WordId = w2.Id, LearnResult = ELearn.Fgt},
			};

			try{
				await RunNoTxn(async(Ctx)=>{
					await RepoWord.OrdAdd(Ctx, AsyE(w1, w2), CT.None);
					return NIL;
				});

				await SvcWordV2.BatAddNewLearnRecord(MkUserCtx(owner), AsyE(learns), CT.None);

				await RunNoTxn(async(Ctx)=>{
					var gotLearns = await ToList(RepoLearn.OrdGetByIdWithDel(Ctx, AsyE(learns.Select(x=>x.Id).ToArray()), CT.None));
					Assert.IsTrue(gotLearns.Count == learns.Length && gotLearns.All(x => x is not null), "BatAddNewLearnRecord failed to insert all learn records");
					var gotWords = await ToList(RepoWord.OrdGetByIdWithDel(Ctx, AsyE(w1.Id, w2.Id), CT.None));
					Assert.IsTrue(gotWords.Count == 2 && gotWords.All(x => x is not null), "failed to load words after BatAddNewLearnRecord");
					Assert.IsTrue(gotWords.All(x => !x!.BizUpdatedAt.IsNullOrDefault()), "BatAddNewLearnRecord did not touch PoWord.BizUpdatedAt");
					return NIL;
				});
				return NIL;
			}
			finally{
				await RunNoTxn(async(Ctx)=>{
					await RepoLearn.OrdHardDelById(Ctx, AsyE(learns.Select(x=>x.Id).ToArray()), CT.None);
					await RepoWord.OrdHardDelById(Ctx, AsyE(w1.Id, w2.Id), CT.None);
					return NIL;
				});
			}
		});

		R("BatAddNewLearnRecord_WhenInputEmpty_Should_NoOp", async(o)=>{
			var owner = new IdUser();
			var token = "ut_wv2_empty_learn_" + Guid.NewGuid().ToString("N");
			var word = new PoWord{Id = new IdWord(), Owner = owner, Head = token + "_w", Lang = "en", BizUpdatedAt = UnixMs.Zero};
			try{
				await RunNoTxn(async(Ctx)=>{
					await RepoWord.OrdAdd(Ctx, AsyE(word), CT.None);
					return NIL;
				});

				await SvcWordV2.BatAddNewLearnRecord(MkUserCtx(owner), AsyE<PoWordLearn>(), CT.None);

				await RunNoTxn(async(Ctx)=>{
					var learns = await ToList(RepoLearn.GetAll(Ctx, CT.None));
					Assert.IsTrue(learns.All(x => x.WordId != word.Id), "empty learn-record input should not insert learn rows");
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
	}
}
