using Tsinswreng.CsTreeTest;
using Tsinswreng.CsSql;
using Ngaq.Core.Shared.Kv.Models;
using Ngaq.Core.Sys.Models;
using Ngaq.Core.Shared.User.Models.Po.User;
using Ngaq.Core.Shared.Base.Models.Po;
using Ngaq.Core.Shared.Word.Models.Po.Kv;
namespace Ngaq.Local.Test.CsSql.Repo;

public partial class TestRepo{
	readonly List<PoKv> _delInIdEnts = new();
	readonly List<IdKv> _delInIdIds = new();

	void RegisterDelInId(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestRepo)
			,[typeof(IRepo<PoKv, IdKv>)]
			,[]
		);
		var R = register.Register;
		
		register.TesteeFnNames = [nameof(IRepo<PoKv, IdKv>.BatAdd)];
		R("DelInId_Insert_Multi", async(o)=>{
			return await RunInTxnIfNoCtx(async(Ctx)=>{
				var ents = new List<PoKv>();
				for(var i = 0; i < 3; i++){
					var e = new PoKv{
						Id = new IdKv(),
						Owner = default,
						KType = EKvType.Str,
						KStr = "del_in_id_k_" + System.Guid.NewGuid().ToString("N"),
						VType = EKvType.Str,
						VStr = "del_in_id_v_" + System.Guid.NewGuid().ToString("N"),
					};
					ents.Add(e);
				}

				var resp = await Repo.BatAdd(Ctx, AsyE(ents.ToArray()), CT.None);
				if(resp is null){
					throw new Exception("BatInsert returned null response");
				}

				_delInIdEnts.Clear();
				_delInIdIds.Clear();
				_delInIdEnts.AddRange(ents);
				_delInIdIds.AddRange(ents.Select(x=>x.Id));
				return NIL;
			});
		});

		register.TesteeFnNames = [
			nameof(IRepo<PoKv, IdKv>.SoftDelInId)
			,nameof(IRepo<PoKv, IdKv>.BatGetByIdWithDel)
		];
		R("SoftDelInId", async(o)=>{
			if(_delInIdIds.Count == 0){
				throw new Exception("DelInId_Insert_Multi not executed or no ids recorded");
			}
			return await RunInTxnIfNoCtx(async(Ctx)=>{
				var resp = await Repo.SoftDelInId(Ctx, AsyE(_delInIdIds.ToArray()), CT.None);
				if(resp is null){
					throw new Exception("SoftDelInId returned null response");
				}
				var verify = Repo.BatGetByIdWithDel(Ctx, AsyE(_delInIdIds.ToArray()), CT.None);
				var list = new List<PoKv?>();
				await foreach(var item in verify) list.Add(item);
				if(list.Count != _delInIdIds.Count){
					throw new Exception($"Expected {_delInIdIds.Count} entries, got {list.Count}");
				}
				for(var i = 0; i < list.Count; i++){
					var got = list[i];
					if(got is null){
						throw new Exception($"Expected non-null entity at index {i}");
					}
					if(!got.IsDeleted()){
						throw new Exception($"Expected IsDeleted at index {i}");
					}
				}
				return NIL;
			});
		});

		register.TesteeFnNames = [
			nameof(IRepo<PoKv, IdKv>.HardDelInId)
			,nameof(IRepo<PoKv, IdKv>.BatGetByIdWithDel)
		];
		R("HardDelInId", async(o)=>{
			if(_delInIdIds.Count == 0){
				throw new Exception("DelInId_Insert_Multi not executed or no ids recorded");
			}
			return await RunInTxnIfNoCtx(async(Ctx)=>{
				var resp = await Repo.HardDelInId(Ctx, AsyE(_delInIdIds.ToArray()), CT.None);
				if(resp is null){
					throw new Exception("HardDelInId returned null response");
				}
				var verify = Repo.BatGetByIdWithDel(Ctx, AsyE(_delInIdIds.ToArray()), CT.None);
				var list = new List<PoKv?>();
				await foreach(var item in verify) list.Add(item);
				if(list.Any(x=>x != null)){
					throw new Exception("Expected all nulls after hard delete");
				}
				return NIL;
			});
		});

		register.TesteeFnNames = [
			nameof(IRepo<PoKv, IdKv>.BatHardDelById)
			,nameof(IRepo<PoKv, IdKv>.BatGetByIdWithDel)
		];
		R("DelInId_Cleanup_HardDelete", async(o)=>{
			if(_delInIdIds.Count == 0){
				return NIL;
			}
			return await RunInTxnIfNoCtx(async(Ctx)=>{
				var resp = await Repo.BatHardDelById(Ctx, AsyE(_delInIdIds.ToArray()), CT.None);
				if(resp is null){
					throw new Exception("BatHardDelById returned null response");
				}
				var verify = Repo.BatGetByIdWithDel(Ctx, AsyE(_delInIdIds.ToArray()), CT.None);
				var list = new List<PoKv?>();
				await foreach(var item in verify) list.Add(item);
				if(list.Any(x=>x != null)){
					throw new Exception("Expected all nulls after hard delete");
				}
				return NIL;
			});
		});
	}
}
