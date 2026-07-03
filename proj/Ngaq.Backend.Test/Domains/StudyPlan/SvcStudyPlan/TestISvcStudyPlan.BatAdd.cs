using Ngaq.Core.Shared.StudyPlan.Models.Po.PreFilter;
using Ngaq.Core.Shared.StudyPlan.Models.Po.WeightArg;
using Ngaq.Core.Shared.StudyPlan.Models.Po.WeightCalculator;
using Ngaq.Core.Shared.StudyPlan.Models.Req;
using Tsinswreng.CsPage;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Backend.Test.Domains.StudyPlan;

public partial class TestISvcStudyPlan{
	void RegisterBatAddPreFilter(ITestNode Node){
		var register = Node.MkTestFnRegister(typeof(TestISvcStudyPlan), [typeof(Ngaq.Core.Shared.StudyPlan.Svc.ISvcStudyPlan)], []);
		var R = register.Register;
		register.TesteeFnNames = [nameof(Ngaq.Core.Shared.StudyPlan.Svc.ISvcStudyPlan.OrdAddPreFilter)];

		R("BatAddPreFilter_Should_ForceOwner_FromUser", async(o)=>{
			var userCtx = MkUserCtx(_ownerA);
			var rows = new[]{
				new PoPreFilter{Id = new IdPreFilter(), Owner = _ownerB, UniqName = _token + "_svc_add_pf_1", Descr = "svc_pf_1", BizUpdatedAt = 9101},
				new PoPreFilter{Id = new IdPreFilter(), Owner = _ownerB, UniqName = _token + "_svc_add_pf_2", Descr = "svc_pf_2", BizUpdatedAt = 9102},
			};
			await SvcStudyPlan.OrdAddPreFilter(userCtx, AsyE(rows), CT.None);
			_preFilterIds.AddRange(rows.Select(x=>x.Id));

			var page = await SvcStudyPlan.PagePreFilter(userCtx, new ReqPagePreFilter{
				PageQry = new PageQry{PageIdx = 0, PageSize = 20, WantTotCnt = false},
				UniqNameSearch = _token + "_svc_add_pf_",
			}, CT.None);
			var data = await ToList(page.DataAsyE);
			if(data.Count != 2 || data.Any(x=>x.Owner != _ownerA)){
				throw new Exception("BatAddPreFilter owner-injection assert failed");
			}
			return NIL;
		});
	}

	void RegisterBatAddWeightArg(ITestNode Node){
		var register = Node.MkTestFnRegister(typeof(TestISvcStudyPlan), [typeof(Ngaq.Core.Shared.StudyPlan.Svc.ISvcStudyPlan)], []);
		var R = register.Register;
		register.TesteeFnNames = [nameof(Ngaq.Core.Shared.StudyPlan.Svc.ISvcStudyPlan.OrdAddWeightArg)];

		R("BatAddWeightArg_Should_ForceOwner_FromUser", async(o)=>{
			var userCtx = MkUserCtx(_ownerA);
			var rows = new[]{
				new PoWeightArg{Id = new IdWeightArg(), Owner = _ownerB, UniqName = _token + "_svc_add_wa_1", Descr = "svc_wa_1", BizUpdatedAt = 9201},
				new PoWeightArg{Id = new IdWeightArg(), Owner = _ownerB, UniqName = _token + "_svc_add_wa_2", Descr = "svc_wa_2", BizUpdatedAt = 9202},
			};
			await SvcStudyPlan.OrdAddWeightArg(userCtx, AsyE(rows), CT.None);
			_weightArgIds.AddRange(rows.Select(x=>x.Id));

			var page = await SvcStudyPlan.PageWeightArg(userCtx, new ReqPageWeightArg{
				PageQry = new PageQry{PageIdx = 0, PageSize = 20, WantTotCnt = false},
				UniqNameSearch = _token + "_svc_add_wa_",
			}, CT.None);
			var data = await ToList(page.DataAsyE);
			if(data.Count != 2 || data.Any(x=>x.Owner != _ownerA)){
				throw new Exception("BatAddWeightArg owner-injection assert failed");
			}
			return NIL;
		});
	}

	void RegisterBatAddWeightCalculator(ITestNode Node){
		var register = Node.MkTestFnRegister(typeof(TestISvcStudyPlan), [typeof(Ngaq.Core.Shared.StudyPlan.Svc.ISvcStudyPlan)], []);
		var R = register.Register;
		register.TesteeFnNames = [nameof(Ngaq.Core.Shared.StudyPlan.Svc.ISvcStudyPlan.OrdAddWeightCalculator)];

		R("BatAddWeightCalculator_Should_ForceOwner_FromUser", async(o)=>{
			var userCtx = MkUserCtx(_ownerA);
			var rows = new[]{
				new PoWeightCalculator{Id = new IdWeightCalculator(), Owner = _ownerB, UniqName = _token + "_svc_add_wc_1", Descr = "svc_wc_1"},
				new PoWeightCalculator{Id = new IdWeightCalculator(), Owner = _ownerB, UniqName = _token + "_svc_add_wc_2", Descr = "svc_wc_2"},
			};
			await SvcStudyPlan.OrdAddWeightCalculator(userCtx, AsyE(rows), CT.None);
			_weightCalculatorIds.AddRange(rows.Select(x=>x.Id));

			var page = await SvcStudyPlan.PageWeightCalculator(userCtx, new ReqPageWeightCalculator{
				PageQry = new PageQry{PageIdx = 0, PageSize = 20, WantTotCnt = false},
				UniqNameSearch = _token + "_svc_add_wc_",
			}, CT.None);
			var data = await ToList(page.DataAsyE);
			if(data.Count != 2 || data.Any(x=>x.Owner != _ownerA)){
				throw new Exception("BatAddWeightCalculator owner-injection assert failed");
			}
			return NIL;
		});
	}
}
