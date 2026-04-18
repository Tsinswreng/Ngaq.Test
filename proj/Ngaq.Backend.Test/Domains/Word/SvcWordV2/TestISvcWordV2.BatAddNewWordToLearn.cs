using Ngaq.Core.Shared.User.Models.Po.User;
using Ngaq.Core.Model.Po.Kv;
using Ngaq.Core.Shared.Word.Models;
using Ngaq.Core.Shared.Word.Models.Learn_;
using Ngaq.Core.Shared.Word.Models.Po.Kv;
using Ngaq.Core.Shared.Word.Models.Po.Word;
using Ngaq.Core.Shared.Word.Svc;
using Tsinswreng.CsSql;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Backend.Test.Domains.Word;

public partial class TestISvcWordV2{
	void RegisterBatAddNewWordToLearn(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestISvcWordV2)
			,[typeof(ISvcWordV2)]
			,[]
		);
		var R = register.Register;
		register.TesteeFnNames = [nameof(ISvcWordV2.BatAddNewWordToLearn)];

		R("BatAddNewWordToLearn_Should_InsertNewWords_And_AddLearnsByDescriptionCount", async(o)=>{
			var owner = new IdUser();
			var token = "ut_wv2_add_new_" + Guid.NewGuid().ToString("N");
			var w1 = MkInputWord(token + "_w1", "en", [
				(K: KeysProp.Inst.description, V: token + "_w1_d1"),
				(K: KeysProp.Inst.description, V: token + "_w1_d2"),
				(K: KeysProp.Inst.note, V: token + "_w1_n1"),
			]);
			var w2 = MkInputWord(token + "_w2", "en", [
				(K: KeysProp.Inst.description, V: token + "_w2_d1"),
			]);

			try{
				await SvcWordV2.BatAddNewWordToLearn(MkUserCtx(owner), AsyE(w1, w2), CT.None);

				await RunNoTxn(async(Ctx)=>{
					var words = (await ToList(RepoWord.GetAll(Ctx, CT.None)))
						.Where(x=>x.Owner == owner && x.Head.StartsWith(token))
						.ToList();
					if(words.Count != 2){
						throw new Exception("should insert 2 new words");
					}

					var wordIds = words.Select(x=>x.Id).ToHashSet();
					var props = (await ToList(RepoProp.GetAll(Ctx, CT.None)))
						.Where(x=>wordIds.Contains(x.WordId))
						.ToList();
					var learns = (await ToList(RepoLearn.GetAll(Ctx, CT.None)))
						.Where(x=>wordIds.Contains(x.WordId))
						.ToList();

					var descCnt = props.Count(x=>x.KStr == KeysProp.Inst.description);
					if(descCnt != 3){
						throw new Exception("description prop count mismatch after insert");
					}
					if(learns.Count(x=>x.LearnResult == ELearn.Add) != 3){
						throw new Exception("ELearn.Add count should equal description prop count");
					}
					return NIL;
				});
				return NIL;
			}
			finally{
				await TryCleanupByHeadOwner(owner, token);
			}
		});

		R("BatAddNewWordToLearn_WhenExistingWord_Should_MergeProps_And_AddOnlyNewDescriptionLearns", async(o)=>{
			var owner = new IdUser();
			var token = "ut_wv2_add_merge_" + Guid.NewGuid().ToString("N");
			var head = token + "_w";
			var oldWord = new PoWord{Id = new IdWord(), Owner = owner, Head = head, Lang = "en"};
			var oldProp = new PoWordProp{
				Id = new IdWordProp(),
				WordId = oldWord.Id,
				KStr = KeysProp.Inst.description,
				VType = EKvType.Str,
				VStr = token + "_d0",
			};

			var input = MkInputWord(head, "en", [
				(K: KeysProp.Inst.description, V: token + "_d0"), // 重複
				(K: KeysProp.Inst.description, V: token + "_d1"), // 新增
				(K: KeysProp.Inst.note, V: token + "_n1"),
			]);

			try{
				await RunNoTxn(async(Ctx)=>{
					await RepoWord.BatAdd(Ctx, AsyE(oldWord), CT.None);
					await RepoProp.BatAdd(Ctx, AsyE(oldProp), CT.None);
					return NIL;
				});

				await SvcWordV2.BatAddNewWordToLearn(MkUserCtx(owner), AsyE(input), CT.None);

				await RunNoTxn(async(Ctx)=>{
					var words = (await ToList(RepoWord.GetAll(Ctx, CT.None)))
						.Where(x=>x.Owner == owner && x.Head == head && x.Lang == "en")
						.ToList();
					if(words.Count != 1){
						throw new Exception("existing word should be merged instead of duplicated insert");
					}

					var wordId = words[0].Id;
					var props = (await ToList(RepoProp.GetAll(Ctx, CT.None)))
						.Where(x=>x.WordId == wordId)
						.ToList();
					var learns = (await ToList(RepoLearn.GetAll(Ctx, CT.None)))
						.Where(x=>x.WordId == wordId)
						.ToList();

					var d0Cnt = props.Count(x=>x.KStr == KeysProp.Inst.description && x.VStr == token + "_d0");
					var d1Cnt = props.Count(x=>x.KStr == KeysProp.Inst.description && x.VStr == token + "_d1");
					if(d0Cnt != 1 || d1Cnt != 1){
						throw new Exception("existing-word merge should keep old desc and append only new desc");
					}
					if(learns.Count(x=>x.LearnResult == ELearn.Add) != 1){
						throw new Exception("existing-word merge should add ELearn.Add only for truly new descriptions");
					}
					return NIL;
				});
				return NIL;
			}
			finally{
				await TryCleanupByHeadOwner(owner, token);
			}
		});

		R("BatAddNewWordToLearn_SameBatchDuplicateHeadLang_Should_BeMergedInBatch", async(o)=>{
			var owner = new IdUser();
			var token = "ut_wv2_add_dupli_" + Guid.NewGuid().ToString("N");
			var head = token + "_w";
			var a = MkInputWord(head, "en", [
				(K: KeysProp.Inst.description, V: token + "_a_d1"),
			]);
			var b = MkInputWord(head, "en", [
				(K: KeysProp.Inst.description, V: token + "_b_d1"),
			]);

			try{
				await SvcWordV2.BatAddNewWordToLearn(MkUserCtx(owner), AsyE(a, b), CT.None);
				await RunNoTxn(async(Ctx)=>{
					var words = (await ToList(RepoWord.GetAll(Ctx, CT.None)))
						.Where(x=>x.Owner == owner && x.Head == head && x.Lang == "en")
						.ToList();
					if(words.Count != 1){
						throw new Exception("same batch duplicate (Head,Lang) should not create duplicate roots");
					}

					var wordId = words[0].Id;
					var props = (await ToList(RepoProp.GetAll(Ctx, CT.None)))
						.Where(x=>x.WordId == wordId && x.KStr == KeysProp.Inst.description)
						.ToList();
					var learns = (await ToList(RepoLearn.GetAll(Ctx, CT.None)))
						.Where(x=>x.WordId == wordId && x.LearnResult == ELearn.Add)
						.ToList();

					if(props.Count != 2 || learns.Count != 2){
						throw new Exception("same-batch merged word should keep both new descriptions and add matching learns");
					}
					return NIL;
				});
				return NIL;
			}
			finally{
				await TryCleanupByHeadOwner(owner, token);
			}
		});


	static JnWord MkInputWord(
		str Head,
		str Lang,
		IEnumerable<(str K, str V)> Kvs
	){
		var w = new JnWord{
			Word = new PoWord{
				Id = new IdWord(),
				Head = Head,
				Lang = Lang,
			},
		};
		w.Props = Kvs.Select(x=>new PoWordProp{
			Id = new IdWordProp(),
			KType = EKvType.Str,
			KStr = x.K,
			VType = EKvType.Str,
			VStr = x.V,
		}).ToList();
		w.Learns = [];
		return w;
	}
	}
}
