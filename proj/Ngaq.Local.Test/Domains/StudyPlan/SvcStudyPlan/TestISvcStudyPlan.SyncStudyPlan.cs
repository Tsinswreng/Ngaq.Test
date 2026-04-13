using Ngaq.Core.Shared.StudyPlan.Models.Po.PreFilter;
using Ngaq.Core.Shared.StudyPlan.Models.Po.StudyPlan;
using Ngaq.Core.Shared.StudyPlan.Models.Po.WeightArg;
using Ngaq.Core.Shared.StudyPlan.Models.Po.WeightCalculator;
using Ngaq.Core.Shared.StudyPlan.Svc;
using Ngaq.Core.Infra;
using Tsinswreng.CsSql;
using Tsinswreng.CsTreeTest;
using Tsinswreng.CsTempus;

namespace Ngaq.Local.Test.Domains.StudyPlan;

public partial class TestISvcStudyPlan{
	void RegisterSyncStudyPlan(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestISvcStudyPlan),
			[typeof(ISvcStudyPlan)],
			[]
		);
		var R = register.Register;
		register.TesteeFnNames = [nameof(ISvcStudyPlan.SyncStudyPlan)];

		R("SyncStudyPlan_Should_InsertAndUpdate_ById", async(o)=>{
			var ctx = MkUserCtx(_ownerA);
			var token = _token + "_sync_sp_" + Guid.NewGuid().ToString("N");
			var pf = new PoPreFilter{
				Id = new IdPreFilter(),
				Owner = _ownerA,
				UniqName = token + "_pf",
				Type = EPreFilterType.Json,
				DataSchemaVer = new Version(1, 0),
				Text = "{\"Version\":\"1.0.0.0\",\"CoreFilter\":[],\"PropFilter\":[]}",
			};
			var wa = new PoWeightArg{
				Id = new IdWeightArg(),
				Owner = _ownerA,
				UniqName = token + "_wa",
				Type = EWeightArgType.Json,
				WeightCalculatorId = new IdWeightCalculator(),
				Text = "{}",
			};
			var wc = new PoWeightCalculator{
				Id = new IdWeightCalculator(),
				Owner = _ownerA,
				UniqName = token + "_wc",
				Type = EWeightCalculatorType.Builtin,
			};
			var add = new PoStudyPlan{
				Id = new IdStudyPlan(),
				Owner = _ownerA,
				UniqName = token + "_add",
				Descr = "add",
				PreFilterId = pf.Id,
				WeightArgId = wa.Id,
				WeightCalculatorId = wc.Id,
			};
			var upd = new PoStudyPlan{
				Id = new IdStudyPlan(),
				Owner = _ownerA,
				UniqName = token + "_upd",
				Descr = "old",
				PreFilterId = new IdPreFilter(),
				WeightArgId = new IdWeightArg(),
				WeightCalculatorId = new IdWeightCalculator(),
			};

			await RunNoTxn(async(db)=>{
				await RepoPreFilter.BatAdd(db, AsyE(pf), CT.None);
				await RepoWeightArg.BatAdd(db, AsyE(wa), CT.None);
				await RepoWeightCalculator.BatAdd(db, AsyE(wc), CT.None);
				await RepoStudyPlan.BatAdd(db, AsyE(upd), CT.None);
				return NIL;
			});
			_preFilterIds.Add(pf.Id);
			_weightArgIds.Add(wa.Id);
			_weightCalculatorIds.Add(wc.Id);
			_studyPlanIds.Add(add.Id);
			_studyPlanIds.Add(upd.Id);

			upd.Descr = "updated";
			upd.PreFilterId = pf.Id;
			upd.WeightArgId = wa.Id;
			upd.WeightCalculatorId = wc.Id;
			upd.BizUpdatedAt = Tempus.Now();
			await SvcStudyPlan.SyncStudyPlan(ctx, AsyE(add, upd), CT.None);

			await RunNoTxn(async(db)=>{
				var got = await ToList(RepoStudyPlan.BatGetByIdWithDel(db, AsyE(add.Id, upd.Id), CT.None));
				if(got.Count != 2 || got.Any(x=>x is null)){
					throw new Exception("SyncStudyPlan should keep both insert and update targets");
				}
				var addGot = got.First(x=>x!.Id == add.Id)!;
				var updGot = got.First(x=>x!.Id == upd.Id)!;
				if(addGot.Descr != add.Descr){
					throw new Exception("SyncStudyPlan should insert missing row");
				}
				if(updGot.Descr != upd.Descr || updGot.PreFilterId != pf.Id || updGot.WeightArgId != wa.Id || updGot.WeightCalculatorId != wc.Id){
					throw new Exception("SyncStudyPlan should update existing row");
				}
				return NIL;
			});
			return NIL;
		});
	}
}

