#if false
測試方法:
+ 直接在Ngaq.Windows.Test中執行 dotnet run (非AOT編譯)
+ 執行以下腳本 在AOT還境下測試
```bash
dotnet publish -c Release -r win-x64
./bin/Release/net10.0/win-x64/publish/Ngaq.Windows.Test.exe
```
#endif

using Microsoft.Extensions.DependencyInjection;
using Ngaq.Core;
using Ngaq.Backend;
using Ngaq.Backend.Di;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Windows.Test;

internal class Program{
	public static IServiceCollection SvcColct = new ServiceCollection();
	public static IServiceProvider SvcProvdr = null!;
	public static async Task Main(string[] args){
		SvcColct
			.SetupCore()
			.SetupLocal()
			.SetupLocalFrontend()
		;

		var mgr = WindowsTestMgr.Inst;
		SvcProvdr = mgr.InitSvc(SvcColct, sc => sc.BuildServiceProvider());

		AppIniter.Inst.Sp = SvcProvdr;
		_ = AppIniter.Inst.Init(default).Result;
		ITestExecutor executor = new TreeTestExecutor();
		await executor.RunEtPrint(mgr.TestNode);
	}
}
