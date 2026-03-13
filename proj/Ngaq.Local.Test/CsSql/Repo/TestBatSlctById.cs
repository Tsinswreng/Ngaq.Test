using Tsinswreng.CsTest;
using Tsinswreng.CsSql;
using Ngaq.Core.Shared.Kv.Models;
using Ngaq.Core.Sys.Models;
namespace Ngaq.Local.Test.CsSql.Repo;

public partial class TestRepo{
	void RegisterBatSlctById(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestRepo)
			,[typeof(IRepo<PoKv, IdKv>)]
			,[nameof(IRepo<PoKv, IdKv>.BatSlctById)]
		);
		var R = register.Register;

		register.TesteeFnNames = [nameof(IRepo<PoKv, IdKv>.BatSlctById)];
		R("BatSlctById_EmptyIds_ReturnsEmpty", async(o)=>{
			var Ctx = new DbFnCtx();
			var Result = await Repo.BatSlctById(Ctx, AsyE<IdKv>(), CT.None);
			var List = new List<PoKv?>();
			await foreach(var Item in Result) List.Add(Item);
			if(List.Count != 0){
				throw new Exception($"Expected empty, got {List.Count}");
			}
			return NIL;
		});

		register.TesteeFnNames = [nameof(IRepo<PoKv, IdKv>.BatSlctById)];
		R("BatSlctById_NonExistIds_ReturnsNulls", async(o)=>{
			var Ctx = new DbFnCtx();
			var Id1 = new IdKv();
			var Id2 = new IdKv();
			var Result = await Repo.BatSlctById(Ctx, AsyE(Id1, Id2), CT.None);
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
	}
}
