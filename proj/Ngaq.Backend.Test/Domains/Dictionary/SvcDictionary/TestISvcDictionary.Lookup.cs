using Ngaq.Core.Shared.Dictionary.Models;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Backend.Test.Domains.Dictionary;

/// 大模型詞典 Lookup 的本需求測試宣告。
///
/// 每個用例均以來源語言、目標語言與輸入文本構造請求，對真實模型併發執行五次。
/// 所有回應都必須通過同一語義檢查；測試不要求模型重複產生完全相同的文字。
public partial class TestISvcDictionary{
	/// 註冊大模型詞典 Lookup 的來源語言與整句翻譯行為測試。
	///
	/// 所有用例都通過同一個 Lookup 入口，確保請求組裝、模型呼叫及 YamlMd 解析一起受測。
	/// 本節點不由 LocalTestMgr 註冊，需由呼叫端單獨建立並執行。
	private partial void RegisterLookup(ITestNode Node);

	/// 驗證 fr -> zh 的 sang 不會被誤解為英語 sang 的詞義。
	///
	/// 此案例覆蓋拉丁字母同形、但來源語言不同時，來源語言必須優先於拼寫印象。
	public partial Task<nil> Lookup_FrenchSangToChinese_ShouldReturnBloodMeaning(obj? O);

	/// 驗證 es -> zh 的 ni 不會被誤解為漢字「你」。
	///
	/// 此案例覆蓋拉丁字母輸入與漢字讀音相近時，模型不得跳過指定的西班牙語來源。
	public partial Task<nil> Lookup_SpanishNiToChinese_ShouldReturnSpanishMeaning(obj? O);

	/// 驗證法語完整句子在 fr -> zh 時返回完整中文譯文，而不是其中任一詞的釋義。
	///
	/// 斷言只要求 Descrs 含有完整譯文；Head 與讀音維持模型既有的輸出語義，
	/// 不由此需求或此測試擅自限定。
	public partial Task<nil> Lookup_FrenchSentenceToChinese_ShouldReturnWholeTranslation(obj? O);

	/// 驗證西語完整句子在 es -> zh 時返回完整中文譯文。
	///
	/// 此案例與單詞 ni 搭配，防止模型只在單詞查詢時遵守來源語言、在句子翻譯時又重新猜語言。
	public partial Task<nil> Lookup_SpanishSentenceToChinese_ShouldReturnWholeTranslation(obj? O);

	/// 由每個用例顯式調用，對同一查詢發出規定次數的併發真實模型請求。
	private partial Task<IRespLlmDict[]> LookupRepeated(str InputText, str SrcLang, str TgtLang);

	/// 以界面同構的請求資料執行一次詞典查詢。
	private partial Task<IRespLlmDict> Lookup(str InputText, str SrcLang, str TgtLang);

	/// 建立與詞典界面正規化結果相同的語言資料。
	private static partial NormLangDetail MkLang(str Code);

	/// 確保每一次模型回應都有可檢查的描述內容。
	private static partial void AssertDescriptionsAreNotEmpty(IList<IRespLlmDict> Resps);

	/// 確保每一次模型回應均明確回傳並正確判定輸入語言。
	private static partial void AssertDetectedInputLanguage(IList<IRespLlmDict> Resps, str ExpectedCode);

	/// 確保每一次模型回應同時滿足正確語義錨點並不含錯義。
	private static partial void AssertDescriptionsMatch(
		IList<IRespLlmDict> Resps,
		str ExpectedDetectedInputLangCode,
		IList<IList<str>> RequiredGroups,
		IList<str> Forbidden
	);

	/// 將所有重複請求的描述彙總為斷言失敗訊息。
	private static partial str DescribeResponses(IList<IRespLlmDict> Resps);
}
