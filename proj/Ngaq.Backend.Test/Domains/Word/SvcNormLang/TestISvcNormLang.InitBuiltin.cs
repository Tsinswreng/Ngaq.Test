using Ngaq.Core.Shared.Dictionary.Models;
using Ngaq.Core.Shared.Dictionary.Models.Po.NormLang;
using Ngaq.Core.Shared.Word.Models.Dto;
using Ngaq.Core.Shared.Word.Svc;
using Tsinswreng.CsPage;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Backend.Test.Domains.Word;

public partial class TestISvcNormLang{
	void RegisterInitBuiltinNormLang(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestISvcNormLang),
			[typeof(ISvcNormLang)],
			[]
		);
		var R = register.Register;
		register.TesteeFnNames = [nameof(ISvcNormLang.InitBuiltinNormLang)];

		R("InitBuiltinNormLang_Should_InsertBuiltinRows_AndSkipOnSecondRun", async(o)=>{
			await SvcNormLang.InitBuiltinNormLang(MkUserCtx(_ownerInit), CT.None);
			await SvcNormLang.InitBuiltinNormLang(MkUserCtx(_ownerInit), CT.None);

			var rows = await ToList(SvcNormLang.OrdGetNormLangByTypeCode(
				MkUserCtx(_ownerInit),
				AsyE(
					(ELangIdentType.Bcp47, "en"),
					(ELangIdentType.Bcp47, "en-US")
				),
				CT.None
			));
			if(rows.Count != 2 || rows.Any(x=>x is null)){
				throw new Exception("InitBuiltinNormLang should ensure builtin rows exist");
			}
			if(rows.Any(x=>x!.Owner != _ownerInit)){
				throw new Exception("InitBuiltinNormLang should write rows for current user");
			}

			var page = await SvcNormLang.PageNormLang(
				MkUserCtx(_ownerInit),
				new ReqPageNormLang{
					PageQry = new PageQry{PageIdx = 0, PageSize = 20},
					SearchText = "en-US",
				},
				CT.None
			);
			var pageRows = await ToList(page.DataAsyE);
			var cnt = pageRows.Count(x=>x.Code == "en-US" && x.Type == ELangIdentType.Bcp47);
			if(cnt != 1){
				throw new Exception("InitBuiltinNormLang should skip conflicting builtin rows on second run");
			}

			foreach(var row in rows.Where(x=>x is not null).Select(x=>x!)){
				_ids.Add(row.Id);
			}
			return NIL;
		});
	}
}
