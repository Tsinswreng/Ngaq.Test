using Ngaq.Core.Shared.StudyPlan.Models.Po.WeightArg;
using Ngaq.Core.Shared.StudyPlan.Models.Po.WeightCalculator;
using Ngaq.Core.Shared.StudyPlan.Svc;
using Ngaq.Core.Infra;
using Tsinswreng.CsSql;
using Tsinswreng.CsTreeTest;
using Tsinswreng.CsTempus;

namespace Ngaq.Backend.Test.Domains.StudyPlan;

public partial class TestISvcStudyPlan{
	void RegisterSyncWeightArg(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestISvcStudyPlan),
			[typeof(ISvcStudyPlan)],
			[]
		);
		var R = register.Register;
		register.TesteeFnNames = [nameof(ISvcStudyPlan.SyncWeightArg)];

		R("SyncWeightArg_Should_InsertAndUpdate_ById", async(o)=>{
			var ctx = MkUserCtx(_ownerA);
			var token = _token + "_sync_wa_" + Guid.NewGuid().ToString("N");
			var add = new PoWeightArg{
				Id = new IdWeightArg(),
				Owner = _ownerA,
				UniqName = token + "_add",
				Type = EWeightArgType.Json,
				WeightCalculatorId = new IdWeightCalculator(),
				Text = "{\"v\":1}",
			};
			var upd = new PoWeightArg{
				Id = new IdWeightArg(),
				Owner = _ownerA,
				UniqName = token + "_upd",
				Type = EWeightArgType.Json,
				WeightCalculatorId = new IdWeightCalculator(),
				Text = "{\"v\":10}",
			};

			await RunNoTxn(async(db)=>{
				await RepoWeightArg.OrdAdd(db, AsyE(upd), CT.None);
				return NIL;
			});
			_weightArgIds.Add(add.Id);
			_weightArgIds.Add(upd.Id);

			upd.Text = "{\"v\":99}";
			upd.Descr = "updated";
			upd.BizUpdatedAt = UnixMs.Now();
			await SvcStudyPlan.SyncWeightArg(ctx, AsyE(add, upd), CT.None);

			await RunNoTxn(async(db)=>{
				var got = await ToList(RepoWeightArg.OrdGetByIdWithDel(db, AsyE(add.Id, upd.Id), CT.None));
				if(got.Count != 2 || got.Any(x=>x is null)){
					throw new Exception("SyncWeightArg should keep both insert and update targets");
				}
				var addGot = got.First(x=>x!.Id == add.Id)!;
				var updGot = got.First(x=>x!.Id == upd.Id)!;
				if(addGot.Text != add.Text){
					throw new Exception("SyncWeightArg should insert missing row");
				}
				if(updGot.Text != upd.Text || updGot.Descr != upd.Descr){
					throw new Exception("SyncWeightArg should update existing row");
				}
				return NIL;
			});
			return NIL;
		});
	}
}

