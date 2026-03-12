using Tsinswreng.CsTest;
using Tsinswreng.CsSql;
using Ngaq.Core.Shared.Kv.Models;
using Ngaq.Core.Sys.Models;
namespace Ngaq.Local.Test.CsSql.Repo;

public partial class TestRepo : ITester{
	IRepo<PoKv, IdKv> Repo;
	public TestRepo(IRepo<PoKv, IdKv> Repo){
		this.Repo = Repo;
	}
	public ITestNode RegisterTestsInto(ITestNode? Test){
		Test??=new TestNode();
		Test.Ordered = true;
		var Register = Test.MkTestFnRegister(typeof(TestRepo), typeof(IRepo<PoKv, IdKv>));
		RegisterSlctManyInIdsWithDel(Register);
		RegisterBatSlctById(Register);
		RegisterBatInsert(Register);
		RegisterBatUpd(Register);
		return Test;
	}

	static async IAsyncEnumerable<T> AsyE<T>(params T[] Items){
		foreach(var I in Items) yield return I;
	}
}
