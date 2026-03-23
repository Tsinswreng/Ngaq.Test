using Ngaq.Core.Shared.StudyPlan.Models.Po.PreFilter;
using Ngaq.Core.Shared.StudyPlan.Models.Po.WeightArg;
using Ngaq.Core.Shared.StudyPlan.Models.Po.WeightCalculator;
using Ngaq.Core.Shared.StudyPlan.Models.Req;
using Tsinswreng.CsPage;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Local.Test.Domains.StudyPlan;

public partial class TestISvcStudyPlan{
	void RegisterBatAddPreFilter(ITestNode Node){
		var register = Node.MkTestFnRegister(typeof(TestISvcStudyPlan), [typeof(Ngaq.Core.Shared.StudyPlan.Svc.ISvcStudyPlan)], []);
		var R = register.Register;
		register.TesteeFnNames = [nameof(Ngaq.Core.Shared.StudyPlan.Svc.ISvcStudyPlan.BatAddPreFilter)];

		R("BatAddPreFilter_Should_ForceOwner_FromUser", async(o)=>{
			await RunNoTxn(async(Ctx)=>{
				var user = MkUser(_ownerA);
				var rows = new[]{
					new PoPreFilter{Id = new IdPreFilter(), Owner = _ownerB, UniqName = _token + "_svc_add_pf_1", Descr = "svc_pf_1", BizUpdatedAt = 9101},
					new PoPreFilter{Id = new IdPreFilter(), Owner = _ownerB, UniqName = _token + "_svc_add_pf_2", Descr = "svc_pf_2", BizUpdatedAt = 9102},
				};
				await SvcStudyPlan.BatAddPreFilter(Ctx, user, AsyE(rows), CT.None);
				_preFilterIds.AddRange(rows.Select(x=>x.Id));

				var page = await SvcStudyPlan.PagePreFilter(Ctx, new ReqPagePreFilter{
					Owner = _ownerA,
					PageQry = new PageQry{PageIdx = 0, PageSize = 20, WantTotCnt = false},
					UniqNameSearch = _token + "_svc_add_pf_",
				}, CT.None);
				var data = await ToList(page.DataAsyE);
				if(data.Count != 2 || data.Any(x=>x.Owner != _ownerA)){
					throw new Exception("BatAddPreFilter owner-injection assert failed");
				}
				return NIL;
			});
			return NIL;
		});
	}

	void RegisterBatAddWeightArg(ITestNode Node){
		var register = Node.MkTestFnRegister(typeof(TestISvcStudyPlan), [typeof(Ngaq.Core.Shared.StudyPlan.Svc.ISvcStudyPlan)], []);
		var R = register.Register;
		register.TesteeFnNames = [nameof(Ngaq.Core.Shared.StudyPlan.Svc.ISvcStudyPlan.BatAddWeightArg)];

		R("BatAddWeightArg_Should_ForceOwner_FromUser", async(o)=>{
			await RunNoTxn(async(Ctx)=>{
				var user = MkUser(_ownerA);
				var rows = new[]{
					new PoWeightArg{Id = new IdWeightArg(), Owner = _ownerB, UniqName = _token + "_svc_add_wa_1", Descr = "svc_wa_1", BizUpdatedAt = 9201},
					new PoWeightArg{Id = new IdWeightArg(), Owner = _ownerB, UniqName = _token + "_svc_add_wa_2", Descr = "svc_wa_2", BizUpdatedAt = 9202},
				};
				await SvcStudyPlan.BatAddWeightArg(Ctx, user, AsyE(rows), CT.None);
				_weightArgIds.AddRange(rows.Select(x=>x.Id));

				var page = await SvcStudyPlan.PageWeightArg(Ctx, new ReqPageWeightArg{
					Owner = _ownerA,
					PageQry = new PageQry{PageIdx = 0, PageSize = 20, WantTotCnt = false},
					UniqNameSearch = _token + "_svc_add_wa_",
				}, CT.None);
				var data = await ToList(page.DataAsyE);
				if(data.Count != 2 || data.Any(x=>x.Owner != _ownerA)){
					throw new Exception("BatAddWeightArg owner-injection assert failed");
				}
				return NIL;
			});
			return NIL;
		});
	}

	void RegisterBatAddWeightCalculator(ITestNode Node){
		var register = Node.MkTestFnRegister(typeof(TestISvcStudyPlan), [typeof(Ngaq.Core.Shared.StudyPlan.Svc.ISvcStudyPlan)], []);
		var R = register.Register;
		register.TesteeFnNames = [nameof(Ngaq.Core.Shared.StudyPlan.Svc.ISvcStudyPlan.BatAddWeightCalculator)];

		R("BatAddWeightCalculator_Should_ForceOwner_FromUser", async(o)=>{
			await RunNoTxn(async(Ctx)=>{
				var user = MkUser(_ownerA);
				var rows = new[]{
					new PoWeightCalculator{Id = new IdWeightCalculator(), Owner = _ownerB, UniqName = _token + "_svc_add_wc_1", Descr = "svc_wc_1"},
					new PoWeightCalculator{Id = new IdWeightCalculator(), Owner = _ownerB, UniqName = _token + "_svc_add_wc_2", Descr = "svc_wc_2"},
				};
				await SvcStudyPlan.BatAddWeightCalculator(Ctx, user, AsyE(rows), CT.None);
				_weightCalculatorIds.AddRange(rows.Select(x=>x.Id));

				var page = await SvcStudyPlan.PageWeightCalculator(Ctx, new ReqPageWeightCalculator{
					Owner = _ownerA,
					PageQry = new PageQry{PageIdx = 0, PageSize = 20, WantTotCnt = false},
					UniqNameSearch = _token + "_svc_add_wc_",
				}, CT.None);
				var data = await ToList(page.DataAsyE);
				if(data.Count != 2 || data.Any(x=>x.Owner != _ownerA)){
					throw new Exception("BatAddWeightCalculator owner-injection assert failed");
				}
				return NIL;
			});
			return NIL;
		});
	}
}
