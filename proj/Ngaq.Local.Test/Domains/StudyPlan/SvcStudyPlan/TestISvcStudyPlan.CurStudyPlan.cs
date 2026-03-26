using Ngaq.Core.Shared.StudyPlan.Models;
using Ngaq.Core.Shared.StudyPlan.Models.PreFilter;
using Ngaq.Core.Shared.StudyPlan.Models.Po.PreFilter;
using Ngaq.Core.Shared.StudyPlan.Models.Po.StudyPlan;
using Ngaq.Core.Shared.StudyPlan.Models.Po.WeightArg;
using Ngaq.Core.Shared.StudyPlan.Models.Po.WeightCalculator;
using Ngaq.Core.Shared.Word.Models.Learn_;
using Ngaq.Core.Shared.Word.WeightAlgo;
using Ngaq.Core.Tools;
using Ngaq.Core.Word.Models.Weight;
using Tsinswreng.CsTreeTest;
using System.Text;

namespace Ngaq.Local.Test.Domains.StudyPlan;

public partial class TestISvcStudyPlan{
	void RegisterCurStudyPlanApis(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestISvcStudyPlan)
			,[typeof(Ngaq.Core.Shared.StudyPlan.Svc.ISvcStudyPlan)]
			,[]
		);
		var R = register.Register;
		register.TesteeFnNames = [
			nameof(Ngaq.Core.Shared.StudyPlan.Svc.ISvcStudyPlan.SetCurStudyPlanId)
			,nameof(Ngaq.Core.Shared.StudyPlan.Svc.ISvcStudyPlan.GetCurStudyPlanId)
			,nameof(Ngaq.Core.Shared.StudyPlan.Svc.ISvcStudyPlan.GetCurJnStudyPlan)
			,nameof(Ngaq.Core.Shared.StudyPlan.Svc.ISvcStudyPlan.GetCurBoStudyPlan)
			,nameof(Ngaq.Core.Shared.StudyPlan.Svc.ISvcStudyPlan.GetCurWeightCalctr)
		];

		R("CurStudyPlan_SetGet_Id_Roundtrip", async(o)=>{
			var userCtx = MkUserCtx(_ownerA);
			var sp = new PoStudyPlan{
				Id = new IdStudyPlan(),
				Owner = _ownerA,
				UniqName = _token + "_cur_sp_id_rt",
				Descr = "cur_sp_id_rt",
				BizUpdatedAt = 5101,
			};
			await RunNoTxn(async(ctx)=>{
				await RepoStudyPlan.BatAdd(ctx, AsyE(sp), CT.None);
				return NIL;
			});
			_studyPlanIds.Add(sp.Id);

			await SvcStudyPlan.SetCurStudyPlanId(userCtx, sp.Id, CT.None);
			var gotId = await SvcStudyPlan.GetCurStudyPlanId(userCtx, CT.None);
			if(gotId is not IdStudyPlan id || id != sp.Id){
				throw new Exception("Set/Get current study plan id roundtrip failed");
			}
			return NIL;
		});

		R("CurStudyPlan_GetCurJnStudyPlan_Should_OwnerFilter_Children", async(o)=>{
			var userCtx = MkUserCtx(_ownerA);
			var preFilter = new PoPreFilter{
				Id = new IdPreFilter(),
				Owner = _ownerB,
				UniqName = _token + "_cur_jn_pf_b",
				Type = EPreFilterType.Json,
				DataSchemaVer = new Version(1, 0),
				Data = Encoding.UTF8.GetBytes("{\"Version\":\"1.0.0.0\",\"CoreFilter\":[],\"PropFilter\":[]}"),
				BizUpdatedAt = 5201,
			};
			var weightArg = new PoWeightArg{
				Id = new IdWeightArg(),
				Owner = _ownerB,
				UniqName = _token + "_cur_jn_wa_b",
				Type = EWeightArgType.Json,
				WeightCalculatorName = "x",
				Data = Encoding.UTF8.GetBytes("{}"),
				BizUpdatedAt = 5202,
			};
			var weightCalculator = new PoWeightCalculator{
				Id = new IdWeightCalculator(),
				Owner = _ownerB,
				UniqName = _token + "_cur_jn_wc_b",
				Type = EWeightCalculatorType.Js,
				Data = Encoding.UTF8.GetBytes("'[]'"),
			};
			var studyPlan = new PoStudyPlan{
				Id = new IdStudyPlan(),
				Owner = _ownerA,
				UniqName = _token + "_cur_jn_sp_a",
				Descr = "cur_jn_sp_a",
				PreFilterId = preFilter.Id,
				WeightArgId = weightArg.Id,
				WeightCalculatorId = weightCalculator.Id,
				BizUpdatedAt = 5203,
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

			await SvcStudyPlan.SetCurStudyPlanId(userCtx, studyPlan.Id, CT.None);
			var got = await SvcStudyPlan.GetCurJnStudyPlan(userCtx, CT.None);
			if(got is null){
				throw new Exception("GetCurJnStudyPlan should not return null");
			}
			if(got.StudyPlan.Id != studyPlan.Id){
				throw new Exception("GetCurJnStudyPlan returned wrong study plan");
			}
			if(got.PreFilter is not null || got.WeightArg is not null || got.WeightCalculator is not null){
				throw new Exception("GetCurJnStudyPlan owner filter on children failed");
			}
			return NIL;
		});

		R("CurStudyPlan_GetCurBoStudyPlan_ParseCacheAndWeight", async(o)=>{
			var userCtx = MkUserCtx(_ownerA);
			var pfJson = "{\"Version\":\"1.0.0.0\",\"CoreFilter\":[{\"Fields\":[\"Lang\"],\"Filters\":[{\"Operation\":1,\"ValueType\":1,\"Values\":[\"English\"]}]}],\"PropFilter\":[]}";
			var waJson = "{\"DfltAddBonus\":123,\"DebuffNumerator\":456}";

			var preFilter = new PoPreFilter{
				Id = new IdPreFilter(),
				Owner = _ownerA,
				UniqName = _token + "_cur_bo_pf_a",
				Type = EPreFilterType.Json,
				DataSchemaVer = new Version(1, 0),
				Data = Encoding.UTF8.GetBytes(pfJson),
				BizUpdatedAt = 5301,
			};
			var weightArg = new PoWeightArg{
				Id = new IdWeightArg(),
				Owner = _ownerA,
				UniqName = _token + "_cur_bo_wa_a",
				Type = EWeightArgType.Json,
				WeightCalculatorName = _token + "_cur_bo_wc_a",
				Data = Encoding.UTF8.GetBytes(waJson),
				BizUpdatedAt = 5302,
			};
			var weightCalculator = new PoWeightCalculator{
				Id = new IdWeightCalculator(),
				Owner = _ownerA,
				UniqName = _token + "_cur_bo_wc_a",
				Type = EWeightCalculatorType.Js,
				Data = Encoding.UTF8.GetBytes("'[]'"),
			};
			var studyPlan = new PoStudyPlan{
				Id = new IdStudyPlan(),
				Owner = _ownerA,
				UniqName = _token + "_cur_bo_sp_a",
				Descr = "cur_bo_sp_a",
				PreFilterId = preFilter.Id,
				WeightArgId = weightArg.Id,
				WeightCalculatorId = weightCalculator.Id,
				BizUpdatedAt = 5303,
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

			await SvcStudyPlan.SetCurStudyPlanId(userCtx, studyPlan.Id, CT.None);
			var bo0 = await SvcStudyPlan.GetCurBoStudyPlan(userCtx, CT.None);
			var bo1 = await SvcStudyPlan.GetCurBoStudyPlan(userCtx, CT.None);
			if(bo0 is null || bo1 is null){
				throw new Exception("GetCurBoStudyPlan should not return null");
			}
			if(!object.ReferenceEquals(bo0, bo1)){
				throw new Exception("GetCurBoStudyPlan cache not hit");
			}
			if(bo0.PreFilter is null){
				throw new Exception("GetCurBoStudyPlan should parse prefilter json");
			}
			if(bo0.WeightArg is null || !bo0.WeightArg.ContainsKey("DfltAddBonus")){
				throw new Exception("GetCurBoStudyPlan should parse weight arg json");
			}
			if(bo0.WeightCalctr is null){
				throw new Exception("GetCurBoStudyPlan should build runtime weight calculator");
			}
			return NIL;
		});

		R("CurStudyPlan_GetCurWeightCalctr_Should_Calc_With_DictArg", async(o)=>{
			var userCtx = MkUserCtx(_ownerA);
			var weightCalculator = new PoWeightCalculator{
				Id = new IdWeightCalculator(),
				Owner = _ownerA,
				UniqName = _token + "_cur_wc_only",
				Type = EWeightCalculatorType.Js,
				Data = Encoding.UTF8.GetBytes("'[]'"),
			};
			var studyPlan = new PoStudyPlan{
				Id = new IdStudyPlan(),
				Owner = _ownerA,
				UniqName = _token + "_cur_wc_sp",
				Descr = "cur_wc_sp",
				WeightCalculatorId = weightCalculator.Id,
				BizUpdatedAt = 5401,
			};
			await RunNoTxn(async(ctx)=>{
				await RepoWeightCalculator.BatAdd(ctx, AsyE(weightCalculator), CT.None);
				await RepoStudyPlan.BatAdd(ctx, AsyE(studyPlan), CT.None);
				return NIL;
			});
			_weightCalculatorIds.Add(weightCalculator.Id);
			_studyPlanIds.Add(studyPlan.Id);

			await SvcStudyPlan.SetCurStudyPlanId(userCtx, studyPlan.Id, CT.None);
			var calctr = await SvcStudyPlan.GetCurWeightCalctr(userCtx, CT.None);
			if(calctr is null){
				throw new Exception("GetCurWeightCalctr should not return null");
			}
			var wr = await calctr.Calc(AsyE<IWordForLearn>(), new Dictionary<str, obj?>{{"x", 1}}, CT.None);
			var data = await ToList((IAsyncEnumerable<IWordWeightResult>)wr.Results!);
			if(data.Count != 0){
				throw new Exception("Expected empty result for empty input words");
			}
			return NIL;
		});
	}

	void RegisterDirectImplementedMethods(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestISvcStudyPlan)
			,[typeof(BoStudyPlan), typeof(PreFilter), typeof(WeightCalculator)]
			,[]
		);
		var R = register.Register;

		R("PreFilter_FromPo_Should_Parse_Json", async(o)=>{
			var json = "{\"Version\":\"1.0.0.0\",\"CoreFilter\":[{\"Fields\":[\"Lang\"],\"Filters\":[{\"Operation\":1,\"ValueType\":1,\"Values\":[\"English\"]}]}],\"PropFilter\":[]}";
			var po = new PoPreFilter{
				Type = EPreFilterType.Json,
				DataSchemaVer = new Version(1, 0),
				Data = Encoding.UTF8.GetBytes(json),
			};
			var preFilter = new PreFilter();
			preFilter.FromPo(po);
			if(preFilter.CoreFilter.Count == 0){
				throw new Exception("PreFilter.FromPo parse failed");
			}
			return NIL;
		});

		R("BoStudyPlan_FromJnStudyPlan_Should_Map_And_Build_Runtime", async(o)=>{
			var jn = new JnStudyPlan{
				StudyPlan = new PoStudyPlan{Id = new IdStudyPlan(), Owner = _ownerA},
				PreFilter = new PoPreFilter{
					Id = new IdPreFilter(),
					Owner = _ownerA,
					Type = EPreFilterType.Json,
					DataSchemaVer = new Version(1, 0),
					Data = Encoding.UTF8.GetBytes("{\"Version\":\"1.0.0.0\",\"CoreFilter\":[],\"PropFilter\":[]}"),
				},
				WeightArg = new PoWeightArg{
					Id = new IdWeightArg(),
					Owner = _ownerA,
					Type = EWeightArgType.Json,
					WeightCalculatorName = "x",
					Data = Encoding.UTF8.GetBytes("{\"DfltAddBonus\":777}"),
				},
				WeightCalculator = new PoWeightCalculator{
					Id = new IdWeightCalculator(),
					Owner = _ownerA,
					Type = EWeightCalculatorType.Js,
					Data = Encoding.UTF8.GetBytes("'[]'"),
				},
			};
			var bo = new BoStudyPlan();
			bo.FromJnStudyPlan(jn);
			if(bo.PoStudyPlan is null || bo.PreFilter is null || bo.WeightArg is null || bo.WeightCalctr is null){
				throw new Exception("BoStudyPlan.FromJnStudyPlan map/build failed");
			}
			var wr = await bo.WeightCalctr.Calc(AsyE<IWordForLearn>(), new Dictionary<str, obj?>(), CT.None);
			var data = await ToList((IAsyncEnumerable<IWordWeightResult>)wr.Results!);
			if(data.Count != 0){
				throw new Exception("Expected empty result for empty input words");
			}
			return NIL;
		});

		R("WeightCalculator_DictOverload_Should_Run_On_EmptyInput", async(o)=>{
			var calctr = new WeightCalculator();
			var wr = await calctr.Calc(AsyE<IWordForLearn>(), new Dictionary<str, obj?>{{"Base", 10d}}, CT.None);
			var data = await ToList((IAsyncEnumerable<IWordWeightResult>)wr.Results!);
			if(data.Count != 0){
				throw new Exception("WeightCalculator dict overload expected empty result on empty input");
			}
			return NIL;
		});
	}
}
