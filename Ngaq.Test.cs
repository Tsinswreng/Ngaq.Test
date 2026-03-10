//先dotent build編譯、後在vscode 調試界面 選TestWin㕥行、如是則既有斷點又可連數據庫
//直ᵈ dotnet run 則cwd不對、連不到數據庫
global using static Program;
using System.Diagnostics;
using System.Net.WebSockets;
using Microsoft.Extensions.DependencyInjection;
using Ngaq.Core;
using Ngaq.Core.Frontend.User;
using Ngaq.Core.Infra;
using Ngaq.Core.Infra.Errors;
using Ngaq.Core.Shared.Base.Models.Po;
using Ngaq.Core.Shared.Kv.Models;
using Ngaq.Core.Shared.User.Models.Po;
using Ngaq.Core.Shared.User.Models.Po.User;
using Ngaq.Core.Shared.User.UserCtx;
using Ngaq.Core.Shared.Word.Models;
using Ngaq.Core.Shared.Word.Models.Dto;
using Ngaq.Core.Shared.Word.Models.Po.Word;
using Ngaq.Core.Tools;
using Ngaq.Core.Word.Svc;
using Ngaq.Local;
using Ngaq.Local.Db.TswG;
using Ngaq.Local.Di;
using Ngaq.Local.Domains.Word.Dao;
using Ngaq.Local.Word.Dao;
using Ngaq.Test;
using Ngaq.Test.CsSqlHelper.Integration.Repo;
using Ngaq.Test.CsLang;
using Ngaq.Test.Tools;
using Ngaq.Test.Try;
using Ngaq.Test.Word;
using Tsinswreng.CsPage;
using Tsinswreng.CsSqlHelper;
using Tsinswreng.CsTools;
//dotnet publish -c Release -r win-x64
// ./bin/Release/net10.0/win-x64/publish/Ngaq.Test.exe
using Jn = System.Text.Json.Nodes.JsonNode;
#pragma warning disable CS8321
#region Main

//new TestToolYaml().Run();
Program.Init();
await Program.GetRSvc<TestRepo>().Run(default);

#endregion Main


internal partial class Program{
	public static str GetFullTypeName<T>(){
		return typeof(T).FullName!;
	}
	// static async Task Main(string[] args){

	// }
	public static void Init(){
		Di();
		InitApp();
	}
	
	public static ServiceProvider SvcProvider = null!;
	public static nil Di(){
		var svc = new ServiceCollection();
		svc
			.SetupCore()
			.SetupLocal()//TODO 改成按需API調用
			.SetupLocalFrontend()
			.SetupTest()
		;
		SvcProvider = svc.BuildServiceProvider();
		return NIL;
	}
	public static nil InitApp(){
		AppIniter.Inst.Sp = SvcProvider;
		_ = AppIniter.Inst.Init(default).Result;
		return NIL;
	}
	public static T GetRSvc<T>()
		where T : class
	{
		return SvcProvider.GetRequiredService<T>();
	}

}


