using Ngaq.Core.Shared.Word.Models.Dto;
using Ngaq.Core.Shared.Word.Svc;
using Tsinswreng.CsPage;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Local.Test.Domains.Word;

public partial class TestISvcNormLang{
	void RegisterPageNormLang(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestISvcNormLang),
			[typeof(ISvcNormLang)],
			[]
		);
		var R = register.Register;
		register.TesteeFnNames = [nameof(ISvcNormLang.PageNormLang)];

		R("PageNormLang_Should_FilterByOwnerAndCode_AndOrderByBizUpdatedAtDesc", async(o)=>{
			var req = new ReqPageNormLang{
				PageQry = new PageQry{
					PageIdx = 0,
					PageSize = 20,
				},
				Code = _token + "_",
			};
			var page = await SvcNormLang.PageNormLang(MkUserCtx(_ownerA), req, CT.None);
			var rows = await ToList(page.DataAsyE);
			if(rows.Count != 3){
				throw new Exception("PageNormLang should return 3 owner rows after code filter");
			}
			if(rows.Any(x=>x.Owner != _ownerA)){
				throw new Exception("PageNormLang should isolate by owner");
			}
			if(rows.Any(x=>!x.Code.Contains(_token + "_"))){
				throw new Exception("PageNormLang should apply code search");
			}
			for(var i = 1; i < rows.Count; i++){
				if(rows[i - 1].BizUpdatedAt < rows[i].BizUpdatedAt){
					throw new Exception("PageNormLang should sort by BizUpdatedAt desc");
				}
			}
			return NIL;
		});
	}
}
