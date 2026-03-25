using Tsinswreng.CsTreeTest;
using Tsinswreng.CsSql;
using Ngaq.Core.Shared.Kv.Models;
using Ngaq.Core.Sys.Models;
using System.Collections.Generic;
using System.Linq;
using Ngaq.Core.Shared.User.Models.Po.User;
using Ngaq.Core.Infra.IF;
using Ngaq.Core.Shared.Word.Models.Po.Kv;

namespace Ngaq.Local.Test.CsSql.Repo;

public partial class TestRepo{
	readonly List<PoKv> _batInsertEnts = new();
	readonly List<IdKv> _batInsertIds = new();

	void RegisterBatInsert(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestRepo)
			,[typeof(IRepo<PoKv, IdKv>)]
			,[]
		);
		var R = register.Register;

		register.TesteeFnNames = [nameof(IRepo<PoKv, IdKv>.BatAdd)];
		R("BatInsert_Insert_Multi", async(o)=>{
			return await RunInTxnIfNoCtx(async(Ctx)=>{
				var ents = new List<PoKv>();
				for(var i = 0; i < 3; i++){
					var e = new PoKv{
						Id = new IdKv(),
						Owner = IdUser.Zero,
						KType = EKvType.Str,
						KStr = "bat_insert_k_" + System.Guid.NewGuid().ToString("N"),
						VType = EKvType.Str,
						VStr = "bat_insert_v_" + System.Guid.NewGuid().ToString("N"),
					};
					ents.Add(e);
				}

				var resp = await Repo.BatAdd(Ctx, AsyE(ents.ToArray()), CT.None);
				if(resp is null){
					throw new Exception("BatInsert returned null response");
				}

				_batInsertEnts.Clear();
				_batInsertIds.Clear();
				_batInsertEnts.AddRange(ents);
				_batInsertIds.AddRange(ents.Select(x=>x.Id));
				return NIL;
			});
		});

		register.TesteeFnNames = [nameof(IRepo<PoKv, IdKv>.BatGetByIdWithDel)];
		R("BatInsert_Verify_BatSlctById", async(o)=>{
			if(_batInsertIds.Count == 0){
				throw new Exception("BatInsert_Insert_Multi not executed or no ids recorded");
			}

			return await RunInTxnIfNoCtx(async(Ctx)=>{
				var result = Repo.BatGetByIdWithDel(Ctx, AsyE(_batInsertIds.ToArray()), CT.None);
				var list = new List<PoKv?>();
				await foreach(var item in result) list.Add(item);

				if(list.Count != _batInsertIds.Count){
					throw new Exception($"Expected {_batInsertIds.Count} entries, got {list.Count}");
				}

				for(var i = 0; i < list.Count; i++){
					var got = list[i];
					if(got is null){
						throw new Exception($"Expected non-null entity at index {i}");
					}
					var exp = _batInsertEnts[i];
					if(!got.Id.Equals(exp.Id)){
						throw new Exception($"Id mismatch at index {i}");
					}
					if(got.KStr != exp.KStr || got.VStr != exp.VStr){
						throw new Exception($"Value mismatch at index {i}");
					}
				}
				return NIL;
			});
		});

		register.TesteeFnNames = [nameof(IRepo<PoKv, IdKv>.GetManyInIdWithDel)];
		R("BatInsert_Verify_SlctManyInIdsWithDel", async(o)=>{
			if(_batInsertIds.Count == 0){
				throw new Exception("BatInsert_Insert_Multi not executed or no ids recorded");
			}

			return await RunInTxnIfNoCtx(async(Ctx)=>{
				var result = Repo.GetManyInIdWithDel(Ctx, AsyE(_batInsertIds.ToArray()), CT.None);
				var list = new List<PoKv?>();
				await foreach(var item in result) list.Add(item);

				if(list.Count == 0){
					throw new Exception("Expected non-empty result");
				}

				var expected = new HashSet<IdKv>(_batInsertIds);
				foreach(var item in list){
					if(item is null){
						throw new Exception("Expected non-null entity");
					}
					expected.Remove(item.Id);
				}
				if(expected.Count != 0){
					throw new Exception($"Missing {expected.Count} inserted ids in SlctManyInIdsWithDel");
				}
				return NIL;
			});
		});

		register.TesteeFnNames = [
			nameof(IRepo<PoKv, IdKv>.BatHardDelById)
			,nameof(IRepo<PoKv, IdKv>.BatGetByIdWithDel)
		];
		R("BatInsert_Cleanup_HardDelete", async(o)=>{
			if(_batInsertIds.Count == 0){
				return NIL;
			}

			return await RunInTxnIfNoCtx(async(Ctx)=>{
				var resp = await Repo.BatHardDelById(Ctx, AsyE(_batInsertIds.ToArray()), CT.None);
				if(resp is null){
					throw new Exception("BatHardDelById returned null response");
				}

				var verify = Repo.BatGetByIdWithDel(Ctx, AsyE(_batInsertIds.ToArray()), CT.None);
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
