using Ngaq.Core.Shared.StudyPlan.Models.Req;
using Tsinswreng.CsPage;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Local.Test.Domains.StudyPlan;

public partial class TestISvcStudyPlan{
	void RegisterPagePreFilter(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestISvcStudyPlan)
			,[typeof(Ngaq.Core.Shared.StudyPlan.Svc.ISvcStudyPlan)]
			,[]
		);
		var R = register.Register;
		register.TesteeFnNames = [nameof(Ngaq.Core.Shared.StudyPlan.Svc.ISvcStudyPlan.PagePreFilter)];

		R("PagePreFilter_Search_By_UniqName", async(o)=>{
			await RunNoTxn(async(Ctx)=>{
				var req = new ReqPagePreFilter{
					Owner = _ownerA,
					PageQry = new PageQry{PageIdx = 0, PageSize = 10, WantTotCnt = false},
					UniqNameSearch = _token + "_pf_a_",
				};
				var page = await SvcStudyPlan.PagePreFilter(Ctx, req, CT.None);
				var data = await ToList(page.DataAsyE);
				if(data.Count != 2){
					throw new Exception($"PagePreFilter search expected 2, got {data.Count}");
				}
				if(data.Any(x=>x.Owner != _ownerA)){
					throw new Exception("PagePreFilter search contains wrong owner data");
				}
				return NIL;
			});
			return NIL;
		});

		R("PagePreFilter_Paging_Page0_Page1", async(o)=>{
			await RunNoTxn(async(Ctx)=>{
				var page0 = await SvcStudyPlan.PagePreFilter(Ctx, new ReqPagePreFilter{
					Owner = _ownerA,
					PageQry = new PageQry{PageIdx = 0, PageSize = 2, WantTotCnt = false},
				}, CT.None);
				var page0Data = await ToList(page0.DataAsyE);
				if(page0Data.Count != 2){
					throw new Exception($"PagePreFilter page0 expected 2, got {page0Data.Count}");
				}

				var page1 = await SvcStudyPlan.PagePreFilter(Ctx, new ReqPagePreFilter{
					Owner = _ownerA,
					PageQry = new PageQry{PageIdx = 1, PageSize = 2, WantTotCnt = false},
				}, CT.None);
				var page1Data = await ToList(page1.DataAsyE);
				if(page1Data.Count != 1){
					throw new Exception($"PagePreFilter page1 expected 1, got {page1Data.Count}");
				}
				return NIL;
			});
			return NIL;
		});
	}
}
