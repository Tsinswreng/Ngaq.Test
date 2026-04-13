using Ngaq.Core.Shared.StudyPlan.Models.Po.WeightCalculator;
using Ngaq.Core.Shared.StudyPlan.Svc;
using Ngaq.Core.Infra;
using Tsinswreng.CsSql;
using Tsinswreng.CsTreeTest;
using Tsinswreng.CsTempus;

namespace Ngaq.Local.Test.Domains.StudyPlan;

public partial class TestISvcStudyPlan{
	void RegisterSyncWeightCalculator(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestISvcStudyPlan),
			[typeof(ISvcStudyPlan)],
			[]
		);
		var R = register.Register;
		register.TesteeFnNames = [nameof(ISvcStudyPlan.SyncWeightCalculator)];

		R("SyncWeightCalculator_Should_InsertAndUpdate_ById", async(o)=>{
			var ctx = MkUserCtx(_ownerA);
			var token = _token + "_sync_wc_" + Guid.NewGuid().ToString("N");
			var add = new PoWeightCalculator{
				Id = new IdWeightCalculator(),
				Owner = _ownerA,
				UniqName = token + "_add",
				Type = EWeightCalculatorType.Builtin,
				Text = "add_text",
			};
			var upd = new PoWeightCalculator{
				Id = new IdWeightCalculator(),
				Owner = _ownerA,
				UniqName = token + "_upd",
				Type = EWeightCalculatorType.Builtin,
				Text = "old_text",
			};

			await RunNoTxn(async(db)=>{
				await RepoWeightCalculator.BatAdd(db, AsyE(upd), CT.None);
				return NIL;
			});
			_weightCalculatorIds.Add(add.Id);
			_weightCalculatorIds.Add(upd.Id);

			upd.Type = EWeightCalculatorType.Js;
			upd.Text = "new_text";
			upd.Descr = "updated";
			upd.BizUpdatedAt = Tempus.Now();
			await SvcStudyPlan.SyncWeightCalculator(ctx, AsyE(add, upd), CT.None);

			await RunNoTxn(async(db)=>{
				var got = await ToList(RepoWeightCalculator.BatGetByIdWithDel(db, AsyE(add.Id, upd.Id), CT.None));
				if(got.Count != 2 || got.Any(x=>x is null)){
					throw new Exception("SyncWeightCalculator should keep both insert and update targets");
				}
				var addGot = got.First(x=>x!.Id == add.Id)!;
				var updGot = got.First(x=>x!.Id == upd.Id)!;
				if(addGot.Text != add.Text){
					throw new Exception("SyncWeightCalculator should insert missing row");
				}
				if(updGot.Text != upd.Text || updGot.Type != upd.Type || updGot.Descr != upd.Descr){
					throw new Exception("SyncWeightCalculator should update existing row");
				}
				return NIL;
			});
			return NIL;
		});
	}
}

