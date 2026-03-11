using Microsoft.Extensions.DependencyInjection;
using Ngaq.Local.Test.Domains.Word;
using Tsinswreng.CsTest;

namespace Ngaq.Local.Test;

public static class DiTestLocal{
	extension(IServiceCollection z){
		public IServiceCollection DiTests(){
			z.AddSingleton<TestISvcWord>();
			z.AddSingleton<TestDaoWord>();
			return z;
		}
	}
}



public class LocalTestMgr:I_RegisterTests{
	public IServiceProvider SvcP{get;set;} = null!;
	public IServiceCollection SvcC{get;set;} = null!;
	
	
	public ITestNode RegisterTests(ITestNode? T){
		T??=new TestNode();
		SvcP.GetRSvc<TestISvcWord>().RegisterTests(T.NewChild());
		SvcP.GetRSvc<TestDaoWord>().RegisterTests(T.NewChild());
		return T;
	}
}
