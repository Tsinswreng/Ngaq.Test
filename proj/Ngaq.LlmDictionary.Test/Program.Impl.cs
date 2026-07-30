using Microsoft.Extensions.DependencyInjection;
using Ngaq.Backend;
using Ngaq.Backend.Di;
using Ngaq.Backend.Test.Domains.Dictionary;
using Ngaq.Core;
using Ngaq.Core.Infra.Cfg;
using Ngaq.Core.Infra.Url;
using Ngaq.Core.Shared.Dictionary.Svc;
using Tsinswreng.CsCfg;
using Tsinswreng.CsTreeTest;

namespace Ngaq.LlmDictionary.Test;

internal static partial class Program{
	/// 固定載入私有設定後，執行一次真實模型詞典評估。
	private static async partial Task Main(){
		var RepositoryRoot = FindRepositoryRoot(AppContext.BaseDirectory);
		ConfigurePrivateAppCfg(RepositoryRoot);
		await RunDictionaryEvaluation();
	}

	/// 從建置輸出目錄向上搜尋解決方案與私有設定目錄。
	private static partial str FindRepositoryRoot(str StartDir){
		var Current = new DirectoryInfo(StartDir);
		while(Current is not null){
			var SolutionPath = Path.Combine(Current.FullName, "Ngaq.sln");
			var PrivateConfigDir = Path.Combine(Current.FullName, PrivateConfigDirName);
			if(File.Exists(SolutionPath) && Directory.Exists(PrivateConfigDir)){
				return Current.FullName;
			}
			Current = Current.Parent;
		}
		throw new DirectoryNotFoundException(
			$"Unable to find repository root containing {PrivateConfigDirName}."
		);
	}

	/// 載入私有雙源設定，並使本地相對路徑以私有設定目錄為基準。
	private static partial void ConfigurePrivateAppCfg(str RepositoryRoot){
		var ConfigDir = Path.Combine(RepositoryRoot, PrivateConfigDirName);
		var ReadOnlyConfigPath = Path.Combine(ConfigDir, ReadOnlyConfigFileName);
		var DualCfg = AppCfg.Inst;
		DualCfg.RoCfg = new JsonFileCfgAccessor().FromFile(ReadOnlyConfigPath);

		var RwCfgPath = KeysClientCfg.RwCfgPath.GetFrom(DualCfg) ?? "";
		if(!Path.IsPathRooted(RwCfgPath)){
			RwCfgPath = Path.Combine(ConfigDir, RwCfgPath);
		}
		DualCfg.RwCfg = new JsonFileCfgAccessor().FromFile(RwCfgPath);
		BaseDirMgr.Inst._BaseDir = ConfigDir;
	}

	/// 專用進程只組裝詞典服務，避免建立通用測試樹或任何 UI 依賴。
	private static async partial Task RunDictionaryEvaluation(){
		var SvcColct = new ServiceCollection();
		SvcColct
			.SetupCore()
			.SetupLocal()
			.SetupLocalFrontend()
		;
		using var SvcProvdr = SvcColct.BuildServiceProvider();
		using var Scope = SvcProvdr.CreateScope();
		var SvcDictionary = Scope.ServiceProvider.GetRequiredService<ISvcDictionary>();
		var Node = TestISvcDictionary.MkNode(SvcDictionary);
		await new TreeTestExecutor().RunEtPrint(Node, ThrowOnFailed: true);
	}
}
