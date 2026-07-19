namespace Ngaq.Ui.Test.Views.Word.WordEditV2;

using Ngaq.Core.Frontend.User;
using Ngaq.Core.Infra;
using Ngaq.Core.Shared.Word.Models.Po.Word;
using Ngaq.Ui.Views.Word.WordEditV2;
using Tsinswreng.CsSql;
using Tsinswreng.CsTreeTest;

/// 實現 `ViewWordEditV2` 功能測試的裝配與共用基礎設施。
public partial class TestViewWordEditV2{
	/// 注入前端測試環境中的真實使用者上下文與單詞倉儲。
	public partial TestViewWordEditV2(
		IFrontendUserCtxMgr UserCtxMgr
		,IRepo<PoWord, IdWord> RepoWord
	){
		this.UserCtxMgr = UserCtxMgr;
		this.RepoWord = RepoWord;
	}

	/// 組裝保存與刪除功能測試；UI 與資料庫狀態共享，故禁止並行執行。
	public partial ITestNode RegisterTestsInto(ITestNode? Node){
		Node ??= new TestNode();
		Node.Ordered = true;
		Node.IsParallelRecursive = false;
		RegisterSave(Node);
		RegisterDelete(Node);
		return Node;
	}

	/// 在 Avalonia UI 線程建立全新 View，確保控件、綁定及事件均使用實際運行路徑。
	protected partial async Task<ViewWordEditV2> MkView(CT Ct){
		//Ct.ThrowIfCancellationRequested();
		return await UiTestTools.RunOnUiAsync(()=>new ViewWordEditV2());
	}

	/// 使用獨立無事務上下文執行驗證或清理，並確保連接資源被釋放。
	protected partial async Task<TRtn> RunNoTxn<TRtn>(Func<IDbFnCtx, Task<TRtn>> Fn){
		IDbFnCtx Ctx = new DbFnCtx();
		try{
			return await Fn(Ctx);
		}finally{
			await Ctx.DisposeAsync();
		}
	}
}
