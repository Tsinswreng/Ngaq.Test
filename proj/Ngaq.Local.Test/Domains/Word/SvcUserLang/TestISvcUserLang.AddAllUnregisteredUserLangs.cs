using Ngaq.Core.Infra;
using Ngaq.Core.Shared.Dictionary.Models;
using Ngaq.Core.Shared.Word.Models.Po.UserLang;
using Ngaq.Core.Shared.Word.Models.Po.Word;
using Ngaq.Core.Shared.Word.Svc;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Local.Test.Domains.Word;

public partial class TestISvcUserLang{
	void RegisterAddAllUnregisteredUserLangs(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestISvcUserLang)
			,[typeof(ISvcUserLang)]
			,[]
		);
		var R = register.Register;
		register.TesteeFnNames = [nameof(ISvcUserLang.AddAllUnregisteredUserLangs)];

		R("AddAllUnregisteredUserLangs_Should_AddMissingOnly_AndBeIdempotent", async(o)=>{
			var langA = _token + "_addall_a";
			var langB = _token + "_addall_b";
			var words = new[]{
				new PoWord{Id = new IdWord(), Owner = _ownerA, Head = _token + "_addall_w1", Lang = langA},
				new PoWord{Id = new IdWord(), Owner = _ownerA, Head = _token + "_addall_w2", Lang = langB},
				new PoWord{Id = new IdWord(), Owner = _ownerA, Head = _token + "_addall_w3", Lang = langB},
			};
			var existing = new PoUserLang{
				Id = new IdUserLang(),
				Owner = _ownerA,
				UniqName = langA,
				RelLangType = ELangIdentType.Bcp47,
				RelLang = langA,
			};
			await RunNoTxn(async(Ctx)=>{
				await RepoWord.BatAdd(Ctx, AsyE(words), CT.None);
				await RepoUserLang.BatAdd(Ctx, AsyE(existing), CT.None);
				return NIL;
			});
			_wordIds.AddRange(words.Select(x=>x.Id));
			_userLangIds.Add(existing.Id);

			var userCtx = MkUserCtx(_ownerA);
			await SvcUserLang.AddAllUnregisteredUserLangs(userCtx, CT.None);
			await SvcUserLang.AddAllUnregisteredUserLangs(userCtx, CT.None);

			await RunNoTxn(async(Ctx)=>{
				var allRows = await ToList(RepoUserLang.GetAll(Ctx, CT.None));
				var tokenRows = allRows
					.Where(x=>x.Owner == _ownerA && x.UniqName is not null && x.UniqName.StartsWith(_token + "_addall_"))
					.ToList();
				foreach(var row in tokenRows){
					_userLangIds.Add(row.Id);
				}

				if(tokenRows.Count != 2){
					throw new Exception("AddAllUnregisteredUserLangs should add exactly one missing language");
				}
				var uniqs = tokenRows.Select(x=>x.UniqName).Where(x=>x is not null).Cast<str>().OrderBy(x=>x).ToArray();
				if(uniqs.Length != 2 || uniqs[0] != langA || uniqs[1] != langB){
					throw new Exception("AddAllUnregisteredUserLangs languages mismatch");
				}
				var addedB = tokenRows.FirstOrDefault(x=>x.UniqName == langB);
				if(addedB is null){
					throw new Exception("AddAllUnregisteredUserLangs should add missing language row");
				}
				if(addedB.RelLangType != ELangIdentType.Bcp47 || addedB.RelLang != langB){
					throw new Exception("AddAllUnregisteredUserLangs should map RelLangType/RelLang by default");
				}
				if(addedB.BizUpdatedAt <= Tempus.Zero){
					throw new Exception("AddAllUnregisteredUserLangs should touch BizUpdatedAt");
				}
				return NIL;
			});
			return NIL;
		});
	}
}
