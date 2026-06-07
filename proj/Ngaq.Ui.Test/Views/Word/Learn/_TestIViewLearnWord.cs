using Avalonia.Media;
using Ngaq.Ui.Views.Word.Learn;
using Ngaq.Ui.Views.Word.WordCard;
using Ngaq.Ui.Views.Word.WordInfo;
using Tsinswreng.CsCore;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Ui.Test.Views.Word.Learn;

/// <summary>
/// `IViewLearnWord` 的接口測試主裝配類。
/// 這一層只面向頁面契約，不關心具體 View / Vm / Converter 實現細節。
/// </summary>
public partial class TestIViewLearnWord: ITester{
	readonly IViewLearnWord ViewLearnWord;

	/// <summary>
	/// 由外部 DI 注入被測頁面接口。
	/// 後續在 `Ngaq.Windows.Test` 中完成實現綁定後，這組測試即可直接復用。
	/// </summary>
	/// <param name="ViewLearnWord">被測學習頁面接口實例。</param>
	public TestIViewLearnWord(IViewLearnWord ViewLearnWord){
		this.ViewLearnWord = ViewLearnWord;
	}

	/// <summary>
	/// 註冊 `IViewLearnWord` 的各個接口測試。
	/// </summary>
	/// <param name="Node">當前測試節點。</param>
	/// <returns>已註冊子測試的節點。</returns>
	public ITestNode RegisterTestsInto(ITestNode? Node){
		Node ??= new TestNode();
		// 頁面接口測試後續會涉及共享狀態與資料準備，先固定為有序單線程。
		Node.Ordered = true;
		Node.IsParallelRecursive = false;
		RegisterClickStart(Node);
		RegisterClickReset(Node);
		RegisterClickWordCard(Node);
		RegisterWordInfo(Node);
		RegisterWordListCards(Node);
		return Node;
	}

	/// <summary>
	/// 測試用的 `IViewWordInfo` 假實現，用來驗證接口層的入參/出參契約。
	/// </summary>
	internal sealed class FakeWordInfo: IViewWordInfo{
		public str HeadText{get;set;} = "";
		public IList<str>? Descrs{get;set;} = [];
	}

	/// <summary>
	/// 測試用的 `IViewWordListCard` 假實現。
	/// 後續若要加深斷言，可在此補更多可觀察狀態。
	/// </summary>
	internal sealed class FakeWordListCard: IViewWordListCard{
		public str IndexText{get;set;} = "";
		public str LangText{get;set;} = "";
		public str HeadText{get;set;} = "";
		public str LearnHistoryText{get;set;} = "";
		public str LastLearnedTimeText{get;set;} = "";
		public str WeightText{get;set;} = "";
		public IBrush HeadFontColor{get;set;} = Brushes.Transparent;
		public IBrush LearnedColor{get;set;} = Brushes.Transparent;

		/// <summary>
		/// 假卡片點擊，當前僅保持接口可調用。
		/// </summary>
		/// <param name="Ct">取消令牌。</param>
		/// <returns>固定返回空值。</returns>
		public Task<nil> Click(CT Ct){
			return Task.FromResult<nil>(NIL);
		}
	}
}
