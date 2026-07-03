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
	void RegisterSoftDelPoWordInId(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestISvcWordV2),
			[typeof(ISvcWordV2)],
			[]
		);
		var R = register.Register;
		register.TesteeFnNames = [nameof(ISvcWordV2.SoftDelPoWordInId)];

		R("SoftDelPoWordInId_Should_SoftDelete_Only_Root", async(o)=>{
			var owner = new IdUser();
			var token = "ut_wv2_soft_root_only_" + Guid.NewGuid().ToString("N");
			var word = new PoWord{
				Id = new IdWord(),
				Owner = owner,
				Head = token + "_w",
				Lang = "en",
			};
			var props = new[]{
				new PoWordProp{
					Id = new IdWordProp(),
					WordId = word.Id,
					KType = EKvType.Str,
					KStr = KeysProp.Inst.description,
					VType = EKvType.Str,
					VStr = token + "_d1",
				},
			};
			var learns = new[]{
				new PoWordLearn{
					Id = new IdWordLearn(),
					WordId = word.Id,
					LearnResult = ELearn.Add,
				},
			};

			try{
				await RunNoTxn(async(Ctx)=>{
					await RepoWord.OrdAdd(Ctx, AsyE(word), CT.None);
					await RepoProp.OrdAdd(Ctx, AsyE(props), CT.None);
					await RepoLearn.OrdAdd(Ctx, AsyE(learns), CT.None);
					return NIL;
				});

				await SvcWordV2.SoftDelPoWordInId(MkUserCtx(owner), AsyE(word.Id), CT.None);

				await RunNoTxn(async(Ctx)=>{
					var gotWord = await ToList(RepoWord.OrdGetByIdWithDel(Ctx, AsyE(word.Id), CT.None));
					Assert.IsTrue(gotWord.Count == 1 && gotWord[0] is not null && gotWord[0]!.IsDeleted(), "root should be soft-deleted");

					var gotProps = await ToList(RepoProp.GetInIdWithDel(Ctx, AsyE(props.Select(x=>x.Id).ToArray()), CT.None));
					Assert.IsTrue(gotProps.Count == props.Length && gotProps.All(x=>x is not null && !x!.IsDeleted()), "props should stay non-deleted");

					var gotLearns = await ToList(RepoLearn.GetInIdWithDel(Ctx, AsyE(learns.Select(x=>x.Id).ToArray()), CT.None));
					Assert.IsTrue(gotLearns.Count == learns.Length && gotLearns.All(x=>x is not null && !x!.IsDeleted()), "learns should stay non-deleted");
					return NIL;
				});
				return NIL;
			}
			finally{
				await RunNoTxn(async(Ctx)=>{
					await RepoProp.OrdHardDelById(Ctx, AsyE(props.Select(x=>x.Id).ToArray()), CT.None);
					await RepoLearn.OrdHardDelById(Ctx, AsyE(learns.Select(x=>x.Id).ToArray()), CT.None);
					await RepoWord.OrdHardDelById(Ctx, AsyE(word.Id), CT.None);
					return NIL;
				});
			}
		});
	}
}
