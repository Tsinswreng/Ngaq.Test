using Ngaq.Core.Shared.User.Models.Po.User;
using Ngaq.Core.Shared.Word.Models.Po.Word;
using Ngaq.Core.Shared.Word.Svc;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Local.Test.Domains.Word;

public partial class TestISvcWordV2{
	void RegisterBatAddNewWordToLearn(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestISvcWordV2)
			,[typeof(ISvcWordV2)]
			,[]
		);
		var R = register.Register;
		register.TesteeFnNames = [nameof(ISvcWordV2.BatAddNewWordToLearn)];

		R("BatAddNewWordToLearn_Should_Merge_SameHeadLang", async(o)=>{
			var owner = new IdUser();
			var token = "ut_wv2_add_" + Guid.NewGuid().ToString("N");
			var headDup = token + "_dup";
			var neoWords = new[]{
				new PoWord{Id = new IdWord(), Owner = default, Head = headDup, Lang = "en"},
				new PoWord{Id = new IdWord(), Owner = default, Head = headDup, Lang = "en"},
				new PoWord{Id = new IdWord(), Owner = default, Head = token + "_solo", Lang = "en"},
			};
			try{
				await SvcWordV2.BatAddNewWordToLearn(MkUserCtx(owner), AsyE(neoWords), CT.None);
				var all = await ToList(SvcWordV2.GetWordsToLearn(MkUserCtx(owner), CT.None));
				var tokenWords = all.Where(x=>x.Word.Head.StartsWith(token)).ToList();
				var dupCnt = tokenWords.Count(x=>x.Word.Head == headDup && x.Word.Lang == "en");
				if(dupCnt != 1){
					throw new Exception($"duplicate merge assert failed, expected 1, got {dupCnt}");
				}
				if(tokenWords.Count != 2){
					throw new Exception($"expected 2 words after merge, got {tokenWords.Count}");
				}
				if(tokenWords.Any(x=>x.Word.Owner != owner)){
					throw new Exception("owner injection assert failed");
				}
				return NIL;
			}
			finally{
				await TryCleanupByHeadOwner(owner, token);
			}
		});

		R("BatAddNewWordToLearn_SameHeadDifferentLang_Should_NotMerge", async(o)=>{
			var owner = new IdUser();
			var token = "ut_wv2_add_lang_" + Guid.NewGuid().ToString("N");
			var head = token + "_head";
			var neoWords = new[]{
				new PoWord{Id = new IdWord(), Owner = default, Head = head, Lang = "en"},
				new PoWord{Id = new IdWord(), Owner = default, Head = head, Lang = "ja"},
			};
			try{
				await SvcWordV2.BatAddNewWordToLearn(MkUserCtx(owner), AsyE(neoWords), CT.None);
				var all = await ToList(SvcWordV2.GetWordsToLearn(MkUserCtx(owner), CT.None));
				var tokenWords = all.Where(x=>x.Word.Head == head).ToList();
				if(tokenWords.Count != 2){
					throw new Exception($"same-head-diff-lang should not merge, got {tokenWords.Count}");
				}
				return NIL;
			}
			finally{
				await TryCleanupByHeadOwner(owner, token);
			}
		});

		R("BatAddNewWordToLearn_WhenInputEmpty_Should_NoThrow", async(o)=>{
			var owner = new IdUser();
			await SvcWordV2.BatAddNewWordToLearn(MkUserCtx(owner), AsyE<PoWord>(), CT.None);
			return NIL;
		});
	}
}
