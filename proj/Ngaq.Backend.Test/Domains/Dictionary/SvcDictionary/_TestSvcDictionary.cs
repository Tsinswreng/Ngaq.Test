using Tsinswreng.CsTreeTest;
using Ngaq.Core.Shared.Dictionary.Svc;

namespace Ngaq.Backend.Test.Domains.Dictionary;

/// 大模型詞典查詢行為的測試入口。
///
/// 此節點不加入通用測試樹，避免一般測試意外產生真實模型呼叫。
/// 需要評估模型品質時，由呼叫端以目前的 ISvcDictionary 單獨建立並執行此節點。
public partial class TestISvcDictionary{
	/// 受測服務；測試直接走目前已配置的大模型 API。
	readonly ISvcDictionary SvcDictionary;
	/// 每個語義案例的獨立真實模型呼叫次數。
	/// 所有次數均通過，才視為該案例通過。
	const i32 LlmRepeatCount = 5;

	/// 由依賴注入取得詞典服務。
	public TestISvcDictionary(ISvcDictionary SvcDictionary){
		this.SvcDictionary = SvcDictionary;
	}

	/// 建立不掛入通用測試樹的大模型評估節點。
	/// 呼叫端可將返回節點交給 TreeTestExecutor 單獨執行。
	public static partial ITestNode MkNode(ISvcDictionary SvcDictionary);

	/// 在指定節點上註冊 Lookup 評估案例。
	private partial ITestNode RegisterTestsInto(ITestNode Node);
}
