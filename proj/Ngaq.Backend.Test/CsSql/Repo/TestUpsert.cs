using Tsinswreng.CsTreeTest;
using Tsinswreng.CsSql;
using Ngaq.Core.Shared.Kv.Models;
using Ngaq.Core.Shared.User.Models.Po.User;
using Ngaq.Core.Shared.Base.Models.Po;
using Ngaq.Core.Infra.IF;
using Ngaq.Core.Shared.Word.Models.Po.Kv;

namespace Ngaq.Backend.Test.CsSql.Repo;

public partial class TestRepo{


	readonly List<PoKv> _batExistsUpsertSeed = new();
	readonly List<IdKv> _batExistsUpsertCleanupIds = new();
	IdKv? _batExistsUpsertSoftDeletedId = null;

	void RegisterBatExistsAndUpsert(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestRepo)
			,[typeof(IRepo<PoKv, IdKv>)]
			,[]
		);
		var R = register.Register;

		register.TesteeFnNames = [nameof(IRepo<PoKv, IdKv>.OrdAdd)];
		R("BatExistsUpsert_Insert_Seed", async(o)=>{
			return await RunInTxnIfNoCtx(async(Ctx)=>{
				var a = new PoKv{
					Id = new IdKv(),
					Owner = default,
					KType = EKvType.Str,
					KStr = "bat_exists_seed_k_a_" + System.Guid.NewGuid().ToString("N"),
					VType = EKvType.Str,
					VStr = "bat_exists_seed_v_a_" + System.Guid.NewGuid().ToString("N"),
				};
				var b = new PoKv{
					Id = new IdKv(),
					Owner = default,
					KType = EKvType.Str,
					KStr = "bat_exists_seed_k_b_" + System.Guid.NewGuid().ToString("N"),
					VType = EKvType.Str,
					VStr = "bat_exists_seed_v_b_" + System.Guid.NewGuid().ToString("N"),
				};

				await Repo.OrdAdd(Ctx, AsyE(a, b), CT.None);

				_batExistsUpsertSeed.Clear();
				_batExistsUpsertSeed.Add(a);
				_batExistsUpsertSeed.Add(b);

				_batExistsUpsertCleanupIds.Clear();
				_batExistsUpsertCleanupIds.Add(a.Id);
				_batExistsUpsertCleanupIds.Add(b.Id);
				return NIL;
			});
		});

		register.TesteeFnNames = [nameof(IRepo<PoKv, IdKv>.OrdExistsById)];
		R("BatExistsById_Existing_NonExisting_Existing", async(o)=>{
			if(_batExistsUpsertSeed.Count < 2){
				throw new Exception("BatExistsUpsert_Insert_Seed not executed");
			}

			return await RunInTxnIfNoCtx(async(Ctx)=>{
				var existingA = _batExistsUpsertSeed[0].Id;
				var existingB = _batExistsUpsertSeed[1].Id;
				var nonExisting = new IdKv();
				var ans = Repo.OrdExistsById(Ctx, AsyE(existingA, nonExisting, existingB), CT.None);

				var list = new List<bool>();
				await foreach(var one in ans){
					list.Add(one);
				}
				if(list.Count != 3){
					throw new Exception($"Expected 3 bool results, got {list.Count}");
				}
				if(list[0] != true || list[1] != false || list[2] != true){
					throw new Exception($"Expected [true,false,true], got [{list[0]},{list[1]},{list[2]}]");
				}
				return NIL;
			});
		});

		register.TesteeFnNames = [nameof(IRepo<PoKv, IdKv>.OrdExistsByIdWithDel), nameof(IRepo<PoKv, IdKv>.SoftDelInId)];
		R("BatExistsByIdWithDel_SoftDeleted_Should_Be_True", async(o)=>{
			if(_batExistsUpsertSeed.Count < 2){
				throw new Exception("BatExistsUpsert_Insert_Seed not executed");
			}

			return await RunInTxnIfNoCtx(async(Ctx)=>{
				var softDeletedId = _batExistsUpsertSeed[1].Id;
				var softDelResp = await Repo.SoftDelInId(Ctx, AsyE(softDeletedId), CT.None);
				if(softDelResp is null){
					throw new Exception("SoftDelInId returned null response");
				}
				_batExistsUpsertSoftDeletedId = softDeletedId;

				var ans = Repo.OrdExistsByIdWithDel(Ctx, AsyE(softDeletedId), CT.None);
				var list = new List<bool>();
				await foreach(var one in ans){
					list.Add(one);
				}
				if(list.Count != 1){
					throw new Exception($"Expected 1 bool result, got {list.Count}");
				}
				if(list[0] != true){
					throw new Exception("OrdExistsByIdWithDel should treat soft-deleted row as existing");
				}
				return NIL;
			});
		});

		register.TesteeFnNames = [nameof(IRepo<PoKv, IdKv>.OrdUpsert), nameof(IRepo<PoKv, IdKv>.OrdGetByIdWithDel)];
		R("BatUpsert_Insert_And_Update", async(o)=>{
			if(_batExistsUpsertSeed.Count < 2){
				throw new Exception("BatExistsUpsert_Insert_Seed not executed");
			}

			return await RunInTxnIfNoCtx(async(Ctx)=>{
				var existed = _batExistsUpsertSeed[0];
				var newOne = new PoKv{
					Id = new IdKv(),
					Owner = default,
					KType = EKvType.Str,
					KStr = "bat_upsert_new_k_" + System.Guid.NewGuid().ToString("N"),
					VType = EKvType.Str,
					VStr = "bat_upsert_new_v_" + System.Guid.NewGuid().ToString("N"),
				};

				var existedUpdated = new PoKv{
					Id = existed.Id,
					Owner = existed.Owner,
					KType = EKvType.Str,
					KStr = "bat_upsert_upd_k_" + System.Guid.NewGuid().ToString("N"),
					VType = EKvType.Str,
					VStr = "bat_upsert_upd_v_" + System.Guid.NewGuid().ToString("N"),
				};

				// 按需求，不檢查 Resp 的內容，只驗證最終資料效果。
				await Repo.OrdUpsert(Ctx, AsyE(existedUpdated, newOne), CT.None);

				_batExistsUpsertCleanupIds.Add(newOne.Id);

				var got = Repo.OrdGetByIdWithDel(Ctx, AsyE(existedUpdated.Id, newOne.Id), CT.None);
				var gotList = new List<PoKv?>();
				await foreach(var one in got){
					gotList.Add(one);
				}
				if(gotList.Count != 2){
					throw new Exception($"Expected 2 records, got {gotList.Count}");
				}

				var gotUpdated = gotList[0];
				var gotInserted = gotList[1];
				if(gotUpdated is null){
					throw new Exception("Expected updated record not null");
				}
				if(gotInserted is null){
					throw new Exception("Expected inserted record not null");
				}

				if(gotUpdated.KStr != existedUpdated.KStr || gotUpdated.VStr != existedUpdated.VStr){
					throw new Exception("Upsert update branch did not update expected fields");
				}
				if(gotInserted.KStr != newOne.KStr || gotInserted.VStr != newOne.VStr){
					throw new Exception("Upsert insert branch did not insert expected fields");
				}

				return NIL;
			});
		});

		register.TesteeFnNames = [nameof(IRepo<PoKv, IdKv>.OrdUpsert), nameof(IRepo<PoKv, IdKv>.OrdGetByIdWithDel)];
		R("BatUpsert_SoftDeletedSameId_Should_Update_Not_Insert", async(o)=>{
			if(_batExistsUpsertSoftDeletedId is null){
				throw new Exception("BatExistsByIdWithDel_SoftDeleted_Should_Be_True not executed");
			}

			return await RunInTxnIfNoCtx(async(Ctx)=>{
				var targetId = _batExistsUpsertSoftDeletedId.Value;
				var updated = new PoKv{
					Id = targetId,
					Owner = default,
					KType = EKvType.Str,
					KStr = "bat_upsert_soft_deleted_k_" + System.Guid.NewGuid().ToString("N"),
					VType = EKvType.Str,
					VStr = "bat_upsert_soft_deleted_v_" + System.Guid.NewGuid().ToString("N"),
				};

				await Repo.OrdUpsert(Ctx, AsyE(updated), CT.None);

				var got = Repo.OrdGetByIdWithDel(Ctx, AsyE(targetId), CT.None);
				var gotList = new List<PoKv?>();
				await foreach(var one in got){
					gotList.Add(one);
				}
				if(gotList.Count != 1){
					throw new Exception($"Expected 1 record, got {gotList.Count}");
				}

				var gotUpdated = gotList[0];
				if(gotUpdated is null){
					throw new Exception("Expected updated soft-deleted record not null");
				}
				if(gotUpdated.KStr != updated.KStr || gotUpdated.VStr != updated.VStr){
					throw new Exception("Upsert should update existing soft-deleted row by same id");
				}
				return NIL;
			});
		});

		register.TesteeFnNames = [nameof(IRepo<PoKv, IdKv>.OrdHardDelById)];
		R("BatExistsUpsert_Cleanup_HardDelete", async(o)=>{
			if(_batExistsUpsertCleanupIds.Count == 0){
				return NIL;
			}
			return await RunInTxnIfNoCtx(async(Ctx)=>{
				await Repo.OrdHardDelById(Ctx, AsyE(_batExistsUpsertCleanupIds.ToArray()), CT.None);
				return NIL;
			});
		});
	}
}
