using Microsoft.Extensions.DependencyInjection;
using Ngaq.Core;
using Ngaq.Local;
using Ngaq.Local.Di;
using Ngaq.Local.Test;
using Tsinswreng.CsTest;

namespace Ngaq.Windows.Test;

internal class Program{
	public static IServiceCollection SvcColct = new ServiceCollection();
	public static IServiceProvider SvcProvdr = null!;
	public static async Task Main(string[] args){
		SvcColct
			.SetupCore()
			.SetupLocal()
			.SetupLocalFrontend();
		AppIniter.Inst.Sp = SvcProvdr;
		_ = AppIniter.Inst.Init(default).Result;

		var mgr = WindowsTestMgr.Inst;
		SvcProvdr = mgr.InitSvc(SvcColct);
		ITestExecutor executor = new TreeTestExecutor();
		await executor.RunEtPrint(mgr.TestNode);
		
	}
}

