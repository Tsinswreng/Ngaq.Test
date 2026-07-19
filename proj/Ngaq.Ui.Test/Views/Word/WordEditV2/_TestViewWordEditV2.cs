namespace Ngaq.Ui.Test.Views.Word.WordEditV2;

using Ngaq.Core.Frontend.User;
using Ngaq.Core.Infra;
using Ngaq.Core.Shared.Word.Models.Po.Word;
using Ngaq.Ui.Views.Word.WordEditV2;
using Tsinswreng.CsSql;
using Tsinswreng.CsTreeTest;

/// 從具體 `ViewWordEditV2` 發起新增、修改及刪除操作，驗證完整前端功能流程。
/// 每個用例建立獨立 View，並使用真實測試依賴及可清理的唯一種子數據。
public partial class TestViewWordEditV2: ITester{
	/// 提供目前測試使用者及資料庫上下文。
	readonly IFrontendUserCtxMgr UserCtxMgr;

	/// 供各功能用例準備種子資料、查庫驗證，並在結束後硬刪測試資料。
	readonly IRepo<PoWord, IdWord> RepoWord;

	/// 注入前端測試環境中的真實使用者上下文與單詞倉儲。
	public partial TestViewWordEditV2(
		IFrontendUserCtxMgr UserCtxMgr
		,IRepo<PoWord, IdWord> RepoWord
	);

	/// 組裝 `ViewWordEditV2` 的各項具體 View 測試。
	public partial ITestNode RegisterTestsInto(ITestNode? Node);

	/// 建立一個使用測試環境真實 DI 的全新 View，避免不同用例共用控件狀態。
	protected partial Task<ViewWordEditV2> MkView(CT Ct);

	/// 使用無事務函數上下文執行直接查庫或清理操作。
	protected partial Task<TRtn> RunNoTxn<TRtn>(Func<IDbFnCtx, Task<TRtn>> Fn);

}
