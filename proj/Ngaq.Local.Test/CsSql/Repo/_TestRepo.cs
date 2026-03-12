using Tsinswreng.CsTest;
using Tsinswreng.CsSql;
using Ngaq.Core.Shared.Kv.Models;
using Ngaq.Core.Sys.Models;
namespace Ngaq.Local.Test.CsSql.Repo;

public class TestRepo : ITester{
	IRepo<PoKv, IdKv> Repo;
	public TestRepo(IRepo<PoKv, IdKv> Repo){
		this.Repo = Repo;
	}
	public ITestNode RegisterTestsInto(ITestNode? Test){
		Test??=new TestNode();
		
		return Test;
	}
}
