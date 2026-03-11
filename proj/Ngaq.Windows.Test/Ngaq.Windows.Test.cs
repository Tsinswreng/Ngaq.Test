using Microsoft.Extensions.DependencyInjection;
using Ngaq.Local.Test;
using Tsinswreng.CsTest;

namespace Ngaq.Windows.Test;

internal class Program{
	public static IServiceCollection SvcColct = new ServiceCollection();
	public static IServiceProvider SvcProvdr = SvcColct.BuildServiceProvider();
	public static async Task Main(string[] args){
		var mgr = WindowsTestMgr.Inst;
		mgr.InitSvc(SvcColct, SvcProvdr);
		await mgr.TestNode.RunTests();
	}
}


