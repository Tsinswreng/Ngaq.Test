using Ngaq.Core.Shared.Dictionary.Models;
using Ngaq.Core.Shared.Word.Models.Po.UserLang;
using Ngaq.Core.Shared.Word.Models.Po.Word;
using Ngaq.Core.Shared.Word.Svc;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Local.Test.Domains.Word;

public partial class TestISvcUserLang{
	void RegisterGetUnregisteredUserLangs(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestISvcUserLang)
			,[typeof(ISvcUserLang)]
			,[]
		);
		var R = register.Register;
		register.TesteeFnNames = [nameof(ISvcUserLang.GetUnregisteredUserLangs)];

		R("GetUnregisteredUserLangs_Should_ReturnDistinctMissingLangs", async(o)=>{
			var langEn = _token + "_ureg_en";
			var langFr = _token + "_ureg_fr";
			var langJp = _token + "_ureg_jp";
			var langDeOther = _token + "_ureg_de_other";

			var words = new[]{
				new PoWord{Id = new IdWord(), Owner = _ownerA, Head = _token + "_ureg_w1", Lang = langEn},
				new PoWord{Id = new IdWord(), Owner = _ownerA, Head = _token + "_ureg_w2", Lang = langFr},
				new PoWord{Id = new IdWord(), Owner = _ownerA, Head = _token + "_ureg_w3", Lang = langFr},
				new PoWord{Id = new IdWord(), Owner = _ownerA, Head = _token + "_ureg_w4", Lang = langJp},
				new PoWord{Id = new IdWord(), Owner = _ownerB, Head = _token + "_ureg_w5", Lang = langDeOther},
			};
			var existing = new PoUserLang{
				Id = new IdUserLang(),
				Owner = _ownerA,
				UniqName = langEn,
				RelLangType = ELangIdentType.Bcp47,
				RelLang = langEn,
			};

			await RunNoTxn(async(Ctx)=>{
				await RepoWord.BatAdd(Ctx, AsyE(words), CT.None);
				await RepoUserLang.BatAdd(Ctx, AsyE(existing), CT.None);
				return NIL;
			});
			_wordIds.AddRange(words.Select(x=>x.Id));
			_userLangIds.Add(existing.Id);

			var got = await ToList(SvcUserLang.GetUnregisteredUserLangs(MkUserCtx(_ownerA), CT.None));
			var filtered = got.Where(x=>x.StartsWith(_token + "_ureg_")).OrderBy(x=>x).ToList();
			if(filtered.Count != 2){
				throw new Exception("GetUnregisteredUserLangs should return only missing distinct langs");
			}
			if(filtered[0] != langFr || filtered[1] != langJp){
				throw new Exception("GetUnregisteredUserLangs result mismatch");
			}
			return NIL;
		});
	}
}
