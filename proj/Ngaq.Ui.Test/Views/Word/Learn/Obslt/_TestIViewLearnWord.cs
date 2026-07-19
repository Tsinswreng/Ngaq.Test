// using Ngaq.Core.Frontend.User;
// using Ngaq.Core.Infra;
// using Ngaq.Core.Shared.StudyPlan.Svc;
// using Ngaq.Core.Shared.Word.Models.Po.Word;
// using Ngaq.Ui.Views.Word.Learn;
// using Tsinswreng.CsSql;
// using Tsinswreng.CsTreeTest;

// namespace Ngaq.Ui.Test.Views.Word.Learn;

// /// <summary>
// /// `IViewLearnWord` 的接口測試主裝配類。
// /// 這一層只面向頁面契約，不關心具體 View / Vm / Converter 實現細節。
// /// </summary>
// public partial class TestIViewLearnWord: ITester{
// 	readonly IViewLearnWord ViewLearnWord;
// 	readonly IFrontendUserCtxMgr UserCtxMgr;
// 	readonly IRepo<PoWord, IdWord> RepoWord;
// 	readonly ISvcStudyPlan SvcStudyPlan;

// 	/// <summary>
// 	/// 由外部 DI 注入被測頁面接口。
// 	/// 後續在 `Ngaq.Windows.Test` 中完成實現綁定後，這組測試即可直接復用。
// 	/// </summary>
// 	/// <param name="ViewLearnWord">被測學習頁面接口實例。</param>
// 	public TestIViewLearnWord(
// 		IViewLearnWord ViewLearnWord
// 		,IFrontendUserCtxMgr UserCtxMgr
// 		,IRepo<PoWord, IdWord> RepoWord
// 		,ISvcStudyPlan SvcStudyPlan
// 	){
// 		this.ViewLearnWord = ViewLearnWord;
// 		this.UserCtxMgr = UserCtxMgr;
// 		this.RepoWord = RepoWord;
// 		this.SvcStudyPlan = SvcStudyPlan;
// 	}

// 	/// <summary>
// 	/// 註冊 `IViewLearnWord` 的各個接口測試。
// 	/// </summary>
// 	/// <param name="Node">當前測試節點。</param>
// 	/// <returns>已註冊子測試的節點。</returns>
// 	public ITestNode RegisterTestsInto(ITestNode? Node){
// 		Node ??= new TestNode();
// 		// 頁面接口測試後續會涉及共享狀態與資料準備，先固定為有序單線程。
// 		Node.Ordered = true;
// 		Node.IsParallelRecursive = false;
// 		RegisterClickStart(Node);
// 		RegisterClickReset(Node);
// 		RegisterClickWordCard(Node);
// 		RegisterWordInfo(Node);
// 		RegisterWordListCards(Node);
// 		RegisterEventDrivenExamples(Node);
// 		return Node;
// 	}

// 	protected async Task<TRtn> RunNoTxn<TRtn>(Func<IDbFnCtx, Task<TRtn>> Fn){
// 		IDbFnCtx Ctx = new DbFnCtx();
// 		try{
// 			return await Fn(Ctx);
// 		}
// 		finally{
// 			await Ctx.DisposeAsync();
// 		}
// 	}

// 	protected static async IAsyncEnumerable<T> AsyE<T>(params T[] Items){
// 		foreach(var item in Items){
// 			yield return item;
// 		}
// 	}
// }
