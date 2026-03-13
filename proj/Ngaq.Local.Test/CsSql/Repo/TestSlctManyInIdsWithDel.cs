using Tsinswreng.CsTest;
using Tsinswreng.CsSql;
using Ngaq.Core.Shared.Kv.Models;
using Ngaq.Core.Sys.Models;
namespace Ngaq.Local.Test.CsSql.Repo;

public partial class TestRepo{
	void RegisterSlctManyInIdsWithDel(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestRepo), [typeof(IRepo<PoKv, IdKv>)], [nameof(Repo.SlctManyInIdsWithDel)]
		);
		var R = register.Register;
		R("SlctManyInIdsWithDel_EmptyIds_ReturnsEmpty", async(o)=>{
			var Ctx = new DbFnCtx();
			var Result = await Repo.SlctManyInIdsWithDel(Ctx, AsyE<IdKv>(), CT.None);
			var List = new List<PoKv?>();
			await foreach(var Item in Result) List.Add(Item);
			if(List.Count != 0){
				throw new Exception($"Expected empty, got {List.Count}");
			}
			return NIL;
		});

		R("SlctManyInIdsWithDel_NonExistIds_ReturnsEmpty", async(o)=>{
			var Ctx = new DbFnCtx();
			var Result = await Repo.SlctManyInIdsWithDel(Ctx, AsyE(new IdKv(), new IdKv()), CT.None);
			var List = new List<PoKv?>();
			await foreach(var Item in Result) List.Add(Item);
			if(List.Count != 0){
				throw new Exception($"Expected empty for non-existent ids, got {List.Count}");
			}
			return NIL;
		});
	}
	void RegisterSlctManyInIdsWithDel(ITestFnRegister Register){
		var R = Register.Register;

		
	}
}
