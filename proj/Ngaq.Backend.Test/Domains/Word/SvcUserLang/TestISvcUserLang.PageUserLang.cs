using Ngaq.Core.Shared.Word.Models.Dto;
using Ngaq.Core.Shared.Word.Svc;
using Tsinswreng.CsPage;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Backend.Test.Domains.Word;

public partial class TestISvcUserLang{
	void RegisterPageUserLang(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestISvcUserLang)
			,[typeof(ISvcUserLang)]
			,[]
		);
		var R = register.Register;
		register.TesteeFnNames = [nameof(ISvcUserLang.PageUserLang)];

		R("PageUserLang_Should_FilterByOwnerAndSearch_And_OrderByBizUpdatedAtDesc", async(o)=>{
			var req = new ReqPageUserLang{
				PageQry = new PageQry{
					PageIdx = 0,
					PageSize = 20,
				},
				UniqNameSearch = _token + "_page_a_",
			};
			var page = await SvcUserLang.PageUserLang(MkUserCtx(_ownerA), req, CT.None);
			var rows = await ToList(page.DataAsyE);
			if(rows.Count != 3){
				throw new Exception("PageUserLang should return 3 owner rows after search filter");
			}
			if(rows.Any(x=>x.Owner != _ownerA)){
				throw new Exception("PageUserLang should isolate by owner");
			}
			if(rows.Any(x=>x.UniqName is null || !x.UniqName.Contains(_token + "_page_a_"))){
				throw new Exception("PageUserLang should apply UniqNameSearch");
			}
			for(var i = 1; i < rows.Count; i++){
				if(rows[i - 1].BizUpdatedAt < rows[i].BizUpdatedAt){
					throw new Exception("PageUserLang should sort by BizUpdatedAt desc");
				}
			}
			return NIL;
		});
	}
}
