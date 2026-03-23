using Ngaq.Core.Shared.StudyPlan.Models.Req;
using Tsinswreng.CsPage;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Local.Test.Domains.StudyPlan;

public partial class TestISvcStudyPlan{
	void RegisterPageStudyPlan(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestISvcStudyPlan)
			,[typeof(Ngaq.Core.Shared.StudyPlan.Svc.ISvcStudyPlan)]
			,[]
		);
		var R = register.Register;
		register.TesteeFnNames = [nameof(Ngaq.Core.Shared.StudyPlan.Svc.ISvcStudyPlan.PageStudyPlan)];

		R("PageStudyPlan_Search_By_UniqName", async(o)=>{
			await RunNoTxn(async(Ctx)=>{
				var req = new ReqPageStudyPlan{
					Owner = _ownerA,
					PageQry = new PageQry{PageIdx = 0, PageSize = 10, WantTotCnt = false},
					UniqNameSearch = _token + "_sp_a_",
				};
				var page = await SvcStudyPlan.PageStudyPlan(Ctx, req, CT.None);
				var data = await ToList(page.DataAsyE);
				if(data.Count != 2){
					throw new Exception($"PageStudyPlan search expected 2, got {data.Count}");
				}
				if(data.Any(x=>x.Owner != _ownerA)){
					throw new Exception("PageStudyPlan search contains wrong owner data");
				}
				return NIL;
			});
			return NIL;
		});

		R("PageStudyPlan_Search_NoMatch", async(o)=>{
			await RunNoTxn(async(Ctx)=>{
				var req = new ReqPageStudyPlan{
					Owner = _ownerA,
					PageQry = new PageQry{PageIdx = 0, PageSize = 10, WantTotCnt = false},
					UniqNameSearch = "__not_exist__" + _token,
				};
				var page = await SvcStudyPlan.PageStudyPlan(Ctx, req, CT.None);
				var data = await ToList(page.DataAsyE);
				if(data.Count != 0){
					throw new Exception($"PageStudyPlan no-match expected 0, got {data.Count}");
				}
				return NIL;
			});
			return NIL;
		});

		R("PageStudyPlan_Paging_Page0_Page1", async(o)=>{
			await RunNoTxn(async(Ctx)=>{
				var page0 = await SvcStudyPlan.PageStudyPlan(Ctx, new ReqPageStudyPlan{
					Owner = _ownerA,
					PageQry = new PageQry{PageIdx = 0, PageSize = 2, WantTotCnt = false},
				}, CT.None);
				var page0Data = await ToList(page0.DataAsyE);
				if(page0Data.Count != 2){
					throw new Exception($"PageStudyPlan page0 expected 2, got {page0Data.Count}");
				}

				var page1 = await SvcStudyPlan.PageStudyPlan(Ctx, new ReqPageStudyPlan{
					Owner = _ownerA,
					PageQry = new PageQry{PageIdx = 1, PageSize = 2, WantTotCnt = false},
				}, CT.None);
				var page1Data = await ToList(page1.DataAsyE);
				if(page1Data.Count != 1){
					throw new Exception($"PageStudyPlan page1 expected 1, got {page1Data.Count}");
				}
				return NIL;
			});
			return NIL;
		});
	}
}
