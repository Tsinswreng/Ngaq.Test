using Tsinswreng.CsTreeTest;
using Tsinswreng.CsSql;
using Ngaq.Core.Shared.Kv.Models;
using Ngaq.Core.Shared.User.Models.Po.User;
using Ngaq.Core.Shared.Word.Models.Po.Kv;
using Ngaq.Core.Shared.Base.Models.Po;
using Ngaq.Core.Infra.IF;
namespace Ngaq.Backend.Test.CsSql.Repo;

public partial class TestRepo{
	readonly List<IdKv> _slctManyIds = new();
	IdKv? _slctManySoftDeletedId = null;

	void RegisterSlctManyInIdsWithDel(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestRepo)
			,[typeof(IRepo<PoKv, IdKv>)]
			,[]
		);
		var R = register.Register;

		register.TesteeFnNames = [nameof(IRepo<PoKv, IdKv>.OrdAdd)];
		R("SlctManyInIds_Insert_Multi", async(o)=>{
			return await RunInTxnIfNoCtx(async(Ctx)=>{
				var ents = new List<PoKv>();
				for(var i = 0; i < 3; i++){
					ents.Add(new PoKv{
						Id = new IdKv(),
						Owner = IdUser.Zero,
						KType = EKvType.Str,
						KStr = "slct_many_k_" + System.Guid.NewGuid().ToString("N"),
						VType = EKvType.Str,
						VStr = "slct_many_v_" + System.Guid.NewGuid().ToString("N"),
					});
				}

				var resp = await Repo.OrdAdd(Ctx, AsyE(ents.ToArray()), CT.None);
				if(resp is null){
					throw new Exception("BatAdd returned null response");
				}

				_slctManyIds.Clear();
				_slctManyIds.AddRange(ents.Select(x=>x.Id));
				_slctManySoftDeletedId = _slctManyIds[0];
				return NIL;
			});
		});

		register.TesteeFnNames = [nameof(IRepo<PoKv, IdKv>.SoftDelInId)];
		R("SlctManyInIds_SoftDelete_One", async(o)=>{
			if(_slctManySoftDeletedId is null){
				throw new Exception("SlctManyInIds_Insert_Multi not executed");
			}
			return await RunInTxnIfNoCtx(async(Ctx)=>{
				var resp = await Repo.SoftDelInId(Ctx, AsyE(_slctManySoftDeletedId.Value), CT.None);
				if(resp is null){
					throw new Exception("SoftDelInId returned null response");
				}
				return NIL;
			});
		});

		register.TesteeFnNames = [nameof(IRepo<PoKv, IdKv>.GetInId)];
		R("SlctManyInIds_NonWithDel_Exclude_SoftDeleted", async(o)=>{
			if(_slctManyIds.Count == 0 || _slctManySoftDeletedId is null){
				throw new Exception("SlctManyInIds_Insert_Multi not executed");
			}
			return await RunInTxnIfNoCtx(async(Ctx)=>{
				var result = Repo.GetInId(Ctx, AsyE(_slctManyIds.ToArray()), CT.None);
				var list = new List<PoKv?>();
				await foreach(var item in result){
					list.Add(item);
				}

				var gotIds = list.Where(x=>x is not null).Select(x=>x!.Id).ToHashSet();
				if(gotIds.Contains(_slctManySoftDeletedId.Value)){
					throw new Exception("GetManyInId should not return soft-deleted rows");
				}
				if(gotIds.Count != _slctManyIds.Count - 1){
					throw new Exception($"Expected {_slctManyIds.Count - 1} non-deleted rows, got {gotIds.Count}");
				}
				return NIL;
			});
		});

		register.TesteeFnNames = [nameof(IRepo<PoKv, IdKv>.GetInIdsWithDel)];
		R("SlctManyInIds_WithDel_Include_SoftDeleted", async(o)=>{
			if(_slctManyIds.Count == 0 || _slctManySoftDeletedId is null){
				throw new Exception("SlctManyInIds_Insert_Multi not executed");
			}
			return await RunInTxnIfNoCtx(async(Ctx)=>{
				var result = Repo.GetInIdsWithDel(Ctx, AsyE(_slctManyIds.ToArray()), CT.None);
				var list = new List<PoKv?>();
				await foreach(var item in result){
					list.Add(item);
				}

				var gotIds = list.Where(x=>x is not null).Select(x=>x!.Id).ToHashSet();
				if(gotIds.Count != _slctManyIds.Count){
					throw new Exception($"Expected {_slctManyIds.Count} rows, got {gotIds.Count}");
				}

				var del = list.FirstOrDefault(x=>x is not null && x.Id.Equals(_slctManySoftDeletedId.Value));
				if(del is null){
					throw new Exception("GetManyInIdWithDel should include soft-deleted row");
				}
				if(!del.IsDeleted()){
					throw new Exception("Returned soft-deleted row should be marked deleted");
				}
				return NIL;
			});
		});

		register.TesteeFnNames = [nameof(IRepo<PoKv, IdKv>.GetInIdsWithDel)];
		R("SlctManyInIdsWithDel_EmptyIds_ReturnsEmpty", async(o)=>{
			return await RunInTxnIfNoCtx(async(Ctx)=>{
				var Result = Repo.GetInIdsWithDel(Ctx, AsyE<IdKv>(), CT.None);
				var List = new List<PoKv?>();
				await foreach(var Item in Result) List.Add(Item);
				if(List.Count != 0){
					throw new Exception($"Expected empty, got {List.Count}");
				}
				return NIL;
			});
		});

		register.TesteeFnNames = [nameof(IRepo<PoKv, IdKv>.GetInIdsWithDel)];
		R("SlctManyInIdsWithDel_NonExistIds_ReturnsEmpty", async(o)=>{
			return await RunInTxnIfNoCtx(async(Ctx)=>{
				var Result = Repo.GetInIdsWithDel(Ctx, AsyE(new IdKv(), new IdKv()), CT.None);
				var List = new List<PoKv?>();
				await foreach(var Item in Result) List.Add(Item);
				if(List.Count != 0){
					throw new Exception($"Expected empty for non-existent ids, got {List.Count}");
				}
				return NIL;
			});
		});

		register.TesteeFnNames = [nameof(IRepo<PoKv, IdKv>.OrdHardDelById)];
		R("SlctManyInIds_Cleanup_HardDelete", async(o)=>{
			if(_slctManyIds.Count == 0){
				return NIL;
			}
			return await RunInTxnIfNoCtx(async(Ctx)=>{
				await Repo.OrdHardDelById(Ctx, AsyE(_slctManyIds.ToArray()), CT.None);
				return NIL;
			});
		});
	}
}
