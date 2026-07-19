namespace Ngaq.Ui.Test.Views.Word.WordEditV2;

using Tsinswreng.CsTreeTest;

/// 驗證使用者透過基本資料表單及真實保存按鈕完成新增與修改。
public partial class TestViewWordEditV2{
	/// 註冊保存流程的資料庫結果驗證用例。
	public partial void RegisterSave(ITestNode Node);

	/// 在新增模式下修改詞頭及語言控件並點擊保存後，資料庫應新增內容一致的單詞。
	/// 此流程同時覆蓋輸入綁定、按鈕接線、真實服務調用及持久化結果。
	public partial Task<nil> Add_New_Word_Through_Form_Should_Persist_Input(obj? O);

	/// 載入已存在的種子單詞，修改表單並點擊保存後，原資料應被更新而非重複新增。
	public partial Task<nil> Edit_Existing_Word_Through_Form_Should_Update_Seed_Data(obj? O);

	/// 提交空詞頭時不應新增或修改資料，且頁面應向使用者顯示保存失敗原因。
	public partial Task<nil> Save_With_Empty_Head_Should_Reject_Operation_Without_Data_Change(obj? O);
}
