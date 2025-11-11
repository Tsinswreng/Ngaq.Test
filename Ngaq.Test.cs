global using static Program;
using Microsoft.Extensions.DependencyInjection;
using Ngaq.Core;
using Ngaq.Core.Infra;
using Ngaq.Core.Infra.Errors;
using Ngaq.Core.Shared.Kv.Models;
using Ngaq.Core.Shared.User.Models.Po.User;
using Ngaq.Core.Shared.Word.Models;
using Ngaq.Core.Shared.Word.Models.Dto;
using Ngaq.Core.Tools;
using Ngaq.Local.Di;
using Ngaq.Test;
using Ngaq.Test.Word;
using Tsinswreng.CsTools;
//dotnet publish -c Release -r win-x64
// ./bin/Release/net9.0/win-x64/publish/Ngaq.Test.exe
#region Main

new TestLua().Test();


throw new Exception("AOT");
#endregion Main


internal partial class Program{
	public static str GetFullTypeName<T>(){
		return typeof(T).FullName!;
	}
	// static async Task Main(string[] args){

	// }
	static Program(){
		Di();
	}
	public static ServiceProvider SvcProvider = null!;
	public static nil Di(){
		var svc = new ServiceCollection();
		svc
			.SetupCore()
			.SetupLocal()//TODO 改成按需API調用
		;
		SvcProvider = svc.BuildServiceProvider();
		return NIL;
	}
	public static T? GetSvc<T>(){
		return SvcProvider.GetService<T>();
	}

}
