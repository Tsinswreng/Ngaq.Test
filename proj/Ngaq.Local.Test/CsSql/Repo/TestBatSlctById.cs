using Tsinswreng.CsTreeTest;
using Tsinswreng.CsSql;
using Ngaq.Core.Shared.Kv.Models;
using Ngaq.Core.Shared.User.Models.Po.User;
using Ngaq.Core.Infra.IF;
using Ngaq.Core.Shared.Word.Models.Po.Kv;
using Ngaq.Core.Shared.Base.Models.Po;
namespace Ngaq.Local.Test.CsSql.Repo;

public partial class TestRepo{
	readonly List<IdKv> _batSlctIds = new();
	IdKv? _batSlctSoftDeletedId = null;

	void RegisterBatSlctById(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestRepo)
			,[typeof(IRepo<PoKv, IdKv>)]
			,[]
		);
		var R = register.Register;

		register.TesteeFnNames = [nameof(IRepo<PoKv, IdKv>.BatAdd)];
		R("BatSlctById_Insert_Multi", async(o)=>{
			return await RunInTxnIfNoCtx(async(Ctx)=>{
				var ents = new List<PoKv>();
				for(var i = 0; i < 3; i++){
					ents.Add(new PoKv{
						Id = new IdKv(),
						Owner = IdUser.Zero,
						KType = EKvType.Str,
						KStr = "bat_slct_k_" + System.Guid.NewGuid().ToString("N"),
						VType = EKvType.Str,
						VStr = "bat_slct_v_" + System.Guid.NewGuid().ToString("N"),
					});
				}

				var resp = await Repo.BatAdd(Ctx, AsyE(ents.ToArray()), CT.None);
				if(resp is null){
					throw new Exception("BatAdd returned null response");
				}

				_batSlctIds.Clear();
				_batSlctIds.AddRange(ents.Select(x=>x.Id));
				_batSlctSoftDeletedId = _batSlctIds[0];
				return NIL;
			});
		});

		register.TesteeFnNames = [nameof(IRepo<PoKv, IdKv>.SoftDelInId)];
		R("BatSlctById_SoftDelete_One", async(o)=>{
			if(_batSlctSoftDeletedId is null){
				throw new Exception("BatSlctById_Insert_Multi not executed");
			}
			return await RunInTxnIfNoCtx(async(Ctx)=>{
				var resp = await Repo.SoftDelInId(Ctx, AsyE(_batSlctSoftDeletedId.Value), CT.None);
				if(resp is null){
					throw new Exception("SoftDelInId returned null response");
				}
				return NIL;
			});
		});

		register.TesteeFnNames = [nameof(IRepo<PoKv, IdKv>.BatGetById)];
		R("BatSlctById_NonWithDel_Should_Return_Null_For_SoftDeleted", async(o)=>{
			if(_batSlctIds.Count == 0 || _batSlctSoftDeletedId is null){
				throw new Exception("BatSlctById_Insert_Multi not executed");
			}
			return await RunInTxnIfNoCtx(async(Ctx)=>{
				var result = Repo.BatGetById(Ctx, AsyE(_batSlctIds.ToArray()), CT.None);
				var list = new List<PoKv?>();
				await foreach(var item in result){
					list.Add(item);
				}
				if(list.Count != _batSlctIds.Count){
					throw new Exception($"Expected {_batSlctIds.Count} entries, got {list.Count}");
				}

				var softIdx = _batSlctIds.FindIndex(x=>x.Equals(_batSlctSoftDeletedId.Value));
				if(softIdx < 0){
					throw new Exception("soft-deleted id not found in test ids");
				}
				if(list[softIdx] is not null){
					throw new Exception("BatGetById should return null for soft-deleted row");
				}
				return NIL;
			});
		});

		register.TesteeFnNames = [nameof(IRepo<PoKv, IdKv>.BatGetByIdWithDel)];
		R("BatSlctById_EmptyIds_ReturnsEmpty", async(o)=>{
			return await RunInTxnIfNoCtx(async(Ctx)=>{
				var Result = Repo.BatGetByIdWithDel(Ctx, AsyE<IdKv>(), CT.None);
				var List = new List<PoKv?>();
				await foreach(var Item in Result) List.Add(Item);
				if(List.Count != 0){
					throw new Exception($"Expected empty, got {List.Count}");
				}
				return NIL;
			});
		});

		register.TesteeFnNames = [nameof(IRepo<PoKv, IdKv>.BatGetByIdWithDel)];
		R("BatSlctById_WithDel_Should_Return_SoftDeleted", async(o)=>{
			if(_batSlctIds.Count == 0 || _batSlctSoftDeletedId is null){
				throw new Exception("BatSlctById_Insert_Multi not executed");
			}
			return await RunInTxnIfNoCtx(async(Ctx)=>{
				var result = Repo.BatGetByIdWithDel(Ctx, AsyE(_batSlctIds.ToArray()), CT.None);
				var list = new List<PoKv?>();
				await foreach(var item in result){
					list.Add(item);
				}
				if(list.Count != _batSlctIds.Count){
					throw new Exception($"Expected {_batSlctIds.Count} entries, got {list.Count}");
				}

				var softIdx = _batSlctIds.FindIndex(x=>x.Equals(_batSlctSoftDeletedId.Value));
				if(softIdx < 0){
					throw new Exception("soft-deleted id not found in test ids");
				}
				var softOne = list[softIdx];
				if(softOne is null){
					throw new Exception("BatGetByIdWithDel should return soft-deleted row");
				}
				if(!softOne.IsDeleted()){
					throw new Exception("Returned soft-deleted row should be marked deleted");
				}
				return NIL;
			});
		});

		register.TesteeFnNames = [nameof(IRepo<PoKv, IdKv>.BatGetByIdWithDel)];
		R("BatSlctById_NonExistIds_ReturnsNulls", async(o)=>{
			return await RunInTxnIfNoCtx(async(Ctx)=>{
				var Id1 = new IdKv();
				var Id2 = new IdKv();
				var Result = Repo.BatGetByIdWithDel(Ctx, AsyE(Id1, Id2), CT.None);
				var List = new List<PoKv?>();
				await foreach(var Item in Result) List.Add(Item);
				if(List.Count != 2){
					throw new Exception($"Expected 2 entries (one per id), got {List.Count}");
				}
				if(List.Any(x => x != null)){
					throw new Exception("Expected all nulls for non-existent IDs");
				}
				return NIL;
			});
		});

		register.TesteeFnNames = [nameof(IRepo<PoKv, IdKv>.BatHardDelById)];
		R("BatSlctById_Cleanup_HardDelete", async(o)=>{
			if(_batSlctIds.Count == 0){
				return NIL;
			}
			return await RunInTxnIfNoCtx(async(Ctx)=>{
				await Repo.BatHardDelById(Ctx, AsyE(_batSlctIds.ToArray()), CT.None);
				return NIL;
			});
		});
	}
}
