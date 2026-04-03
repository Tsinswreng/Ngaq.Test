using Ngaq.Core.Infra.Errors;
using Ngaq.Core.Infra.IF;
using Ngaq.Core.Shared.StudyPlan.Models.Po.PreFilter;
using Ngaq.Core.Shared.StudyPlan.Models.Po.StudyPlan;
using Ngaq.Core.Shared.StudyPlan.Models.Po.WeightArg;
using Ngaq.Core.Shared.StudyPlan.Models.Po.WeightCalculator;
using Ngaq.Core.Shared.StudyPlan.Svc;
using Tsinswreng.CsErr;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Local.Test.Domains.StudyPlan;

public partial class TestISvcStudyPlan{
	static void AssertThrowsErrItem(
		Exception Ex
		,IErrItem Expected
		,str CaseName
	){
		if(Ex is not AppErr appErr){
			throw new Exception($"{CaseName} should throw AppErr, got {Ex.GetType().FullName}");
		}
		if(!ReferenceEquals(appErr.Type, Expected)){
			throw new Exception($"{CaseName} should throw expected err item, got [{appErr.Key}]");
		}
	}

	void RegisterConflictErrCases(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestISvcStudyPlan)
			,[typeof(ISvcStudyPlan)]
			,[]
		);
		var R = register.Register;
		register.TesteeFnNames = [
			nameof(ISvcStudyPlan.BatAddPreFilter)
			,nameof(ISvcStudyPlan.BatAddWeightArg)
			,nameof(ISvcStudyPlan.BatAddWeightCalculator)
			,nameof(ISvcStudyPlan.BatUpdPreFilter)
			,nameof(ISvcStudyPlan.BatUpdWeightArg)
			,nameof(ISvcStudyPlan.BatUpdWeightCalculator)
			,nameof(ISvcStudyPlan.BatUpdStudyPlan)
		];

		R("BatAddPreFilter_Should_Throw_AddFailedDataMayConflict_OnUniqConflict", async(o)=>{
			var userCtx = MkUserCtx(_ownerA);
			var row = new PoPreFilter{
				Id = new IdPreFilter(),
				Owner = _ownerA,
				UniqName = _token + "_pf_a_1",
				Type = EPreFilterType.Json,
				DataSchemaVer = new Version(1, 0),
				Text = "{\"Version\":\"1.0.0.0\",\"CoreFilter\":[],\"PropFilter\":[]}",
			};
			_preFilterIds.Add(row.Id);

			try{
				await SvcStudyPlan.BatAddPreFilter(userCtx, AsyE(row), CT.None);
				throw new Exception("BatAddPreFilter conflict should throw");
			}catch(Exception ex){
				AssertThrowsErrItem(ex, ItemsErr.StudyPlan.AddFailedDataMayConflict, nameof(ISvcStudyPlan.BatAddPreFilter));
			}
			return NIL;
		});

		R("BatAddWeightArg_Should_Throw_AddFailedDataMayConflict_OnUniqConflict", async(o)=>{
			var userCtx = MkUserCtx(_ownerA);
			var row = new PoWeightArg{
				Id = new IdWeightArg(),
				Owner = _ownerA,
				UniqName = _token + "_wa_a_1",
				Type = EWeightArgType.Json,
				WeightCalculatorId = IdWeightCalculator.Zero,
				Text = "{}",
			};
			_weightArgIds.Add(row.Id);

			try{
				await SvcStudyPlan.BatAddWeightArg(userCtx, AsyE(row), CT.None);
				throw new Exception("BatAddWeightArg conflict should throw");
			}catch(Exception ex){
				AssertThrowsErrItem(ex, ItemsErr.StudyPlan.AddFailedDataMayConflict, nameof(ISvcStudyPlan.BatAddWeightArg));
			}
			return NIL;
		});

		R("BatAddWeightCalculator_Should_Throw_AddFailedDataMayConflict_OnUniqConflict", async(o)=>{
			var userCtx = MkUserCtx(_ownerA);
			var row = new PoWeightCalculator{
				Id = new IdWeightCalculator(),
				Owner = _ownerA,
				UniqName = _token + "_wc_a_1",
				Type = EWeightCalculatorType.Builtin,
			};
			_weightCalculatorIds.Add(row.Id);

			try{
				await SvcStudyPlan.BatAddWeightCalculator(userCtx, AsyE(row), CT.None);
				throw new Exception("BatAddWeightCalculator conflict should throw");
			}catch(Exception ex){
				AssertThrowsErrItem(ex, ItemsErr.StudyPlan.AddFailedDataMayConflict, nameof(ISvcStudyPlan.BatAddWeightCalculator));
			}
			return NIL;
		});

		R("BatUpdPreFilter_Should_Throw_UpdateFailedDataMayConflict_OnUniqConflict", async(o)=>{
			var userCtx = MkUserCtx(_ownerA);
			var row1 = new PoPreFilter{
				Id = new IdPreFilter(),
				Owner = _ownerA,
				UniqName = _token + "_upd_conflict_pf_1",
				Type = EPreFilterType.Json,
				DataSchemaVer = new Version(1, 0),
				Text = "{\"Version\":\"1.0.0.0\",\"CoreFilter\":[],\"PropFilter\":[]}",
			};
			var row2 = new PoPreFilter{
				Id = new IdPreFilter(),
				Owner = _ownerA,
				UniqName = _token + "_upd_conflict_pf_2",
				Type = EPreFilterType.Json,
				DataSchemaVer = new Version(1, 0),
				Text = "{\"Version\":\"1.0.0.0\",\"CoreFilter\":[],\"PropFilter\":[]}",
			};
			await RunNoTxn(async(ctx)=>{
				await RepoPreFilter.BatAdd(ctx, AsyE(row1, row2), CT.None);
				return NIL;
			});
			_preFilterIds.Add(row1.Id);
			_preFilterIds.Add(row2.Id);

			row1.UniqName = row2.UniqName;
			try{
				await SvcStudyPlan.BatUpdPreFilter(userCtx, AsyE(row1), CT.None);
				throw new Exception("BatUpdPreFilter conflict should throw");
			}catch(Exception ex){
				AssertThrowsErrItem(ex, ItemsErr.StudyPlan.UpdateFailedDataMayConflict, nameof(ISvcStudyPlan.BatUpdPreFilter));
			}
			return NIL;
		});

		R("BatUpdWeightArg_Should_Throw_UpdateFailedDataMayConflict_OnUniqConflict", async(o)=>{
			var userCtx = MkUserCtx(_ownerA);
			var row1 = new PoWeightArg{
				Id = new IdWeightArg(),
				Owner = _ownerA,
				UniqName = _token + "_upd_conflict_wa_1",
				Type = EWeightArgType.Json,
				WeightCalculatorId = IdWeightCalculator.Zero,
				Text = "{\"A\":1}",
			};
			var row2 = new PoWeightArg{
				Id = new IdWeightArg(),
				Owner = _ownerA,
				UniqName = _token + "_upd_conflict_wa_2",
				Type = EWeightArgType.Json,
				WeightCalculatorId = IdWeightCalculator.Zero,
				Text = "{\"A\":2}",
			};
			await RunNoTxn(async(ctx)=>{
				await RepoWeightArg.BatAdd(ctx, AsyE(row1, row2), CT.None);
				return NIL;
			});
			_weightArgIds.Add(row1.Id);
			_weightArgIds.Add(row2.Id);

			row1.UniqName = row2.UniqName;
			try{
				await SvcStudyPlan.BatUpdWeightArg(userCtx, AsyE(row1), CT.None);
				throw new Exception("BatUpdWeightArg conflict should throw");
			}catch(Exception ex){
				AssertThrowsErrItem(ex, ItemsErr.StudyPlan.UpdateFailedDataMayConflict, nameof(ISvcStudyPlan.BatUpdWeightArg));
			}
			return NIL;
		});

		R("BatUpdWeightCalculator_Should_Throw_UpdateFailedDataMayConflict_OnUniqConflict", async(o)=>{
			var userCtx = MkUserCtx(_ownerA);
			var row1 = new PoWeightCalculator{
				Id = new IdWeightCalculator(),
				Owner = _ownerA,
				UniqName = _token + "_upd_conflict_wc_1",
				Type = EWeightCalculatorType.Builtin,
			};
			var row2 = new PoWeightCalculator{
				Id = new IdWeightCalculator(),
				Owner = _ownerA,
				UniqName = _token + "_upd_conflict_wc_2",
				Type = EWeightCalculatorType.Builtin,
			};
			await RunNoTxn(async(ctx)=>{
				await RepoWeightCalculator.BatAdd(ctx, AsyE(row1, row2), CT.None);
				return NIL;
			});
			_weightCalculatorIds.Add(row1.Id);
			_weightCalculatorIds.Add(row2.Id);

			row1.UniqName = row2.UniqName;
			try{
				await SvcStudyPlan.BatUpdWeightCalculator(userCtx, AsyE(row1), CT.None);
				throw new Exception("BatUpdWeightCalculator conflict should throw");
			}catch(Exception ex){
				AssertThrowsErrItem(ex, ItemsErr.StudyPlan.UpdateFailedDataMayConflict, nameof(ISvcStudyPlan.BatUpdWeightCalculator));
			}
			return NIL;
		});

		R("BatUpdStudyPlan_Should_Throw_UpdateFailedDataMayConflict_OnUniqConflict", async(o)=>{
			var userCtx = MkUserCtx(_ownerA);
			var row1 = new PoStudyPlan{
				Id = new IdStudyPlan(),
				Owner = _ownerA,
				UniqName = _token + "_upd_conflict_sp_1",
			};
			var row2 = new PoStudyPlan{
				Id = new IdStudyPlan(),
				Owner = _ownerA,
				UniqName = _token + "_upd_conflict_sp_2",
			};
			await RunNoTxn(async(ctx)=>{
				await RepoStudyPlan.BatAdd(ctx, AsyE(row1, row2), CT.None);
				return NIL;
			});
			_studyPlanIds.Add(row1.Id);
			_studyPlanIds.Add(row2.Id);

			row1.UniqName = row2.UniqName;
			try{
				await SvcStudyPlan.BatUpdStudyPlan(userCtx, AsyE(row1), CT.None);
				throw new Exception("BatUpdStudyPlan conflict should throw");
			}catch(Exception ex){
				AssertThrowsErrItem(ex, ItemsErr.StudyPlan.UpdateFailedDataMayConflict, nameof(ISvcStudyPlan.BatUpdStudyPlan));
			}
			return NIL;
		});
	}
}
