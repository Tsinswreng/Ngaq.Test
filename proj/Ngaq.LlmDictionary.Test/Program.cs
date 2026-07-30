namespace Ngaq.LlmDictionary.Test;

/// 真實大模型詞典評估的專用進程入口。
///
/// 此程序集不收編通用測試樹；它固定載入本機私有設定，
/// 因此執行此項目即明確表示同意發出真實模型請求。
internal static partial class Program{
	/// 私有只讀設定目錄相對於倉庫根的位置。
	/// API 金鑰保留在此目錄的讀寫設定中，不進入命令列或原始碼。
	const str PrivateConfigDirName = "ExternalRsrc.__Private";

	/// 詞典服務所讀取的只讀設定檔名。
	const str ReadOnlyConfigFileName = "Ngaq.jsonc";

	/// 啟動專用評估進程。
	/// 不接收命令列參數，避免配置來源隨呼叫方式漂移。
	private static partial Task Main();

	/// 從輸出目錄向上定位倉庫根，以解析私有設定的固定位置。
	private static partial str FindRepositoryRoot(str StartDir);

	/// 以私有只讀設定及其指定的讀寫設定初始化雙源配置。
	private static partial void ConfigurePrivateAppCfg(str RepositoryRoot);

	/// 建立服務、取得詞典服務並執行獨立的大模型測試節點。
	private static partial Task RunDictionaryEvaluation();
}
