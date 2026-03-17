using Tsinswreng.CsTreeTest;
using Tsinswreng.CsSql;
using Ngaq.Core.Shared.Kv.Models;
using Ngaq.Core.Sys.Models;
using Ngaq.Core.Shared.User.Models.Po.User;
using Ngaq.Core.Shared.Word.Models.Po.Kv;
namespace Ngaq.Local.Test.CsSql.Repo;

public partial class TestRepo{
	readonly List<PoKv> _batUpdEnts = new();
	readonly List<IdKv> _batUpdIds = new();

	void RegisterBatUpd(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestRepo)
			,[typeof(IRepo<PoKv, IdKv>)]
			,[]
		);
		var R = register.Register;

		register.TesteeFnNames = [nameof(IRepo<PoKv, IdKv>.BatInsert)];
		R("BatUpd_Insert_Multi", async(o)=>{
			var Ctx = new DbFnCtx();
			var ents = new List<PoKv>();
			for(var i = 0; i < 3; i++){
				var e = new PoKv{
					Id = new IdKv(),
					Owner = default,
					KType = EKvType.Str,
					KStr = "bat_upd_k_" + System.Guid.NewGuid().ToString("N"),
					VType = EKvType.Str,
					VStr = "bat_upd_v_" + System.Guid.NewGuid().ToString("N"),
				};
				ents.Add(e);
			}

			var resp = await Repo.BatInsert(Ctx, AsyE(ents.ToArray()), CT.None);
			if(resp is null){
				throw new Exception("BatInsert returned null response");
			}

			_batUpdEnts.Clear();
			_batUpdIds.Clear();
			_batUpdEnts.AddRange(ents);
			_batUpdIds.AddRange(ents.Select(x=>x.Id));
			return NIL;
		});

		register.TesteeFnNames = [
			nameof(IRepo<PoKv, IdKv>.BatUpdById)
			,nameof(IRepo<PoKv, IdKv>.BatSlctById)
		];
		R("BatUpd_ById", async(o)=>{
			if(_batUpdIds.Count == 0){
				throw new Exception("BatUpd_Insert_Multi not executed or no ids recorded");
			}
			var Ctx = new DbFnCtx();
			var upds = new List<PoKv>();
			for(var i = 0; i < _batUpdEnts.Count; i++){
				var src = _batUpdEnts[i];
				upds.Add(new PoKv{
					Id = src.Id,
					Owner = src.Owner,
					KType = src.KType,
					VType = src.VType,
					KStr = "bat_upd_k2_" + System.Guid.NewGuid().ToString("N"),
					VStr = "bat_upd_v2_" + System.Guid.NewGuid().ToString("N"),
				});
			}
			var resp = await Repo.BatUpdById(Ctx, AsyE(upds.ToArray()), CT.None);
			if(resp is null){
				throw new Exception("BatUpdById returned null response");
			}

			var verify = await Repo.BatSlctById(Ctx, AsyE(_batUpdIds.ToArray()), CT.None);
			var list = new List<PoKv?>();
			await foreach(var item in verify) list.Add(item);
			if(list.Count != _batUpdIds.Count){
				throw new Exception($"Expected {_batUpdIds.Count} entries, got {list.Count}");
			}
			for(var i = 0; i < list.Count; i++){
				var got = list[i];
				if(got is null){
					throw new Exception($"Expected non-null entity at index {i}");
				}
				var exp = upds[i];
				if(!got.Id.Equals(exp.Id)){
					throw new Exception($"Id mismatch at index {i}");
				}
				if(got.KStr != exp.KStr || got.VStr != exp.VStr){
					throw new Exception($"Value mismatch at index {i}");
				}
			}
			_batUpdEnts.Clear();
			_batUpdEnts.AddRange(upds);
			return NIL;
		});

		register.TesteeFnNames = [
			nameof(IRepo<PoKv, IdKv>.BatUpdByCodeDict)
			,nameof(IRepo<PoKv, IdKv>.BatSlctById)
		];
		R("BatUpd_ByCodeDict", async(o)=>{
			if(_batUpdIds.Count == 0){
				throw new Exception("BatUpd_Insert_Multi not executed or no ids recorded");
			}
			var Ctx = new DbFnCtx();
			var dicts = new List<IDictionary<str, obj?>>();
			var expK = new List<str?>();
			var expV = new List<str?>();
			for(var i = 0; i < _batUpdIds.Count; i++){
				var k = "bat_upd_k3_" + System.Guid.NewGuid().ToString("N");
				var v = "bat_upd_v3_" + System.Guid.NewGuid().ToString("N");
				expK.Add(k);
				expV.Add(v);
				dicts.Add(new Dictionary<str, obj?>{
					[nameof(PoKv.KStr)] = k,
					[nameof(PoKv.VStr)] = v,
				});
			}
			var resp = await Repo.BatUpdByCodeDict(Ctx, AsyE(_batUpdIds.ToArray()), AsyE(dicts.ToArray()), CT.None);
			if(resp is null){
				throw new Exception("BatUpdByCodeDict returned null response");
			}
			var verify = await Repo.BatSlctById(Ctx, AsyE(_batUpdIds.ToArray()), CT.None);
			var list = new List<PoKv?>();
			await foreach(var item in verify) list.Add(item);
			for(var i = 0; i < list.Count; i++){
				var got = list[i];
				if(got is null){
					throw new Exception($"Expected non-null entity at index {i}");
				}
				if(got.KStr != expK[i] || got.VStr != expV[i]){
					throw new Exception($"Value mismatch at index {i}");
				}
			}
			return NIL;
		});

		register.TesteeFnNames = [
			nameof(IRepo<PoKv, IdKv>.BatUpdByDbDict)
			,nameof(IRepo<PoKv, IdKv>.BatSlctById)
		];
		R("BatUpd_ByDbDict", async(o)=>{
			if(_batUpdIds.Count == 0){
				throw new Exception("BatUpd_Insert_Multi not executed or no ids recorded");
			}
			var Ctx = new DbFnCtx();
			var dicts = new List<IDictionary<str, obj?>>();
			var expV = new List<str?>();
			for(var i = 0; i < _batUpdIds.Count; i++){
				var v = "bat_upd_v4_" + System.Guid.NewGuid().ToString("N");
				expV.Add(v);
				dicts.Add(new Dictionary<str, obj?>{
					[nameof(PoKv.VStr)] = v,
				});
			}
			var resp = await Repo.BatUpdByDbDict(Ctx, AsyE(_batUpdIds.ToArray()), AsyE(dicts.ToArray()), CT.None);
			if(resp is null){
				throw new Exception("BatUpdByDbDict returned null response");
			}
			var verify = await Repo.BatSlctById(Ctx, AsyE(_batUpdIds.ToArray()), CT.None);
			var list = new List<PoKv?>();
			await foreach(var item in verify) list.Add(item);
			for(var i = 0; i < list.Count; i++){
				var got = list[i];
				if(got is null){
					throw new Exception($"Expected non-null entity at index {i}");
				}
				if(got.VStr != expV[i]){
					throw new Exception($"Value mismatch at index {i}");
				}
			}
			return NIL;
		});

		register.TesteeFnNames = [
			nameof(IRepo<PoKv, IdKv>.BatHardDelById)
			,nameof(IRepo<PoKv, IdKv>.BatSlctById)
		];
		R("BatUpd_Cleanup_HardDelete", async(o)=>{
			if(_batUpdIds.Count == 0){
				return NIL;
			}
			var Ctx = new DbFnCtx();
			var resp = await Repo.BatHardDelById(Ctx, AsyE(_batUpdIds.ToArray()), CT.None);
			if(resp is null){
				throw new Exception("BatHardDelById returned null response");
			}
			var verify = await Repo.BatSlctById(Ctx, AsyE(_batUpdIds.ToArray()), CT.None);
			var list = new List<PoKv?>();
			await foreach(var item in verify) list.Add(item);
			if(list.Any(x=>x != null)){
				throw new Exception("Expected all nulls after hard delete");
			}
			return NIL;
		});
	}
}
