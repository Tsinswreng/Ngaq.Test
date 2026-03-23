using Ngaq.Core.Shared.StudyPlan.Models.Req;
using Tsinswreng.CsPage;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Local.Test.Domains.StudyPlan;

public partial class TestISvcStudyPlan{
	void RegisterPageWeightArg(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestISvcStudyPlan)
			,[typeof(Ngaq.Core.Shared.StudyPlan.Svc.ISvcStudyPlan)]
			,[]
		);
		var R = register.Register;
		register.TesteeFnNames = [nameof(Ngaq.Core.Shared.StudyPlan.Svc.ISvcStudyPlan.PageWeightArg)];

		R("PageWeightArg_Search_By_UniqName", async(o)=>{
			await RunNoTxn(async(Ctx)=>{
				var req = new ReqPageWeightArg{
					Owner = _ownerA,
					PageQry = new PageQry{PageIdx = 0, PageSize = 10, WantTotCnt = false},
					UniqNameSearch = _token + "_wa_a_",
				};
				var page = await SvcStudyPlan.PageWeightArg(Ctx, req, CT.None);
				var data = await ToList(page.DataAsyE);
				if(data.Count != 2){
					throw new Exception($"PageWeightArg search expected 2, got {data.Count}");
				}
				if(data.Any(x=>x.Owner != _ownerA)){
					throw new Exception("PageWeightArg search contains wrong owner data");
				}
				return NIL;
			});
			return NIL;
		});

		R("PageWeightArg_Paging_Page0_Page1", async(o)=>{
			await RunNoTxn(async(Ctx)=>{
				var page0 = await SvcStudyPlan.PageWeightArg(Ctx, new ReqPageWeightArg{
					Owner = _ownerA,
					PageQry = new PageQry{PageIdx = 0, PageSize = 2, WantTotCnt = false},
				}, CT.None);
				var page0Data = await ToList(page0.DataAsyE);
				if(page0Data.Count != 2){
					throw new Exception($"PageWeightArg page0 expected 2, got {page0Data.Count}");
				}

				var page1 = await SvcStudyPlan.PageWeightArg(Ctx, new ReqPageWeightArg{
					Owner = _ownerA,
					PageQry = new PageQry{PageIdx = 1, PageSize = 2, WantTotCnt = false},
				}, CT.None);
				var page1Data = await ToList(page1.DataAsyE);
				if(page1Data.Count != 1){
					throw new Exception($"PageWeightArg page1 expected 1, got {page1Data.Count}");
				}
				return NIL;
			});
			return NIL;
		});
	}
}
