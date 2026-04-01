using Ngaq.Core.Shared.Base.Models.Po;
using Ngaq.Core.Shared.StudyPlan.Models;
using Ngaq.Core.Shared.StudyPlan.Models.Po.PreFilter;
using Ngaq.Core.Shared.StudyPlan.Models.Po.StudyPlan;
using Ngaq.Core.Shared.StudyPlan.Models.Po.WeightArg;
using Ngaq.Core.Shared.StudyPlan.Models.Po.WeightCalculator;
using Ngaq.Core.Shared.StudyPlan.Models.Req;
using Ngaq.Core.Shared.StudyPlan.Svc;
using Ngaq.Core.Shared.Word.WeightAlgo;
using Tsinswreng.CsPage;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Local.Test.Domains.StudyPlan;

public partial class TestISvcStudyPlan{
	void RegisterUpdateSoftDeleteAndRestoreApis(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestISvcStudyPlan)
			,[typeof(ISvcStudyPlan)]
			,[]
		);
		var R = register.Register;
		register.TesteeFnNames = [
			nameof(ISvcStudyPlan.BatUpdPreFilter)
			,nameof(ISvcStudyPlan.BatUpdWeightArg)
			,nameof(ISvcStudyPlan.BatUpdWeightCalculator)
			,nameof(ISvcStudyPlan.BatUpdStudyPlan)
			,nameof(ISvcStudyPlan.BatSoftDelPreFilter)
			,nameof(ISvcStudyPlan.BatSoftDelWeightArg)
			,nameof(ISvcStudyPlan.BatSoftDelWeightCalculator)
			,nameof(ISvcStudyPlan.BatSoftDelStudyPlan)
			,nameof(ISvcStudyPlan.RestoreBuiltinStudyPlan)
		];

		R("BatUpdPreFilter_Should_OnlyUpdate_OwnedRows", async(o)=>{
			var userCtx = MkUserCtx(_ownerA);
			var mine = new PoPreFilter{
				Id = new IdPreFilter(),
				Owner = _ownerA,
				UniqName = _token + "_upd_pf_mine",
				Descr = "before_mine",
				Type = EPreFilterType.Json,
				DataSchemaVer = new Version(1, 0),
				Text = "{\"Version\":\"1.0.0.0\",\"CoreFilter\":[],\"PropFilter\":[]}",
			};
			var other = new PoPreFilter{
				Id = new IdPreFilter(),
				Owner = _ownerB,
				UniqName = _token + "_upd_pf_other",
				Descr = "before_other",
				Type = EPreFilterType.Json,
				DataSchemaVer = new Version(1, 0),
				Text = "{\"Version\":\"1.0.0.0\",\"CoreFilter\":[],\"PropFilter\":[]}",
			};
			await RunNoTxn(async(ctx)=>{
				await RepoPreFilter.BatAdd(ctx, AsyE(mine, other), CT.None);
				return NIL;
			});
			_preFilterIds.Add(mine.Id);
			_preFilterIds.Add(other.Id);

			mine.Owner = _ownerB;
			mine.Descr = "after_mine";
			other.Owner = _ownerA;
			other.Descr = "after_other_should_not_apply";
			await SvcStudyPlan.BatUpdPreFilter(userCtx, AsyE(mine, other), CT.None);

			await RunNoTxn(async(ctx)=>{
				var gotMine = await RepoPreFilter.BatGetByIdWithDel(ctx, AsyE(mine.Id), CT.None).FirstOrDefaultAsync(CT.None);
				var gotOther = await RepoPreFilter.BatGetByIdWithDel(ctx, AsyE(other.Id), CT.None).FirstOrDefaultAsync(CT.None);
				if(gotMine is null || gotMine.Descr != "after_mine" || gotMine.Owner != _ownerA){
					throw new Exception("BatUpdPreFilter should update only owned row and force owner");
				}
				if(gotOther is null || gotOther.Descr != "before_other" || gotOther.Owner != _ownerB){
					throw new Exception("BatUpdPreFilter should not update other owner's row");
				}
				return NIL;
			});
			return NIL;
		});

		R("BatUpdWeightArg_Should_OnlyUpdate_OwnedRows", async(o)=>{
			var userCtx = MkUserCtx(_ownerA);
			var mine = new PoWeightArg{
				Id = new IdWeightArg(),
				Owner = _ownerA,
				UniqName = _token + "_upd_wa_mine",
				Type = EWeightArgType.Json,
				WeightCalculatorName = "calc_a",
				Text = "{\"A\":1}",
			};
			var other = new PoWeightArg{
				Id = new IdWeightArg(),
				Owner = _ownerB,
				UniqName = _token + "_upd_wa_other",
				Type = EWeightArgType.Json,
				WeightCalculatorName = "calc_b",
				Text = "{\"B\":2}",
			};
			await RunNoTxn(async(ctx)=>{
				await RepoWeightArg.BatAdd(ctx, AsyE(mine, other), CT.None);
				return NIL;
			});
			_weightArgIds.Add(mine.Id);
			_weightArgIds.Add(other.Id);

			mine.Owner = _ownerB;
			mine.Descr = "after_mine";
			mine.Text = "{\"A\":999}";
			other.Owner = _ownerA;
			other.Descr = "after_other_should_not_apply";
			await SvcStudyPlan.BatUpdWeightArg(userCtx, AsyE(mine, other), CT.None);

			await RunNoTxn(async(ctx)=>{
				var gotMine = await RepoWeightArg.BatGetByIdWithDel(ctx, AsyE(mine.Id), CT.None).FirstOrDefaultAsync(CT.None);
				var gotOther = await RepoWeightArg.BatGetByIdWithDel(ctx, AsyE(other.Id), CT.None).FirstOrDefaultAsync(CT.None);
				if(gotMine is null || gotMine.Descr != "after_mine" || gotMine.Owner != _ownerA){
					throw new Exception("BatUpdWeightArg should update only owned row and force owner");
				}
				if(gotOther is null || gotOther.Descr == "after_other_should_not_apply"){
					throw new Exception("BatUpdWeightArg should not update other owner's row");
				}
				return NIL;
			});
			return NIL;
		});

		R("BatUpdWeightCalculator_Should_OnlyUpdate_OwnedRows", async(o)=>{
			var userCtx = MkUserCtx(_ownerA);
			var mine = new PoWeightCalculator{
				Id = new IdWeightCalculator(),
				Owner = _ownerA,
				UniqName = _token + "_upd_wc_mine",
				Type = EWeightCalculatorType.Builtin,
			};
			var other = new PoWeightCalculator{
				Id = new IdWeightCalculator(),
				Owner = _ownerB,
				UniqName = _token + "_upd_wc_other",
				Type = EWeightCalculatorType.Builtin,
			};
			await RunNoTxn(async(ctx)=>{
				await RepoWeightCalculator.BatAdd(ctx, AsyE(mine, other), CT.None);
				return NIL;
			});
			_weightCalculatorIds.Add(mine.Id);
			_weightCalculatorIds.Add(other.Id);

			mine.Owner = _ownerB;
			mine.Descr = "after_mine";
			other.Owner = _ownerA;
			other.Descr = "after_other_should_not_apply";
			await SvcStudyPlan.BatUpdWeightCalculator(userCtx, AsyE(mine, other), CT.None);

			await RunNoTxn(async(ctx)=>{
				var gotMine = await RepoWeightCalculator.BatGetByIdWithDel(ctx, AsyE(mine.Id), CT.None).FirstOrDefaultAsync(CT.None);
				var gotOther = await RepoWeightCalculator.BatGetByIdWithDel(ctx, AsyE(other.Id), CT.None).FirstOrDefaultAsync(CT.None);
				if(gotMine is null || gotMine.Descr != "after_mine" || gotMine.Owner != _ownerA){
					throw new Exception("BatUpdWeightCalculator should update only owned row and force owner");
				}
				if(gotOther is null || gotOther.Descr == "after_other_should_not_apply"){
					throw new Exception("BatUpdWeightCalculator should not update other owner's row");
				}
				return NIL;
			});
			return NIL;
		});

		R("BatUpdStudyPlan_Should_OnlyUpdate_OwnedRows", async(o)=>{
			var userCtx = MkUserCtx(_ownerA);
			var mine = new PoStudyPlan{
				Id = new IdStudyPlan(),
				Owner = _ownerA,
				UniqName = _token + "_upd_sp_mine",
				Descr = "before_mine",
			};
			var other = new PoStudyPlan{
				Id = new IdStudyPlan(),
				Owner = _ownerB,
				UniqName = _token + "_upd_sp_other",
				Descr = "before_other",
			};
			await RunNoTxn(async(ctx)=>{
				await RepoStudyPlan.BatAdd(ctx, AsyE(mine, other), CT.None);
				return NIL;
			});
			_studyPlanIds.Add(mine.Id);
			_studyPlanIds.Add(other.Id);

			mine.Owner = _ownerB;
			mine.Descr = "after_mine";
			other.Owner = _ownerA;
			other.Descr = "after_other_should_not_apply";
			await SvcStudyPlan.BatUpdStudyPlan(userCtx, AsyE(mine, other), CT.None);

			await RunNoTxn(async(ctx)=>{
				var gotMine = await RepoStudyPlan.BatGetByIdWithDel(ctx, AsyE(mine.Id), CT.None).FirstOrDefaultAsync(CT.None);
				var gotOther = await RepoStudyPlan.BatGetByIdWithDel(ctx, AsyE(other.Id), CT.None).FirstOrDefaultAsync(CT.None);
				if(gotMine is null || gotMine.Descr != "after_mine" || gotMine.Owner != _ownerA){
					throw new Exception("BatUpdStudyPlan should update only owned row and force owner");
				}
				if(gotOther is null || gotOther.Descr == "after_other_should_not_apply"){
					throw new Exception("BatUpdStudyPlan should not update other owner's row");
				}
				return NIL;
			});
			return NIL;
		});

		R("BatSoftDelPreFilter_Should_OnlyDelete_OwnedRows", async(o)=>{
			var userCtx = MkUserCtx(_ownerA);
			var mine = new PoPreFilter{
				Id = new IdPreFilter(),
				Owner = _ownerA,
				UniqName = _token + "_del_pf_mine",
				Type = EPreFilterType.Json,
				DataSchemaVer = new Version(1, 0),
				Text = "{\"Version\":\"1.0.0.0\",\"CoreFilter\":[],\"PropFilter\":[]}",
			};
			var other = new PoPreFilter{
				Id = new IdPreFilter(),
				Owner = _ownerB,
				UniqName = _token + "_del_pf_other",
				Type = EPreFilterType.Json,
				DataSchemaVer = new Version(1, 0),
				Text = "{\"Version\":\"1.0.0.0\",\"CoreFilter\":[],\"PropFilter\":[]}",
			};
			await RunNoTxn(async(ctx)=>{
				await RepoPreFilter.BatAdd(ctx, AsyE(mine, other), CT.None);
				return NIL;
			});
			_preFilterIds.Add(mine.Id);
			_preFilterIds.Add(other.Id);

			await SvcStudyPlan.BatSoftDelPreFilter(userCtx, AsyE(mine, other), CT.None);

			await RunNoTxn(async(ctx)=>{
				var gotMine = await RepoPreFilter.BatGetByIdWithDel(ctx, AsyE(mine.Id), CT.None).FirstOrDefaultAsync(CT.None);
				var gotOther = await RepoPreFilter.BatGetByIdWithDel(ctx, AsyE(other.Id), CT.None).FirstOrDefaultAsync(CT.None);
				if(gotMine is null || !gotMine.IsDeleted()){
					throw new Exception("BatSoftDelPreFilter should delete owned row");
				}
				if(gotOther is null || gotOther.IsDeleted()){
					throw new Exception("BatSoftDelPreFilter should not delete other owner's row");
				}
				return NIL;
			});
			return NIL;
		});

		R("BatSoftDelWeightArg_Should_OnlyDelete_OwnedRows", async(o)=>{
			var userCtx = MkUserCtx(_ownerA);
			var mine = new PoWeightArg{
				Id = new IdWeightArg(),
				Owner = _ownerA,
				UniqName = _token + "_del_wa_mine",
				Type = EWeightArgType.Json,
				WeightCalculatorName = "x",
				Text = "{}",
			};
			var other = new PoWeightArg{
				Id = new IdWeightArg(),
				Owner = _ownerB,
				UniqName = _token + "_del_wa_other",
				Type = EWeightArgType.Json,
				WeightCalculatorName = "x",
				Text = "{}",
			};
			await RunNoTxn(async(ctx)=>{
				await RepoWeightArg.BatAdd(ctx, AsyE(mine, other), CT.None);
				return NIL;
			});
			_weightArgIds.Add(mine.Id);
			_weightArgIds.Add(other.Id);

			await SvcStudyPlan.BatSoftDelWeightArg(userCtx, AsyE(mine, other), CT.None);

			await RunNoTxn(async(ctx)=>{
				var gotMine = await RepoWeightArg.BatGetByIdWithDel(ctx, AsyE(mine.Id), CT.None).FirstOrDefaultAsync(CT.None);
				var gotOther = await RepoWeightArg.BatGetByIdWithDel(ctx, AsyE(other.Id), CT.None).FirstOrDefaultAsync(CT.None);
				if(gotMine is null || !gotMine.IsDeleted()){
					throw new Exception("BatSoftDelWeightArg should delete owned row");
				}
				if(gotOther is null || gotOther.IsDeleted()){
					throw new Exception("BatSoftDelWeightArg should not delete other owner's row");
				}
				return NIL;
			});
			return NIL;
		});

		R("BatSoftDelWeightCalculator_Should_OnlyDelete_OwnedRows", async(o)=>{
			var userCtx = MkUserCtx(_ownerA);
			var mine = new PoWeightCalculator{
				Id = new IdWeightCalculator(),
				Owner = _ownerA,
				UniqName = _token + "_del_wc_mine",
				Type = EWeightCalculatorType.Builtin,
			};
			var other = new PoWeightCalculator{
				Id = new IdWeightCalculator(),
				Owner = _ownerB,
				UniqName = _token + "_del_wc_other",
				Type = EWeightCalculatorType.Builtin,
			};
			await RunNoTxn(async(ctx)=>{
				await RepoWeightCalculator.BatAdd(ctx, AsyE(mine, other), CT.None);
				return NIL;
			});
			_weightCalculatorIds.Add(mine.Id);
			_weightCalculatorIds.Add(other.Id);

			await SvcStudyPlan.BatSoftDelWeightCalculator(userCtx, AsyE(mine, other), CT.None);

			await RunNoTxn(async(ctx)=>{
				var gotMine = await RepoWeightCalculator.BatGetByIdWithDel(ctx, AsyE(mine.Id), CT.None).FirstOrDefaultAsync(CT.None);
				var gotOther = await RepoWeightCalculator.BatGetByIdWithDel(ctx, AsyE(other.Id), CT.None).FirstOrDefaultAsync(CT.None);
				if(gotMine is null || !gotMine.IsDeleted()){
					throw new Exception("BatSoftDelWeightCalculator should delete owned row");
				}
				if(gotOther is null || gotOther.IsDeleted()){
					throw new Exception("BatSoftDelWeightCalculator should not delete other owner's row");
				}
				return NIL;
			});
			return NIL;
		});

		R("BatSoftDelStudyPlan_Should_DeleteRootOnly", async(o)=>{
			var userCtx = MkUserCtx(_ownerA);
			var preFilter = new PoPreFilter{
				Id = new IdPreFilter(),
				Owner = _ownerA,
				UniqName = _token + "_del_sp_pf",
				Type = EPreFilterType.Json,
				DataSchemaVer = new Version(1, 0),
				Text = "{\"Version\":\"1.0.0.0\",\"CoreFilter\":[],\"PropFilter\":[]}",
			};
			var weightArg = new PoWeightArg{
				Id = new IdWeightArg(),
				Owner = _ownerA,
				UniqName = _token + "_del_sp_wa",
				Type = EWeightArgType.Json,
				WeightCalculatorName = _token + "_del_sp_wc",
				Text = "{}",
			};
			var weightCalculator = new PoWeightCalculator{
				Id = new IdWeightCalculator(),
				Owner = _ownerA,
				UniqName = _token + "_del_sp_wc",
				Type = EWeightCalculatorType.Builtin,
			};
			var studyPlan = new PoStudyPlan{
				Id = new IdStudyPlan(),
				Owner = _ownerA,
				UniqName = _token + "_del_sp_root",
				PreFilterId = preFilter.Id,
				WeightArgId = weightArg.Id,
				WeightCalculatorId = weightCalculator.Id,
			};

			await RunNoTxn(async(ctx)=>{
				await RepoPreFilter.BatAdd(ctx, AsyE(preFilter), CT.None);
				await RepoWeightArg.BatAdd(ctx, AsyE(weightArg), CT.None);
				await RepoWeightCalculator.BatAdd(ctx, AsyE(weightCalculator), CT.None);
				await RepoStudyPlan.BatAdd(ctx, AsyE(studyPlan), CT.None);
				return NIL;
			});
			_preFilterIds.Add(preFilter.Id);
			_weightArgIds.Add(weightArg.Id);
			_weightCalculatorIds.Add(weightCalculator.Id);
			_studyPlanIds.Add(studyPlan.Id);

			await SvcStudyPlan.BatSoftDelStudyPlan(userCtx, AsyE(studyPlan), CT.None);

			await RunNoTxn(async(ctx)=>{
				var gotSp = await RepoStudyPlan.BatGetByIdWithDel(ctx, AsyE(studyPlan.Id), CT.None).FirstOrDefaultAsync(CT.None);
				var gotPf = await RepoPreFilter.BatGetByIdWithDel(ctx, AsyE(preFilter.Id), CT.None).FirstOrDefaultAsync(CT.None);
				var gotWa = await RepoWeightArg.BatGetByIdWithDel(ctx, AsyE(weightArg.Id), CT.None).FirstOrDefaultAsync(CT.None);
				var gotWc = await RepoWeightCalculator.BatGetByIdWithDel(ctx, AsyE(weightCalculator.Id), CT.None).FirstOrDefaultAsync(CT.None);
				if(gotSp is null || !gotSp.IsDeleted()){
					throw new Exception("BatSoftDelStudyPlan should soft-delete root studyplan");
				}
				if(gotPf is null || gotPf.IsDeleted() || gotWa is null || gotWa.IsDeleted() || gotWc is null || gotWc.IsDeleted()){
					throw new Exception("BatSoftDelStudyPlan should not delete related assets");
				}
				return NIL;
			});
			return NIL;
		});

		R("RestoreBuiltinStudyPlan_Should_SoftDelete_OldBuiltin_And_Rebuild", async(o)=>{
			var userCtx = MkUserCtx(_ownerA);
			var oldCalc = new PoWeightCalculator{
				Id = new IdWeightCalculator(),
				Owner = _ownerA,
				UniqName = Consts.BuiltinPrefix + DfltWeightCalculator.Name,
				Type = EWeightCalculatorType.Js,
				Text = "invalid_old_builtin",
				Descr = "old",
			};
			var oldArg = new PoWeightArg{
				Id = new IdWeightArg(),
				Owner = _ownerA,
				UniqName = Consts.BuiltinPrefix + DfltWeightCfg.Name,
				Type = EWeightArgType.Json,
				WeightCalculatorName = oldCalc.UniqName ?? "",
				Text = "{\"Old\":1}",
				Descr = "old",
			};
			var oldPlan = new PoStudyPlan{
				Id = new IdStudyPlan(),
				Owner = _ownerA,
				UniqName = Consts.BuiltinPrefix + "Default",
				Descr = "old",
				WeightCalculatorId = oldCalc.Id,
				WeightArgId = oldArg.Id,
			};
			await RunNoTxn(async(ctx)=>{
				await RepoWeightCalculator.BatAdd(ctx, AsyE(oldCalc), CT.None);
				await RepoWeightArg.BatAdd(ctx, AsyE(oldArg), CT.None);
				await RepoStudyPlan.BatAdd(ctx, AsyE(oldPlan), CT.None);
				return NIL;
			});
			_weightCalculatorIds.Add(oldCalc.Id);
			_weightArgIds.Add(oldArg.Id);
			_studyPlanIds.Add(oldPlan.Id);
			await SvcStudyPlan.SetCurStudyPlanId(userCtx, oldPlan.Id, CT.None);

			await SvcStudyPlan.RestoreBuiltinStudyPlan(userCtx, CT.None);

			await RunNoTxn(async(ctx)=>{
				var oldCalcGot = await RepoWeightCalculator.BatGetByIdWithDel(ctx, AsyE(oldCalc.Id), CT.None).FirstOrDefaultAsync(CT.None);
				var oldArgGot = await RepoWeightArg.BatGetByIdWithDel(ctx, AsyE(oldArg.Id), CT.None).FirstOrDefaultAsync(CT.None);
				var oldPlanGot = await RepoStudyPlan.BatGetByIdWithDel(ctx, AsyE(oldPlan.Id), CT.None).FirstOrDefaultAsync(CT.None);
				if(oldCalcGot is null || !oldCalcGot.IsDeleted() || oldArgGot is null || !oldArgGot.IsDeleted() || oldPlanGot is null || !oldPlanGot.IsDeleted()){
					throw new Exception("RestoreBuiltinStudyPlan should soft-delete old builtin rows");
				}
				return NIL;
			});

			var calcPage = await SvcStudyPlan.PageWeightCalculator(userCtx, new ReqPageWeightCalculator{
				PageQry = new PageQry{PageIdx = 0, PageSize = 20},
				UniqNameSearch = Consts.BuiltinPrefix + DfltWeightCalculator.Name,
			}, CT.None);
			var argPage = await SvcStudyPlan.PageWeightArg(userCtx, new ReqPageWeightArg{
				PageQry = new PageQry{PageIdx = 0, PageSize = 20},
				UniqNameSearch = Consts.BuiltinPrefix + DfltWeightCfg.Name,
			}, CT.None);
			var planPage = await SvcStudyPlan.PageStudyPlan(userCtx, new ReqPageStudyPlan{
				PageQry = new PageQry{PageIdx = 0, PageSize = 20},
				UniqNameSearch = Consts.BuiltinPrefix + "Default",
			}, CT.None);

			var calcs = await ToList(calcPage.DataAsyE);
			var args = await ToList(argPage.DataAsyE);
			var plans = await ToList(planPage.DataAsyE);
			var newCalc = calcs.FirstOrDefault(x=>x.UniqName == Consts.BuiltinPrefix + DfltWeightCalculator.Name);
			var newArg = args.FirstOrDefault(x=>x.UniqName == Consts.BuiltinPrefix + DfltWeightCfg.Name);
			var newPlan = plans.FirstOrDefault(x=>x.UniqName == Consts.BuiltinPrefix + "Default");
			if(newCalc is null || newArg is null || newPlan is null){
				throw new Exception("RestoreBuiltinStudyPlan should rebuild builtin rows");
			}
			if(newCalc.Type != EWeightCalculatorType.Builtin){
				throw new Exception("Restored builtin calculator should be Builtin type");
			}
			if(string.IsNullOrWhiteSpace(newArg.Text)){
				throw new Exception("Restored builtin weight arg should have default json");
			}
			_weightCalculatorIds.Add(newCalc.Id);
			_weightArgIds.Add(newArg.Id);
			_studyPlanIds.Add(newPlan.Id);

			var curJn = await SvcStudyPlan.GetCurJnStudyPlan(userCtx, CT.None);
			if(curJn is null){
				throw new Exception("RestoreBuiltinStudyPlan should keep current plan retrievable");
			}
			return NIL;
		});
	}
}
