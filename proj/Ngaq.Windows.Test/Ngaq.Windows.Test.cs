#if false
dotnet publish -c Release -r win-x64
./bin/Release/net10.0/win-x64/publish/Ngaq.Windows.Test.exe
#endif

using System.Text;
using System.Text.Unicode;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Schema;
using Ngaq.Core;
using Ngaq.Core.Frontend.User;
using Ngaq.Core.Infra;
using Ngaq.Core.Shared.StudyPlan.Models.Po.WeightArg;
using Ngaq.Core.Shared.StudyPlan.Svc;
using Ngaq.Core.Shared.Word.Models;
using Ngaq.Core.Tools;
using Ngaq.Core.Tools.Json;
using Ngaq.Local;
using Ngaq.Local.Di;
using Ngaq.Local.Test;
using Tsinswreng.CsTempus;
using Tsinswreng.CsTools;
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
		// var studyPlan = SvcProvdr.GetRequiredService<ISvcStudyPlan>();
		// var userCtxMgr = SvcProvdr.GetRequiredService<IFrontendUserCtxMgr>();
		// await studyPlan.RestoreBuiltinStudyPlan(userCtxMgr.GetDbUserCtx(), default);
	}
}
