using Ngaq.Core.Model.Po.Learn_;
using Ngaq.Core.Model.Po.Kv;
using Ngaq.Core.Shared.User.Models.Po.User;
using Ngaq.Core.Shared.Word.Models.Learn_;
using Ngaq.Core.Shared.Word.Models.Po.Kv;
using Ngaq.Core.Shared.Word.Models.Po.Learn;
using Ngaq.Core.Shared.Word.Models.Po.Word;
using Ngaq.Core.Shared.Word.Svc;
using Tsinswreng.CsSql;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Local.Test.Domains.Word;

public partial class TestISvcWordV2{
	void RegisterBatChangeId(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestISvcWordV2),
			[typeof(ISvcWordV2)],
			[]
		);
		var R = register.Register;
		register.TesteeFnNames = [nameof(ISvcWordV2.BatChangeId)];

		R("BatChangeId_Should_ChangeRootId_And_MoveAssetForeignKeys", async(o)=>{
			var owner = new IdUser();
			var token = "ut_wv2_chgid_" + Guid.NewGuid().ToString("N");
			var oldId = new IdWord();
			var newId = new IdWord();
			var word = new PoWord{Id = oldId, Owner = owner, Head = token + "_w", Lang = "en"};
			var prop = new PoWordProp{
				Id = new IdWordProp(),
				WordId = oldId,
				KType = EKvType.Str,
				KStr = KeysProp.Inst.description,
				VType = EKvType.Str,
				VStr = token + "_d1",
			};
			var learn = new PoWordLearn{
				Id = new IdWordLearn(),
				WordId = oldId,
				LearnResult = ELearn.Add,
			};
			try{
				await RunNoTxn(async(Ctx)=>{
					await RepoWord.BatAdd(Ctx, AsyE(word), CT.None);
					await RepoProp.BatAdd(Ctx, AsyE(prop), CT.None);
					await RepoLearn.BatAdd(Ctx, AsyE(learn), CT.None);
					return NIL;
				});

				await SvcWordV2.BatChangeId(MkUserCtx(owner), AsyE((oldId, newId)), CT.None);

				await RunNoTxn(async(Ctx)=>{
					var oldWord = await ToList(RepoWord.BatGetByIdWithDel(Ctx, AsyE(oldId), CT.None));
					if(oldWord.Count != 1 || oldWord[0] is not null){
						throw new Exception("BatChangeId should remove old root id");
					}

					var newWord = await ToList(RepoWord.BatGetByIdWithDel(Ctx, AsyE(newId), CT.None));
					if(newWord.Count != 1 || newWord[0] is null || newWord[0]!.Owner != owner){
						throw new Exception("BatChangeId should create root row with new id");
					}

					var props = (await ToList(RepoProp.GetAll(Ctx, CT.None)))
						.Where(x=>x.Id == prop.Id)
						.ToList();
					if(props.Count != 1 || props[0].WordId != newId){
						throw new Exception("BatChangeId should move PoWordProp.WordId to new id");
					}

					var learns = (await ToList(RepoLearn.GetAll(Ctx, CT.None)))
						.Where(x=>x.Id == learn.Id)
						.ToList();
					if(learns.Count != 1 || learns[0].WordId != newId){
						throw new Exception("BatChangeId should move PoWordLearn.WordId to new id");
					}
					return NIL;
				});
				return NIL;
			}
			finally{
				await RunNoTxn(async(Ctx)=>{
					await RepoProp.BatHardDelById(Ctx, AsyE(prop.Id), CT.None);
					await RepoLearn.BatHardDelById(Ctx, AsyE(learn.Id), CT.None);
					await RepoWord.BatHardDelById(Ctx, AsyE(oldId, newId), CT.None);
					return NIL;
				});
			}
		});

		R("BatChangeId_WhenEmptyInput_Should_NoThrow", async(o)=>{
			await SvcWordV2.BatChangeId(MkUserCtx(new IdUser()), AsyE<(IdWord Old, IdWord New)>(), CT.None);
			return NIL;
		});
	}
}
