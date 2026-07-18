using Tsinswreng.CsTreeTest;

namespace Ngaq.Backend.Test.CsSql.Repo;

/// 為 IRepo.OrdSoftDelById 提供獨立測試聲明，避免與其他刪除 API 混在同一測試分部。
public partial class TestRepo{
	/// 註冊有序軟刪 API 的測試用例。
	public partial void RegisterOrdSoftDelById(ITestNode Node);

	/// 驗證有序軟刪會標記已存在資料、忽略不存在 ID，且空輸入不會破壞資料。
	public partial Task<nil> OrdSoftDelByIdMarksExistingRowsAndAcceptsMissingOrEmptyIds(obj? O);
}
