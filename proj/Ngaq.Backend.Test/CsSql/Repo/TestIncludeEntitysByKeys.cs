using Tsinswreng.CsTreeTest;

namespace Ngaq.Backend.Test.CsSql.Repo;

/// 為 IRepo.IncludeEntitysByKeys 的兩個 overload 提供獨立測試聲明。
public partial class TestRepo{
	/// 註冊非泛型表與泛型表 overload 的測試用例。
	public partial void RegisterIncludeEntitysByKeys(ITestNode Node);

	/// 驗證非泛型 ITable overload 會分組多個 key、過濾 null key，並排除軟刪資料。
	public partial Task<nil> IncludeEntitysByKeysUntypedTableGroupsKeysFiltersNullAndExcludesDeleted(obj? O);

	/// 驗證泛型 ITable overload 在 IncludeDeleted=true 時包含軟刪資料，且接受空 key 集合。
	public partial Task<nil> IncludeEntitysByKeysTypedTableIncludesDeletedAndAcceptsEmptyKeys(obj? O);
}
