namespace Ngaq.Ui.Test.Views.Word.WordEditV2;

using Tsinswreng.CsTreeTest;

/// 驗證使用者從單詞編輯頁刪除既有資料的完整功能流程。
public partial class TestViewWordEditV2{
	/// 註冊刪除流程的資料庫結果驗證用例。
	public partial void RegisterDelete(ITestNode Node);

	/// 載入種子單詞並點擊刪除後，資料庫中的該單詞應被軟刪除且不再出現在普通查詢中。
	/// 用例結束後仍需硬刪種子資料，避免測試資料污染後續執行。
	public partial Task<nil> Delete_Existing_Word_Through_Button_Should_Soft_Delete_Seed_Data(obj? O);
}
