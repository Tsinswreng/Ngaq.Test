using Ngaq.Core.Shared.Word.Models.Dto;
using Ngaq.Core.Shared.Word.Svc;
using Tsinswreng.CsPage;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Local.Test.Domains.Word;

public partial class TestISvcNormLangToUserLang{
	void RegisterPageNormLangToUserLang(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestISvcNormLangToUserLang),
			[typeof(ISvcNormLangToUserLang)],
			[]
		);
		var R = register.Register;
		register.TesteeFnNames = [nameof(ISvcNormLangToUserLang.PageNormLangToUserLang)];

		R("PageNormLangToUserLang_Should_FilterByOwnerAndUserLang_AndOrderByBizUpdatedAtDesc", async(o)=>{
			var req = new ReqPageNormLangToUserLang{
				PageQry = new PageQry{
					PageIdx = 0,
					PageSize = 20,
				},
				UserLang = _token + "_user_",
			};
			var page = await SvcNormLangToUserLang.PageNormLangToUserLang(MkUserCtx(_ownerA), req, CT.None);
			var rows = await ToList(page.DataAsyE);
			if(rows.Count != 2){
				throw new Exception("PageNormLangToUserLang should return 2 owner rows after UserLang filter");
			}
			if(rows.Any(x=>x.Owner != _ownerA)){
				throw new Exception("PageNormLangToUserLang should isolate by owner");
			}
			if(rows.Any(x=>!x.UserLang.Contains(_token + "_user_"))){
				throw new Exception("PageNormLangToUserLang should apply UserLang search");
			}
			for(var i = 1; i < rows.Count; i++){
				if(rows[i - 1].BizUpdatedAt < rows[i].BizUpdatedAt){
					throw new Exception("PageNormLangToUserLang should sort by BizUpdatedAt desc");
				}
			}
			return NIL;
		});
	}
}
