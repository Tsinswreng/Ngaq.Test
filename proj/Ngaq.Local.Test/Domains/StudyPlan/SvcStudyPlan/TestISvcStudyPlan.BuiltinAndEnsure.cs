using Ngaq.Core.Shared.StudyPlan.Models;
using Ngaq.Core.Shared.StudyPlan.Svc;
using Ngaq.Core.Shared.Word.WeightAlgo;
using Ngaq.Core.Shared.Word.WeightAlgo.Models;
using Ngaq.Core.Tools;
using Tsinswreng.CsTools;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Local.Test.Domains.StudyPlan;

public partial class TestISvcStudyPlan{
	void RegisterBuiltinAndEnsureApis(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestISvcStudyPlan)
			,[typeof(ISvcStudyPlan)]
			,[]
		);
		var R = register.Register;
		register.TesteeFnNames = [
			nameof(ISvcStudyPlan.GetDfltStudyPlan)
			,nameof(ISvcStudyPlan.EnsureCurStudyPlan)
		];
		
		//人工
		R("DfltWeightCfgJson",async(o)=>{
			var cfg = new DfltWeightCfg();
			var json = JsonS.Stringify(cfg);
			var dict = ToolJson.JsonStrToDict(json);
			if(dict is null || dict.Count == 0){
				throw new Exception("DfltWeightCfgJson: dict should not be null or empty");
			}
			return NIL;
		});
		R("GetBuiltinStudyPlan_Should_Have_Prefix_And_Defaults", async(o)=>{
			var userCtx = MkUserCtx(_ownerA);
			var bo = await SvcStudyPlan.GetDfltStudyPlan(userCtx, CT.None);

			if(bo.PoStudyPlan is not { } poSp){
				throw new Exception("GetBuiltinStudyPlan PoStudyPlan should not be null");
			}
			if(!poSp.UniqName.StartsWith(Consts.BuiltinPrefix)){
				throw new Exception($"GetBuiltinStudyPlan UniqName should start with {Consts.BuiltinPrefix}, got {poSp.UniqName}");
			}
			if(bo.WeightCalctr is null){
				throw new Exception("GetBuiltinStudyPlan WeightCalctr should not be null");
			}
			if(bo.WeightArg is null){
				throw new Exception("GetBuiltinStudyPlan WeightArg should not be null");
			}
			if(bo.WeightArg.Count == 0){
				throw new Exception("GetBuiltinStudyPlan WeightArg should not be empty");
			}
			if(bo.PoWeightCalculator is not { } poWc){
				throw new Exception("GetBuiltinStudyPlan PoWeightCalculator should not be null");
			}
			if(!poWc.UniqName.StartsWith(Consts.BuiltinPrefix)){
				throw new Exception($"GetBuiltinStudyPlan WeightCalculator UniqName should start with {Consts.BuiltinPrefix}");
			}
			if(bo.PoWeightArg is not { } poWa){
				throw new Exception("GetBuiltinStudyPlan PoWeightArg should not be null");
			}
			if(!poWa.UniqName.StartsWith(Consts.BuiltinPrefix)){
				throw new Exception($"GetBuiltinStudyPlan WeightArg UniqName should start with {Consts.BuiltinPrefix}");
			}
			return NIL;
		});

		R("EnsureCurStudyPlan_Should_Create_Builtin_When_None", async(o)=>{
			var hasBefore = await SvcStudyPlan.GetCurStudyPlanId(MkUserCtx(_ownerB), CT.None);
			if(hasBefore is not null){
				throw new Exception("EnsureCurStudyPlan: user should have no plan before ensure");
			}

			var created = await SvcStudyPlan.EnsureCurStudyPlan(MkUserCtx(_ownerB), CT.None);

			var curId = await SvcStudyPlan.GetCurStudyPlanId(MkUserCtx(_ownerB), CT.None);
			if(curId is null){
				throw new Exception("EnsureCurStudyPlan should set current plan id");
			}

			var jn = await SvcStudyPlan.GetCurJnStudyPlan(MkUserCtx(_ownerB), CT.None);
			if(jn is null){
				throw new Exception("EnsureCurStudyPlan should create a retrievable plan");
			}
			// if(jn.StudyPlan is not { } spBuiltin || !spBuiltin.UniqName.StartsWith(Consts.BuiltinPrefix)){
			// 	throw new Exception(@$"
			// 	EnsureCurStudyPlan created plan should have builtin prefix
			// 	{jn.StudyPlan},
			// 	{jn.StudyPlan.UniqName}
			// 	");
			// }
			_studyPlanIds.Add(jn.StudyPlan.Id);
			return NIL;
		});

		R("EnsureCurStudyPlan_Should_Not_Recreate_When_Exists", async(o)=>{
			var curIdBefore = await SvcStudyPlan.GetCurStudyPlanId(MkUserCtx(_ownerA), CT.None);
			if(curIdBefore is null){
				throw new Exception("EnsureCurStudyPlan: userA should already have plan from seed data");
			}

			var created = await SvcStudyPlan.EnsureCurStudyPlan(MkUserCtx(_ownerA), CT.None);
			if(created){
				throw new Exception("EnsureCurStudyPlan should return false when plan already exists");
			}

			var curIdAfter = await SvcStudyPlan.GetCurStudyPlanId(MkUserCtx(_ownerA), CT.None);
			if(curIdAfter is null || curIdAfter != curIdBefore){
				throw new Exception("EnsureCurStudyPlan should not change existing plan");
			}
			return NIL;
		});
	}
}
