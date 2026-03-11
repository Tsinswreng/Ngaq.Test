using Microsoft.Extensions.DependencyInjection;
using Ngaq.Local.Test;
using Tsinswreng.CsTest;

namespace Ngaq.Windows.Test;

internal class Program{
	public static IServiceCollection SvcColct = new ServiceCollection();
	public static IServiceProvider SvcProvdr = null!;
	public static async Task Main(string[] args){
		var mgr = WindowsTestMgr.Inst;
		SvcProvdr = mgr.InitSvc(SvcColct);
		await mgr.TestNode.RunTests();
	}
}


