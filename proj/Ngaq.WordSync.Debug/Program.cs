using System.Collections.Generic;
using System.IO.Abstractions;
using System.Text;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Ngaq.Backend;
using Ngaq.Backend.Db.TswG;
using Ngaq.Backend.Di;
using Ngaq.Backend.Domains.Word.Dao;
using Ngaq.Core;
using Ngaq.Core.Frontend.User;
using Ngaq.Core.Infra;
using Ngaq.Core.Infra.Cfg;
using Ngaq.Core.Infra.IF;
using Ngaq.Core.Model.Po.Kv;
using Ngaq.Core.Infra.Url;
using Ngaq.Core.Shared.Sync;
using Ngaq.Core.Shared.Word.Models;
using Ngaq.Core.Shared.Word.Models.Dto;
using Ngaq.Core.Shared.Word.Models.Po.Kv;
using Ngaq.Core.Shared.Word.Models.Po.Word;
using Ngaq.Core.Shared.Word.Svc;
using Tsinswreng.CsCfg;
using Tsinswreng.CsSql;
using Tsinswreng.CsTools;

namespace Ngaq.WordSync.Debug;

/// <summary>
/// 專用於重現「單詞同步導入 .ngaq 後觸發 WordProp.Id 唯一約束」的最小調試入口。
/// 這個程序不碰 tmp 原始樣本，而是每次把樣本複製到 exe 旁的工作目錄後再運行。
/// </summary>
internal sealed class Program{
	/// <summary>
	/// raw: 原樣重現；
	/// normalize-zero-prop-id: 在複製出的 DB 與遠端包內存副本中，先把全 0 WordProp.Id 換成新 Id，再跑一次同步。
	/// </summary>
	private const string RawScenario = "raw";
	private const string NormalizeZeroPropIdScenario = "normalize-zero-prop-id";

	private const string AndroidDbFileName = "Ngaq.sqlite_FromAndroid";
	private const string PackageFileName = "2026_0703_005840.ngaq";
	private const string ConfigFileName = "Ngaq.jsonc";
	private const string RwConfigFileName = "Ngaq.Rw.jsonc";
	private const string RuntimeDbFileName = "Ngaq.sqlite";

	[STAThread]
	private static async Task Main(string[] Args){
		var Scenario = ParseScenario(Args);
		var ExeDir = AppContext.BaseDirectory;
		var RepoRoot = FindRepoRoot(ExeDir);
		var ScenarioDir = PrepareScenarioWorkspace(RepoRoot, ExeDir, Scenario);

		Console.WriteLine($"[boot] repoRoot={RepoRoot}");
		Console.WriteLine($"[boot] exeDir={ExeDir}");
		Console.WriteLine($"[boot] scenario={Scenario}");
		Console.WriteLine($"[boot] scenarioDir={ScenarioDir}");

		ConfigureAppCfg(ScenarioDir);
		BaseDirMgr.Inst._BaseDir = ScenarioDir;

		var ServiceCollection = new ServiceCollection();
		ServiceCollection
			.SetupCore()
			.SetupLocal()
			.SetupLocalFrontend();

		var ServiceProvider = ServiceCollection.BuildServiceProvider();
		AppIniter.Inst.Sp = ServiceProvider;
		await AppIniter.Inst.Init(default);

		if(Scenario == NormalizeZeroPropIdScenario){
			await NormalizeZeroIdWordPropsInDb(ServiceProvider, default);
		}

		await DiagnoseAndRunSync(ServiceProvider, ScenarioDir, Scenario, default);
	}

	/// <summary>
	/// 場景只允許固定值，避免調試過程中跑錯目錄或誤寫到未知位置。
	/// </summary>
	private static string ParseScenario(string[] Args){
		if(Args.Length == 0){
			return RawScenario;
		}
		var Scenario = Args[0].Trim();
		if(Scenario == RawScenario || Scenario == NormalizeZeroPropIdScenario){
			return Scenario;
		}
		throw new ArgumentException(
			$"Unsupported scenario: {Scenario}. " +
			$"Use `{RawScenario}` or `{NormalizeZeroPropIdScenario}`."
		);
	}

	/// <summary>
	/// 從輸出目錄一路向上找倉庫根，避免把 tmp 樣本路徑硬編碼死在程序裡。
	/// </summary>
	private static string FindRepoRoot(string StartDir){
		var Current = new DirectoryInfo(StartDir);
		while(Current is not null){
			var TmpDir = Path.Combine(Current.FullName, "tmp");
			var ExternalRsrcDir = Path.Combine(Current.FullName, "ExternalRsrc");
			if(
				Directory.Exists(TmpDir)
				&& File.Exists(Path.Combine(TmpDir, AndroidDbFileName))
				&& Directory.Exists(ExternalRsrcDir)
			){
				return Current.FullName;
			}
			Current = Current.Parent;
		}
		throw new DirectoryNotFoundException("Failed to locate repo root from executable directory.");
	}

	/// <summary>
	/// 每次都基於原始樣本重新複製，保證 raw / normalize 場景之間互不污染。
	/// 同時把只供本次 debug 使用的配置文件寫進場景目錄，明確由配置決定 DB 路徑。
	/// </summary>
	private static string PrepareScenarioWorkspace(string RepoRoot, string ExeDir, string Scenario){
		var WorkRoot = Path.Combine(ExeDir, "WordSyncDebugWork");
		var ScenarioDir = Path.Combine(WorkRoot, Scenario);
		if(Directory.Exists(ScenarioDir)){
			Directory.Delete(ScenarioDir, true);
		}
		Directory.CreateDirectory(ScenarioDir);

		var SourceDbPath = Path.Combine(RepoRoot, "tmp", AndroidDbFileName);
		var SourcePackagePath = Path.Combine(RepoRoot, "tmp", PackageFileName);
		var RuntimeDbPath = Path.Combine(ScenarioDir, RuntimeDbFileName);
		var RuntimePackagePath = Path.Combine(ScenarioDir, PackageFileName);

		File.Copy(SourceDbPath, RuntimeDbPath, true);
		File.Copy(SourcePackagePath, RuntimePackagePath, true);

		WriteScenarioConfig(ScenarioDir);
		return ScenarioDir;
	}

	/// <summary>
	/// 這裏故意把配置文件生成到當前場景目錄，方便直接檢查本次進程到底連的是哪個 sqlite。
	/// </summary>
	private static void WriteScenarioConfig(string ScenarioDir){
		var ConfigPath = Path.Combine(ScenarioDir, ConfigFileName);
		var RwConfigPath = Path.Combine(ScenarioDir, RwConfigFileName);

		var RoCfg =
"""
{
	"Version": "1.0.0",
	"RwCfgPath": "Ngaq.Rw.jsonc",
	"SqlitePath": "./Ngaq.sqlite",
	"Word": {
		"MaxDisplayedWordCount": 999999
	}
}
""";
		File.WriteAllText(ConfigPath, RoCfg, Encoding.UTF8);
		ToolFile.EnsureFile(RwConfigPath);
	}

	/// <summary>
	/// 直接模擬 Windows 入口的雙源配置裝配，但不引入 UI 啓動鏈路。
	/// </summary>
	private static void ConfigureAppCfg(string ScenarioDir){
		var DualSrcCfg = AppCfg.Inst;
		var RoCfg = new JsonFileCfgAccessor();
		DualSrcCfg.RoCfg = RoCfg;
		RoCfg.FromFile(Path.Combine(ScenarioDir, ConfigFileName));

		var GuiCfg = new JsonFileCfgAccessor();
		DualSrcCfg.RwCfg = GuiCfg;
		GuiCfg.FromFile(Path.Combine(ScenarioDir, RwConfigFileName));
	}

	/// <summary>
	/// 先做不落庫的對照診斷，再真正調同步接口。
	/// raw 場景用 FromStream 直接重現原路徑；
	/// normalize 場景則先在內存把遠端全 0 PropId 改掉，再走 BatSyncJnWordByBizId。
	/// </summary>
	private static async Task DiagnoseAndRunSync(
		IServiceProvider ServiceProvider,
		string ScenarioDir,
		string Scenario,
		CancellationToken Ct
	){
		using var Scope = ServiceProvider.CreateScope();
		var Sp = Scope.ServiceProvider;

		var UserCtxMgr = Sp.GetRequiredService<IFrontendUserCtxMgr>();
		var WordSvc = Sp.GetRequiredService<ISvcWordV2>();
		var WordInMemSvc = Sp.GetRequiredService<ISvcWordInMem>();
		var DaoWordV2 = Sp.GetRequiredService<DaoWordV2>();
		var PackagePath = Path.Combine(ScenarioDir, PackageFileName);
		var DbPath = Path.Combine(ScenarioDir, RuntimeDbFileName);

		Console.WriteLine($"[ctx] userId={UserCtxMgr.GetUserCtx().UserId}");
		Console.WriteLine($"[ctx] dbPath={DbPath}");
		Console.WriteLine($"[ctx] packagePath={PackagePath}");

		var LocalWords = await ToListAsync(
			WordSvc.GetAllWordsWithDel(UserCtxMgr.GetDbUserCtx(), Ct),
			Ct
		);
		Console.WriteLine($"[local] wordCount={LocalWords.Count}");

		await LogLocalZeroIdWordProps(LocalWords, "before-remote-diagnose", Ct);

		var RemoteWords = await LoadRemoteWords(WordSvc, PackagePath, Ct);
		PrepareRemoteWords(RemoteWords, UserCtxMgr);
		Console.WriteLine($"[remote] wordCount={RemoteWords.Count}");

		if(Scenario == NormalizeZeroPropIdScenario){
			NormalizeZeroIdWordPropsInMemory(RemoteWords, "remote-memory");
		}else{
			LogZeroIdWordPropsInMemory(RemoteWords, "remote-memory");
		}

		LogRemoteDuplicatePropIds(RemoteWords);

		await LogClassificationPreview(
			UserCtxMgr,
			WordInMemSvc,
			DaoWordV2,
			LocalWords,
			RemoteWords,
			Ct
		);

		try{
			Console.WriteLine($"[apply] scenario={Scenario} begin");
			if(Scenario == RawScenario){
				await ApplyRawStreamSync(WordSvc, UserCtxMgr, PackagePath, Ct);
			}else{
				await ApplyNormalizedSync(WordSvc, UserCtxMgr, RemoteWords, Ct);
			}
			Console.WriteLine($"[apply] scenario={Scenario} completed");
		}catch(Exception Ex){
			Console.WriteLine($"[apply] scenario={Scenario} failed: {Ex.GetType().FullName}");
			Console.WriteLine($"[apply] message={Ex.Message}");
			Console.WriteLine(Ex);
			await LogLocalWordPropIdCollisionsAfterFailure(ServiceProvider, UserCtxMgr, RemoteWords, Ct);
			throw;
		}
	}

	/// <summary>
	/// 原樣重現 UI 導入路徑，確保第一次報錯不是被調試代碼“改出來”的。
	/// </summary>
	private static async Task ApplyRawStreamSync(
		ISvcWordV2 WordSvc,
		IFrontendUserCtxMgr UserCtxMgr,
		string PackagePath,
		CancellationToken Ct
	){
		using var PackageStream = File.OpenRead(PackagePath);
		await foreach(var Dto in WordSvc.BatSyncJnWordByBizIdFromStream(
			UserCtxMgr.GetDbUserCtx(),
			PackageStream,
			Ct
		).WithCancellation(Ct)){
			LogSyncDto("[apply-dto][raw]", Dto);
		}
	}

	/// <summary>
	/// normalize 場景不再經過 stream 入口，避免還沒來得及改全 0 Id 就直接落入唯一約束。
	/// 這個場景只用於驗證“修正 0 Id 之後故障是否消失”。
	/// </summary>
	private static async Task ApplyNormalizedSync(
		ISvcWordV2 WordSvc,
		IFrontendUserCtxMgr UserCtxMgr,
		IList<JnWord> RemoteWords,
		CancellationToken Ct
	){
		await foreach(var Dto in WordSvc.BatSyncJnWordByBizId(
			UserCtxMgr.GetDbUserCtx(),
			ToAsyncEnumerable(RemoteWords),
			Ct
		).WithCancellation(Ct)){
			LogSyncDto("[apply-dto][normalized]", Dto);
		}
	}

	/// <summary>
	/// 在真正同步前，先用與服務一致的 Owner / ForeignKey 規整邏輯對遠端包做標準化。
	/// 這樣 preview 的分類結果才和實際服務內部一致。
	/// </summary>
	private static void PrepareRemoteWords(IList<JnWord> RemoteWords, IFrontendUserCtxMgr UserCtxMgr){
		var UserId = UserCtxMgr.GetUserCtx().UserId;
		foreach(var RemoteWord in RemoteWords){
			RemoteWord.Owner = UserId;
			RemoteWord.EnsureForeignId();
		}
	}

	/// <summary>
	/// 使用服務本身的解包器，避免調試入口和正式代碼在 .ngaq 解包邏輯上出現偏差。
	/// </summary>
	private static async Task<List<JnWord>> LoadRemoteWords(
		ISvcWordV2 WordSvc,
		string PackagePath,
		CancellationToken Ct
	){
		using var PackageStream = File.OpenRead(PackagePath);
		return await ToListAsync(WordSvc.UnpackJnWords(PackageStream, Ct), Ct);
	}

	/// <summary>
	/// 預覽每個遠端詞在同步前會被分到哪個分支，並打印和本地 PropId 的交集。
	/// 如果某個詞被判成 LocalNotExist，但它的 PropId 已經在本地出現，這就是直接可疑點。
	/// </summary>
	private static async Task LogClassificationPreview(
		IFrontendUserCtxMgr UserCtxMgr,
		ISvcWordInMem WordInMemSvc,
		DaoWordV2 DaoWordV2,
		IList<JnWord> LocalWords,
		IList<JnWord> RemoteWords,
		CancellationToken Ct
	){
		var LocalByHeadLang = new Dictionary<string, JnWord>();
		var LocalPropOwners = BuildPropOwnerIndex(LocalWords);
		foreach(var LocalWord in LocalWords){
			LocalByHeadLang[ToHeadLangKey(LocalWord.Head, LocalWord.Lang)] = LocalWord;
		}

		Console.WriteLine("[preview] begin");
		foreach(var RemoteWord in RemoteWords){
			var Key = ToHeadLangKey(RemoteWord.Head, RemoteWord.Lang);
			LocalByHeadLang.TryGetValue(Key, out var LocalWord);
			var SyncResult = WordInMemSvc.SyncJnWord(LocalWord, RemoteWord);
			LogSyncDto("[preview-dto]", SyncResult);

			var Intersections = FindIntersectedPropIds(LocalPropOwners, RemoteWord);
			foreach(var Intersection in Intersections){
				Console.WriteLine(
					"[preview-prop-intersection] " +
					$"remoteHead={RemoteWord.Head} " +
					$"remoteLang={RemoteWord.Lang} " +
					$"remoteWordId={RemoteWord.Id} " +
					$"propId={Intersection.PropId} " +
					$"sameWord={Intersection.IsSameWord} " +
					$"localHead={Intersection.LocalHead} " +
					$"localLang={Intersection.LocalLang} " +
					$"localWordId={Intersection.LocalWordId}"
				);
			}
		}

		// 額外校驗一次服務查本地詞的官方路徑，避免我們自建索引和實際 DAO 路徑有偏差。
		await using var DbFnCtx = new DbFnCtx();
		var HeadLangs = new List<Head_Lang>();
		foreach(var RemoteWord in RemoteWords){
			HeadLangs.Add(new Head_Lang(RemoteWord.Head, RemoteWord.Lang));
		}
		var MatchedRoots = await ToListAsync(
			DaoWordV2.BatGetPoWordByOwnerHeadLangWithDel(
				DbFnCtx,
				UserCtxMgr.GetUserCtx().UserId,
				ToAsyncEnumerable(HeadLangs),
				Ct
			),
			Ct
		);
		for(var Index = 0; Index < RemoteWords.Count; Index++){
			var RemoteWord = RemoteWords[Index];
			var LocalRoot = MatchedRoots[Index];
			Console.WriteLine(
				"[preview-dao-match] " +
				$"remoteHead={RemoteWord.Head} " +
				$"remoteLang={RemoteWord.Lang} " +
				$"daoMatchedWordId={(LocalRoot is null ? "<null>" : LocalRoot.Id.ToString())}"
			);
		}
		Console.WriteLine("[preview] end");
	}

	/// <summary>
	/// 如果同步最終拋唯一約束，就把“本地已存在的 PropId”再完整打印一遍，
	/// 方便對照到底是 local/local、remote/local，還是 0 Id 導致的碰撞。
	/// </summary>
	private static async Task LogLocalWordPropIdCollisionsAfterFailure(
		IServiceProvider RootServiceProvider,
		IFrontendUserCtxMgr UserCtxMgr,
		IList<JnWord> RemoteWords,
		CancellationToken Ct
	){
		using var Scope = RootServiceProvider.CreateScope();
		var Sp = Scope.ServiceProvider;
		var WordSvc = Sp.GetRequiredService<ISvcWordV2>();
		var LocalWords = await ToListAsync(
			WordSvc.GetAllWordsWithDel(UserCtxMgr.GetDbUserCtx(), Ct),
			Ct
		);
		var LocalPropOwners = BuildPropOwnerIndex(LocalWords);

		Console.WriteLine("[failure-analysis] begin");
		foreach(var RemoteWord in RemoteWords){
			var Intersections = FindIntersectedPropIds(LocalPropOwners, RemoteWord);
			foreach(var Intersection in Intersections){
				Console.WriteLine(
					"[failure-analysis-prop] " +
					$"remoteHead={RemoteWord.Head} " +
					$"remoteLang={RemoteWord.Lang} " +
					$"remoteWordId={RemoteWord.Id} " +
					$"propId={Intersection.PropId} " +
					$"sameWord={Intersection.IsSameWord} " +
					$"localHead={Intersection.LocalHead} " +
					$"localLang={Intersection.LocalLang} " +
					$"localWordId={Intersection.LocalWordId}"
				);
			}
		}
		Console.WriteLine("[failure-analysis] end");
	}

	/// <summary>
	/// 只打印本地 zero Id，不做修正。
	/// 這個日志能快速告訴我們 Android DB 本身是否已帶着壞數據進入同步。
	/// </summary>
	private static Task LogLocalZeroIdWordProps(
		IList<JnWord> LocalWords,
		string Label,
		CancellationToken Ct
	){
		_ = Ct;
		var ZeroId = default(IdWordProp);
		foreach(var LocalWord in LocalWords){
			foreach(var Prop in LocalWord.Props){
				if(Prop.Id != ZeroId){
					continue;
				}
				Console.WriteLine(
					"[local-zero-prop-id] " +
					$"label={Label} " +
					$"head={LocalWord.Head} " +
					$"lang={LocalWord.Lang} " +
					$"wordId={LocalWord.Id} " +
					$"propKey={Prop.KStr} " +
					$"propId={Prop.Id}"
				);
			}
		}
		return Task.CompletedTask;
	}

	/// <summary>
	/// normalize 場景下，直接修改複製出的 sqlite。
	/// 這一步只動工作副本，不會碰 repo/tmp 裏的原始樣本。
	/// </summary>
	private static async Task NormalizeZeroIdWordPropsInDb(IServiceProvider RootServiceProvider, CancellationToken Ct){
		using var Scope = RootServiceProvider.CreateScope();
		var Sp = Scope.ServiceProvider;
		var FileSystem = Sp.GetRequiredService<IFileSystem>();
		var TblMgr = Sp.GetRequiredService<ITblMgr>();
		var DbPath = Path.Combine(BaseDirMgr.Inst.GetBaseDir(), RuntimeDbFileName);
		var PropTable = TblMgr.GetTbl<PoWordProp>().DbTblName;
		var ZeroBytes = new byte[16];

		if(!FileSystem.File.Exists(DbPath)){
			throw new FileNotFoundException("Runtime sqlite db not found.", DbPath);
		}

		await using var Connection = new SqliteConnection($"Data Source={DbPath}");
		await Connection.OpenAsync(Ct);

		var SelectSql =
			$"SELECT rowid, WordId, KStr FROM {PropTable} WHERE Id = $zeroId";
		await using var SelectCommand = new SqliteCommand(SelectSql, Connection);
		SelectCommand.Parameters.AddWithValue("$zeroId", ZeroBytes);

		var Targets = new List<ZeroIdDbRow>();
		await using(var Reader = await SelectCommand.ExecuteReaderAsync(Ct)){
			while(await Reader.ReadAsync(Ct)){
				var Row = new ZeroIdDbRow{
					RowId = Reader.GetInt64(0),
					WordId = ConvertBlobToBase64Url((byte[])Reader[1]),
					KStr = Reader.IsDBNull(2) ? null : Reader.GetString(2),
				};
				Targets.Add(Row);
			}
		}

		Console.WriteLine($"[normalize-db] zeroPropRowCount={Targets.Count}");
		foreach(var Target in Targets){
			var NewId = new IdWordProp();
			var UpdateSql =
				$"UPDATE {PropTable} SET Id = $newId WHERE rowid = $rowId";
			await using var UpdateCommand = new SqliteCommand(UpdateSql, Connection);
			UpdateCommand.Parameters.AddWithValue("$newId", NewId.ToByteArr());
			UpdateCommand.Parameters.AddWithValue("$rowId", Target.RowId);
			await UpdateCommand.ExecuteNonQueryAsync(Ct);

			Console.WriteLine(
				"[normalize-db-row] " +
				$"rowId={Target.RowId} " +
				$"wordId={Target.WordId} " +
				$"propKey={Target.KStr} " +
				$"newPropId={NewId}"
			);
		}
	}

	/// <summary>
	/// normalize 場景下也要把包裏的 zero PropId 改掉，否則仍可能在導入時與本地零值或其他異常值碰撞。
	/// </summary>
	private static void NormalizeZeroIdWordPropsInMemory(IList<JnWord> Words, string Label){
		var ZeroId = default(IdWordProp);
		foreach(var Word in Words){
			foreach(var Prop in Word.Props){
				if(Prop.Id != ZeroId){
					continue;
				}
				var NewId = new IdWordProp();
				Console.WriteLine(
					"[normalize-memory-prop] " +
					$"label={Label} " +
					$"head={Word.Head} " +
					$"lang={Word.Lang} " +
					$"wordId={Word.Id} " +
					$"propKey={Prop.KStr} " +
					$"oldPropId={Prop.Id} " +
					$"newPropId={NewId}"
				);
				Prop.Id = NewId;
			}
		}
	}

	/// <summary>
	/// raw 場景只記錄 zero PropId，不做任何修復，保證能真實復現現象。
	/// </summary>
	private static void LogZeroIdWordPropsInMemory(IList<JnWord> Words, string Label){
		var ZeroId = default(IdWordProp);
		foreach(var Word in Words){
			foreach(var Prop in Word.Props){
				if(Prop.Id != ZeroId){
					continue;
				}
				Console.WriteLine(
					"[remote-zero-prop-id] " +
					$"label={Label} " +
					$"head={Word.Head} " +
					$"lang={Word.Lang} " +
					$"wordId={Word.Id} " +
					$"propKey={Prop.KStr} " +
					$"propId={Prop.Id}"
				);
			}
		}
	}

	/// <summary>
	/// 把本地已有 PropId 映射回所屬單詞，用於快速判定“這個遠端 PropId 在本地已經屬於誰”。
	/// </summary>
	private static Dictionary<IdWordProp, PropOwnerInfo> BuildPropOwnerIndex(IList<JnWord> LocalWords){
		var Result = new Dictionary<IdWordProp, PropOwnerInfo>();
		foreach(var LocalWord in LocalWords){
			foreach(var Prop in LocalWord.Props){
				Result[Prop.Id] = new PropOwnerInfo{
					LocalWord = LocalWord,
					LocalHead = LocalWord.Head,
					LocalLang = LocalWord.Lang,
					LocalWordId = LocalWord.Id.ToString(),
				};
			}
		}
		return Result;
	}

	/// <summary>
	/// 只要遠端 PropId 在本地已存在，就打印出本地歸屬。
	/// 這對分析「被判 LocalNotExist 卻撞到唯一約束」非常直接。
	/// </summary>
	private static List<PropIntersectionInfo> FindIntersectedPropIds(
		Dictionary<IdWordProp, PropOwnerInfo> LocalPropOwners,
		JnWord RemoteWord
	){
		var Result = new List<PropIntersectionInfo>();
		foreach(var RemoteProp in RemoteWord.Props){
			if(!LocalPropOwners.TryGetValue(RemoteProp.Id, out var OwnerInfo)){
				continue;
			}
			Result.Add(new PropIntersectionInfo{
				PropId = RemoteProp.Id.ToString(),
				IsSameWord = OwnerInfo.LocalWord.Word.Id == RemoteWord.Word.Id,
				LocalHead = OwnerInfo.LocalHead,
				LocalLang = OwnerInfo.LocalLang,
				LocalWordId = OwnerInfo.LocalWordId,
			});
		}
		return Result;
	}

	/// <summary>
	/// 統一打印同步分類，避免 raw / preview / normalized 三處格式飄散。
	/// </summary>
	private static void LogSyncDto(string Prefix, DtoJnWordSyncResult Dto){
		var LocalWordId = Dto.Local is null ? "<null>" : Dto.Local.Id.ToString();
		var RemoteWordId = Dto.Remote is null ? "<null>" : Dto.Remote.Id.ToString();
		var RemoteHead = Dto.Remote is null ? "<null>" : Dto.Remote.Head;
		var RemoteLang = Dto.Remote is null ? "<null>" : Dto.Remote.Lang;
		Console.WriteLine(
			Prefix + " " +
			$"diff={ToDiffName(Dto.DiffResult)} " +
			$"localWordId={LocalWordId} " +
			$"remoteWordId={RemoteWordId} " +
			$"remoteHead={RemoteHead} " +
			$"remoteLang={RemoteLang} " +
			$"newPropCount={(Dto.NewAssets is null ? 0 : Dto.NewAssets.Props.Count)} " +
			$"changedPropCount={(Dto.ChangedAssets is null ? 0 : Dto.ChangedAssets.Props.Count)} " +
			$"newLearnCount={(Dto.NewAssets is null ? 0 : Dto.NewAssets.Learns.Count)} " +
			$"changedLearnCount={(Dto.ChangedAssets is null ? 0 : Dto.ChangedAssets.Learns.Count)}"
		);
	}

	/// <summary>
	/// 直接把遠端包內重複 PropId 打出來。
	/// 若同一個 PropId 出現在兩個不同單詞裡，RemoteIsNewer 分支進行批量 upsert 時就非常可疑。
	/// </summary>
	private static void LogRemoteDuplicatePropIds(IList<JnWord> RemoteWords){
		var OwnersByPropId = new Dictionary<IdWordProp, List<JnWord>>();
		foreach(var RemoteWord in RemoteWords){
			foreach(var Prop in RemoteWord.Props){
				if(!OwnersByPropId.TryGetValue(Prop.Id, out var Owners)){
					Owners = new List<JnWord>();
					OwnersByPropId[Prop.Id] = Owners;
				}
				Owners.Add(RemoteWord);
			}
		}

		foreach(var Pair in OwnersByPropId){
			if(Pair.Value.Count <= 1){
				continue;
			}
			var Builder = new StringBuilder();
			Builder.Append("[remote-duplicate-prop-id] ");
			Builder.Append($"propId={Pair.Key} ");
			Builder.Append($"ownerCount={Pair.Value.Count} ");
			for(var Index = 0; Index < Pair.Value.Count; Index++){
				var OwnerWord = Pair.Value[Index];
				Builder.Append(
					$"owner{Index}Head={OwnerWord.Head} " +
					$"owner{Index}Lang={OwnerWord.Lang} " +
					$"owner{Index}WordId={OwnerWord.Id} "
				);
			}
			Console.WriteLine(Builder.ToString().TrimEnd());
		}
	}

	private static string ToDiffName(EDiffByBizIdResultForSync Diff){
		if(ReferenceEquals(Diff, EDiffByBizIdResultForSync.NoChange)){
			return nameof(EDiffByBizIdResultForSync.NoChange);
		}
		if(ReferenceEquals(Diff, EDiffByBizIdResultForSync.RemoteIsOlder)){
			return nameof(EDiffByBizIdResultForSync.RemoteIsOlder);
		}
		if(ReferenceEquals(Diff, EDiffByBizIdResultForSync.LocalNotExist)){
			return nameof(EDiffByBizIdResultForSync.LocalNotExist);
		}
		if(ReferenceEquals(Diff, EDiffByBizIdResultForSync.IdNotEqual)){
			return nameof(EDiffByBizIdResultForSync.IdNotEqual);
		}
		if(ReferenceEquals(Diff, EDiffByBizIdResultForSync.RemoteIsNewer)){
			return nameof(EDiffByBizIdResultForSync.RemoteIsNewer);
		}
		if(ReferenceEquals(Diff, EDiffByBizIdResultForSync.Unknown)){
			return nameof(EDiffByBizIdResultForSync.Unknown);
		}
		return Diff.GetType().Name;
	}

	private static string ToHeadLangKey(string Head, string Lang){
		return Head + "\u001f" + Lang;
	}

	private static async Task<List<T>> ToListAsync<T>(IAsyncEnumerable<T> Source, CancellationToken Ct){
		var Result = new List<T>();
		await foreach(var Item in Source.WithCancellation(Ct)){
			Result.Add(Item);
		}
		return Result;
	}

	private static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(IEnumerable<T> Items){
		foreach(var Item in Items){
			yield return Item;
			await Task.Yield();
		}
	}

	/// <summary>
	/// 調試打印時只需要可辨認字符串，直接沿用系統 Low64Base 表示。
	/// </summary>
	private static string ConvertBlobToBase64Url(byte[] Bytes){
		if(Bytes.Length == 16){
			return IdWord.FromByteArr(Bytes).ToString();
		}
		return Convert.ToHexString(Bytes);
	}

	private sealed class ZeroIdDbRow{
		public long RowId { get; set; }
		public string WordId { get; set; } = "";
		public string? KStr { get; set; }
	}

	private sealed class PropOwnerInfo{
		public JnWord LocalWord { get; set; } = new JnWord();
		public string LocalHead { get; set; } = "";
		public string LocalLang { get; set; } = "";
		public string LocalWordId { get; set; } = "";
	}

	private sealed class PropIntersectionInfo{
		public string PropId { get; set; } = "";
		public bool IsSameWord { get; set; }
		public string LocalHead { get; set; } = "";
		public string LocalLang { get; set; } = "";
		public string LocalWordId { get; set; } = "";
	}
}
