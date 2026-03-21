using Tsinswreng.CsTreeTest;
using Tsinswreng.CsSql;
using Ngaq.Core.Shared.Kv.Models;
using Ngaq.Core.Sys.Models;
using Ngaq.Core.Shared.User.Models.Po.User;
using Ngaq.Core.Shared.Base.Models.Po;
using Ngaq.Core.Infra.IF;
using Ngaq.Core.Shared.Word.Models.Po.Kv;

namespace Ngaq.Local.Test.CsSql.Repo;

public partial class TestRepo{
	readonly List<IdKv> _getAllIds = new();
	IdKv? _getAllSoftDeletedId = null;

	void RegisterGetAll(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestRepo)
			,[typeof(IRepo<PoKv, IdKv>)]
			,[]
		);
		var R = register.Register;

		register.TesteeFnNames = [nameof(IRepo<PoKv, IdKv>.BatAdd)];
		R("GetAll_Insert_Multi", async(o)=>{
			var Ctx = new DbFnCtx();
			var ents = new List<PoKv>();
			for(var i = 0; i < 3; i++){
				ents.Add(new PoKv{
					Id = new IdKv(),
					Owner = IdUser.Zero,
					KType = EKvType.Str,
					KStr = "get_all_k_" + System.Guid.NewGuid().ToString("N"),
					VType = EKvType.Str,
					VStr = "get_all_v_" + System.Guid.NewGuid().ToString("N"),
				});
			}

			var resp = await Repo.BatAdd(Ctx, AsyE(ents.ToArray()), CT.None);
			if(resp is null){
				throw new Exception("BatAdd returned null response");
			}

			_getAllIds.Clear();
			_getAllIds.AddRange(ents.Select(x=>x.Id));
			_getAllSoftDeletedId = _getAllIds[0];
			return NIL;
		});

		register.TesteeFnNames = [nameof(IRepo<PoKv, IdKv>.SoftDelInId)];
		R("GetAll_SoftDelete_One", async(o)=>{
			if(_getAllSoftDeletedId is null){
				throw new Exception("GetAll_Insert_Multi not executed");
			}
			var Ctx = new DbFnCtx();
			var resp = await Repo.SoftDelInId(Ctx, AsyE(_getAllSoftDeletedId.Value), CT.None);
			if(resp is null){
				throw new Exception("SoftDelInId returned null response");
			}
			return NIL;
		});

		register.TesteeFnNames = [nameof(IRepo<PoKv, IdKv>.GetAll)];
		R("GetAll_Should_Exclude_SoftDeleted", async(o)=>{
			if(_getAllIds.Count == 0 || _getAllSoftDeletedId is null){
				throw new Exception("GetAll_Insert_Multi not executed");
			}
			var Ctx = new DbFnCtx();
			var gotAsy = await Repo.GetAll(Ctx, CT.None);
			var got = new List<PoKv>();
			await foreach(var item in gotAsy){
				got.Add(item);
			}

			var gotInserted = got.Where(x=>_getAllIds.Contains(x.Id)).ToList();
			if(gotInserted.Count != _getAllIds.Count - 1){
				throw new Exception($"Expected {_getAllIds.Count - 1} non-deleted inserted rows, got {gotInserted.Count}");
			}
			if(gotInserted.Any(x=>x.Id.Equals(_getAllSoftDeletedId.Value))){
				throw new Exception("GetAll returned a soft-deleted row");
			}
			if(gotInserted.Any(x=>x.IsDeleted())){
				throw new Exception("GetAll returned deleted row in inserted subset");
			}
			return NIL;
		});

		register.TesteeFnNames = [nameof(IRepo<PoKv, IdKv>.BatHardDelById)];
		R("GetAll_Cleanup_HardDelete", async(o)=>{
			if(_getAllIds.Count == 0){
				return NIL;
			}
			var Ctx = new DbFnCtx();
			await Repo.BatHardDelById(Ctx, AsyE(_getAllIds.ToArray()), CT.None);
			return NIL;
		});
	}
}
