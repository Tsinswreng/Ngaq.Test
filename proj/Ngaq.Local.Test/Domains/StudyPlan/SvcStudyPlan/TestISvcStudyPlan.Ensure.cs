using Ngaq.Core.Shared.StudyPlan.Models;
using Ngaq.Core.Shared.StudyPlan.Models.Po.StudyPlan;
using Ngaq.Core.Shared.StudyPlan.Models.Po.WeightCalculator;
using Ngaq.Core.Shared.User.Models.Po.User;
using Ngaq.Core.Shared.User.UserCtx;
using Ngaq.Core.Shared.Word.WeightAlgo;
using Ngaq.Core.Tools;
using Ngaq.Core.Infra;
using Tsinswreng.CsTools;
using Tsinswreng.CsTreeTest;
using Ngaq.Core.Infra.IF;

namespace Ngaq.Local.Test.Domains.StudyPlan;

public partial class TestISvcStudyPlan{
	void RegisterEnsureMethods(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestISvcStudyPlan)
			,[typeof(Ngaq.Core.Shared.StudyPlan.Svc.ISvcStudyPlan)]
			,[]
		);
		var R = register.Register;
		register.TesteeFnNames = [
			nameof(Ngaq.Core.Shared.StudyPlan.Svc.ISvcStudyPlan.EnsureBuiltinStudyPlan)
			,nameof(Ngaq.Core.Shared.StudyPlan.Svc.ISvcStudyPlan.EnsureCurStudyPlan)
		];

		R("EnsureBuiltinStudyPlan_Should_Create_If_Not_Exists", async(o)=>{
			var userCtx = MkUserCtx(_ownerA);
			var builtinName = Consts.BuiltinPrefix + DfltWeightCalculator.Name;

			// 先清掉可能已有的內置權重算法 避免影響測試
			await RunNoTxn(async(ctx)=>{
				var all = await RepoWeightCalculator.GetAll(ctx, CT.None).ToListAsync(CT.None);
				var existing = all.Where(x => x.UniqName == builtinName).ToList();
				if(existing.Count > 0){
					await RepoWeightCalculator.BatHardDelById(
						ctx, ToolAsyE.ToAsyE(existing.Select(x => x.Id).ToArray()), CT.None
					);
					// 從追蹤列表中移除 已被清理
					foreach(var e in existing){
						_weightCalculatorIds.Remove(e.Id);
					}
				}
				return NIL;
			});

			// 第一次調用: 應該創建
			var created = await SvcStudyPlan.EnsureBuiltinStudyPlan(userCtx, CT.None);
			if(!created){
				throw new Exception("EnsureBuiltinStudyPlan first call should return true (created)");
			}

			// 追蹤創建的內置權重算法id 以便清理
			await RunNoTxn(async(ctx)=>{
				var all = await RepoWeightCalculator.GetAll(ctx, CT.None).ToListAsync(CT.None);
				var found = all.FirstOrDefault(x => x.UniqName == builtinName);
				if(found != null && !_weightCalculatorIds.Contains(found.Id)){
					_weightCalculatorIds.Add(found.Id);
				}
				return NIL;
			});

			// 第二次調用: 已存在 應返回false
			var alreadyExists = await SvcStudyPlan.EnsureBuiltinStudyPlan(userCtx, CT.None);
			if(alreadyExists){
				throw new Exception("EnsureBuiltinStudyPlan second call should return false (already exists)");
			}

			return NIL;
		});

		R("EnsureBuiltinStudyPlan_Should_Have_BuiltinPrefix_And_SystemOwner", async(o)=>{
			var userCtx = MkUserCtx(_ownerA);
			var builtinName = Consts.BuiltinPrefix + DfltWeightCalculator.Name;

			// 確保已創建
			await SvcStudyPlan.EnsureBuiltinStudyPlan(userCtx, CT.None);

			// 查出來驗證
			await RunNoTxn(async(ctx)=>{
				var all = await RepoWeightCalculator.GetAll(ctx, CT.None).ToListAsync(CT.None);
				var found = all.FirstOrDefault(x => x.UniqName == builtinName);
				if(found == null){
					throw new Exception("Builtin weight calculator not found in DB");
				}
				if(found.Owner != IdUser.Zero){
					throw new Exception("Builtin weight calculator owner should be IdUser.Zero (system)");
				}
				if(found.Type != EWeightCalculatorType.Builtin){
					throw new Exception("Builtin weight calculator type should be Builtin");
				}
				return NIL;
			});

			return NIL;
		});

		R("EnsureCurStudyPlan_Should_Create_Default_When_None", async(o)=>{
			var userCtx = MkUserCtx(_ownerB);

			// 確保用戶沒有當前學習方案
			var curIdBefore = await SvcStudyPlan.GetCurStudyPlanId(userCtx, CT.None);
			if(curIdBefore is IdStudyPlan id0 && !id0.IsNullOrDefault()){
				throw new Exception("User should not have current study plan before EnsureCurStudyPlan");
			}

			// 調用 EnsureCurStudyPlan 應該創建默認方案並設為當前
			var created = await SvcStudyPlan.EnsureCurStudyPlan(userCtx, CT.None);
			if(!created){
				throw new Exception("EnsureCurStudyPlan should return true when creating default plan");
			}

			// 驗證當前學習方案已被設置
			var curIdAfter = await SvcStudyPlan.GetCurStudyPlanId(userCtx, CT.None);
			if(curIdAfter is not IdStudyPlan id1 || id1.IsNullOrDefault()){
				throw new Exception("EnsureCurStudyPlan should set current study plan id");
			}

			// 驗證學習方案的屬性
			var jn = await SvcStudyPlan.GetCurJnStudyPlan(userCtx, CT.None);
			if(jn == null){
				throw new Exception("EnsureCurStudyPlan should create a retrievable study plan");
			}
			if(jn.StudyPlan.Owner != _ownerB){
				throw new Exception("EnsureCurStudyPlan created plan should belong to user");
			}

			// 記錄id以便清理
			_studyPlanIds.Add(jn.StudyPlan.Id);

			return NIL;
		});

		R("EnsureCurStudyPlan_Should_Skip_When_Already_Set", async(o)=>{
			var userCtx = MkUserCtx(_ownerB);

			// _ownerB 在上一個用例中已設置了當前學習方案
			var curIdBefore = await SvcStudyPlan.GetCurStudyPlanId(userCtx, CT.None);
			if(curIdBefore is not IdStudyPlan || ((IdStudyPlan)curIdBefore).IsNullOrDefault()){
				throw new Exception("_ownerB should already have current study plan from previous test");
			}

			// 再次調用應該返回false (已存在 不需創建)
			var created = await SvcStudyPlan.EnsureCurStudyPlan(userCtx, CT.None);
			if(created){
				throw new Exception("EnsureCurStudyPlan should return false when current plan already exists");
			}

			// 當前方案id不應改變
			var curIdAfter = await SvcStudyPlan.GetCurStudyPlanId(userCtx, CT.None);
			if(curIdBefore != curIdAfter){
				throw new Exception("EnsureCurStudyPlan should not change existing current plan id");
			}

			return NIL;
		});

		R("EnsureCurStudyPlan_Should_Call_EnsureBuiltin", async(o)=>{
			// 用新用戶測試 確保路徑完整
			var newOwner = new IdUser();
			var userCtx = MkUserCtx(newOwner);

			// 先清掉內置權重算法
			await RunNoTxn(async(ctx)=>{
				var all = await RepoWeightCalculator.GetAll(ctx, CT.None).ToListAsync(CT.None);
				var builtinName = Consts.BuiltinPrefix + DfltWeightCalculator.Name;
				var existing = all.Where(x => x.UniqName == builtinName).ToList();
				if(existing.Count > 0){
					await RepoWeightCalculator.BatHardDelById(
						ctx, ToolAsyE.ToAsyE(existing.Select(x => x.Id).ToArray()), CT.None
					);
					foreach(var e in existing){
						_weightCalculatorIds.Remove(e.Id);
					}
				}
				return NIL;
			});

			// EnsureCurStudyPlan 內部會調用 EnsureBuiltinStudyPlan
			var created = await SvcStudyPlan.EnsureCurStudyPlan(userCtx, CT.None);
			if(!created){
				throw new Exception("EnsureCurStudyPlan should return true for new user");
			}

			// 驗證內置權重算法也被創建了 並追蹤id
			var builtinName2 = Consts.BuiltinPrefix + DfltWeightCalculator.Name;
			await RunNoTxn(async(ctx)=>{
				var all = await RepoWeightCalculator.GetAll(ctx, CT.None).ToListAsync(CT.None);
				var found = all.FirstOrDefault(x => x.UniqName == builtinName2);
				if(found == null){
					throw new Exception("EnsureCurStudyPlan should call EnsureBuiltinStudyPlan internally");
				}
				if(!_weightCalculatorIds.Contains(found.Id)){
					_weightCalculatorIds.Add(found.Id);
				}
				return NIL;
			});

			// 驗證學習方案引用了內置權重算法
			var jn = await SvcStudyPlan.GetCurJnStudyPlan(userCtx, CT.None);
			if(jn == null){
				throw new Exception("EnsureCurStudyPlan should create a study plan for new user");
			}
			if(jn.StudyPlan.WeightCalculatorId.IsNullOrDefault()){
				throw new Exception("EnsureCurStudyPlan created plan should reference builtin weight calculator");
			}

			// 記錄id以便清理
			_studyPlanIds.Add(jn.StudyPlan.Id);

			return NIL;
		});
	}
}
