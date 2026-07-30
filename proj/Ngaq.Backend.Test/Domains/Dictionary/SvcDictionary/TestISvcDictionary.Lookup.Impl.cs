using Ngaq.Core.Shared.Dictionary.Models;
using Ngaq.Core.Shared.Dictionary.Svc;
using Ngaq.Core.Shared.User.UserCtx;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Backend.Test.Domains.Dictionary;

/// Lookup 的真實模型整合測試。
/// 每個用例都以固定語言與文本呼叫目前配置的模型，驗證最終解析出的描述內容。
public partial class TestISvcDictionary{
	/// 註冊本需求的所有真實模型 Lookup 用例。
	private partial void RegisterLookup(ITestNode Node){
		var Register = Node.MkTestFnRegister(
			typeof(TestISvcDictionary),
			[typeof(ISvcDictionary)],
			[nameof(ISvcDictionary.Lookup)]
		);
		Register.Register(nameof(Lookup_FrenchSangToChinese_ShouldReturnBloodMeaning), Lookup_FrenchSangToChinese_ShouldReturnBloodMeaning!);
		Register.Register(nameof(Lookup_SpanishNiToChinese_ShouldReturnSpanishMeaning), Lookup_SpanishNiToChinese_ShouldReturnSpanishMeaning!);
		Register.Register(nameof(Lookup_FrenchSentenceToChinese_ShouldReturnWholeTranslation), Lookup_FrenchSentenceToChinese_ShouldReturnWholeTranslation!);
		Register.Register(nameof(Lookup_SpanishSentenceToChinese_ShouldReturnWholeTranslation), Lookup_SpanishSentenceToChinese_ShouldReturnWholeTranslation!);
	}

	/// 五次併發檢查 fr -> zh 的 sang 均未落入英語 sang 的錯義。
	public partial async Task<nil> Lookup_FrenchSangToChinese_ShouldReturnBloodMeaning(obj? O){
		var Resps = await LookupRepeated("sang", "fr", "zh");
		AssertDescriptionsMatch(
			Resps,
			"fr",
			[["血", "血液"]],
			["唱", "歌", "sing", "past tense", "savoir", "知道", "存在"]
		);
		return NIL;
	}

	/// 五次併發檢查 es -> zh 的 ni 均未落入中文第二人稱的錯義。
	public partial async Task<nil> Lookup_SpanishNiToChinese_ShouldReturnSpanishMeaning(obj? O){
		var Resps = await LookupRepeated("ni", "es", "zh");
		AssertDescriptionsMatch(
			Resps,
			"es",
			[["不", "没有", "沒有", "也不", "既不", "亦不", "也没有", "也沒有"]],
			["你"]
		);
		return NIL;
	}

	/// 驗證法語完整句子返回完整中文譯文，而非僅查其中一個詞。
	public partial async Task<nil> Lookup_FrenchSentenceToChinese_ShouldReturnWholeTranslation(obj? O){
		var Resps = await LookupRepeated("Je suis tres heureux de vous voir.", "fr", "zh");
		AssertDescriptionsMatch(Resps, "fr", [["高兴", "高興"], ["见", "見"]], []);
		return NIL;
	}

	/// 驗證西語完整句子返回完整中文譯文。
	public partial async Task<nil> Lookup_SpanishSentenceToChinese_ShouldReturnWholeTranslation(obj? O){
		var Resps = await LookupRepeated("Me alegra mucho verte.", "es", "zh");
		AssertDescriptionsMatch(Resps, "es", [["高兴", "高興"], ["见", "見", "看"]], []);
		return NIL;
	}

	/// 執行本案例要求的五次併發查詢。
	/// 各測試用例顯式調用此方法決定是否重複評估；此方法只收斂併發執行機制。
	private partial Task<IRespLlmDict[]> LookupRepeated(str InputText, str SrcLang, str TgtLang){
		var Tasks = Enumerable.Range(0, LlmRepeatCount).Select(_=>Lookup(InputText, SrcLang, TgtLang));
		return Task.WhenAll(Tasks);
	}

	/// 使用目前模型設定執行一次詞典查詢。
	private partial Task<IRespLlmDict> Lookup(str InputText, str SrcLang, str TgtLang){
		return SvcDictionary.Lookup(
			new UserCtx(),
			new ReqLlmDict{
				Query = new Query{InputText = InputText},
				OptLang = new OptLang{
					SrcLang = MkLang(SrcLang),
					TgtLangs = [MkLang(TgtLang)],
				},
			},
			CT.None
		);
	}

	/// 按詞典界面查詢前的語言正規化結果補全語言名稱。
	/// 測試必須與界面送給服務的 code、母語名及英文名一致，避免只傳 code 造成模型請求失真。
	private static partial NormLangDetail MkLang(str Code){
		return Code switch{
			"fr" => new NormLangDetail{
				Code = Code,
				NativeName = "Français",
				EnglishName = "French",
			},
			"es" => new NormLangDetail{
				Code = Code,
				NativeName = "Español",
				EnglishName = "Spanish",
			},
			"zh" => new NormLangDetail{
				Code = Code,
				NativeName = "中文",
				EnglishName = "Chinese",
			},
			_ => new NormLangDetail{Code = Code},
		};
	}

	/// 確保每次模型呼叫均產生可用描述，避免「沒有結果」被誤判為未出現錯義。
	private static partial void AssertDescriptionsAreNotEmpty(IList<IRespLlmDict> Resps){
		Assert.IsTrue(Resps.All(Resp=>Resp.Descrs.Any(X=>!str.IsNullOrWhiteSpace(X))), DescribeResponses(Resps));
	}

	/// 斷言每個真實模型回應都帶有非空母語名，且語言代碼等於本次請求來源。
	///
	/// 代碼是可穩定比較的協議主鍵；母語名僅驗證模型實際填寫了新增對象，
	/// 不把不同語言名稱寫法當成非確定性模型輸出的失敗原因。
	private static partial void AssertDetectedInputLanguage(IList<IRespLlmDict> Resps, str ExpectedCode){
		Assert.IsTrue(
			Resps.All(Resp=>
				Resp.DetectedInputLang is not null
				&& Resp.DetectedInputLang.Code.Equals(ExpectedCode, StringComparison.OrdinalIgnoreCase)
				&& !str.IsNullOrWhiteSpace(Resp.DetectedInputLang.NativeName)
			),
			DescribeResponses(Resps)
		);
	}

	/// 驗證每次模型輸出都同時具備正確語義並排除錯義。
	/// RequiredGroups 的每一組至少命中一個詞；Forbidden 中任一字串出現即失敗。
	private static partial void AssertDescriptionsMatch(
		IList<IRespLlmDict> Resps,
		str ExpectedDetectedInputLangCode,
		IList<IList<str>> RequiredGroups,
		IList<str> Forbidden
	){
		AssertDescriptionsAreNotEmpty(Resps);
		AssertDetectedInputLanguage(Resps, ExpectedDetectedInputLangCode);
		var Descriptions = Resps.Select(Resp=>string.Join("\n", Resp.Descrs));
		Assert.IsTrue(
			Descriptions.All(Text=>
				RequiredGroups.All(Group=>Group.Any(Word=>Text.Contains(Word, StringComparison.OrdinalIgnoreCase)))
				&& Forbidden.All(Word=>!Text.Contains(Word, StringComparison.OrdinalIgnoreCase))
			),
			$"Required groups: {string.Join("; ", RequiredGroups.Select(Group=>string.Join(", ", Group)))}. " +
			$"Forbidden meanings: {string.Join(", ", Forbidden)}. {DescribeResponses(Resps)}"
		);
	}

	/// 將所有重複呼叫的描述集中到錯誤訊息，方便定位是哪一次模型輸出不符合預期。
	private static partial str DescribeResponses(IList<IRespLlmDict> Resps){
		return string.Join(" || ", Resps.Select((Resp, Index)=>$"[{Index + 1}] {string.Join(" | ", Resp.Descrs)}"));
	}
}
