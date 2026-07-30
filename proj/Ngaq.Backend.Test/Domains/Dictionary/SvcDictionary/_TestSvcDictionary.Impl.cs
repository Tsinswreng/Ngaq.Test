using Tsinswreng.CsTreeTest;
using Ngaq.Core.Shared.Dictionary.Svc;

namespace Ngaq.Backend.Test.Domains.Dictionary;

/// 大模型詞典測試器的組裝實作。
public partial class TestISvcDictionary{
	/// 使用既有詞典服務建立可單獨執行的大模型評估節點。
	public static partial ITestNode MkNode(ISvcDictionary SvcDictionary){
		var Node = new TestNode{
			// 每個測例本身已併發執行五次，節點間不再額外併發以免打爆模型端點。
			Ordered = true,
			IsParallelRecursive = true,
		};
		return new TestISvcDictionary(SvcDictionary).RegisterTestsInto(Node);
	}

	/// 在獨立節點上掛入 Lookup 行為用例。
	private partial ITestNode RegisterTestsInto(ITestNode Node){
		RegisterLookup(Node);
		return Node;
	}
}
