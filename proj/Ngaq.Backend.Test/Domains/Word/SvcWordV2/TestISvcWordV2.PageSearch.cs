using Ngaq.Core.Shared.Word.Svc;
using Tsinswreng.CsPage;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Backend.Test.Domains.Word;

public partial class TestISvcWordV2{
	/// <summary>
	/// 註冊 <see cref="ISvcWordV2.PageSearch"/> 的全部測試用例。
	/// 用例覆蓋 ID 命中、字首搜尋、排序分頁及軟刪除資產過濾。
	/// </summary>
	partial void RegisterPageSearch(ITestNode Node);

	/// <summary>
	/// 建立供 <see cref="ISvcWordV2.PageSearch"/> 使用的分頁查詢條件。
	/// </summary>
	private static partial IPageQry MkPageQry(u64 PageIdx, u64 PageSize, bool WantTotCnt);

	/// 驗證詞 ID 精確命中會返回詞本身。
	public partial Task<nil> PageSearchWhenRawStrIsWordIdShouldReturnExactWordHit(obj? O);
	/// 驗證詞屬性 ID 精確命中會攜帶所屬詞。
	public partial Task<nil> PageSearchWhenRawStrIsPropIdShouldReturnExactPropHitAndRoot(obj? O);
	/// 驗證學習記錄 ID 精確命中會攜帶所屬詞。
	public partial Task<nil> PageSearchWhenRawStrIsLearnIdShouldReturnExactLearnHitAndRoot(obj? O);
	/// 驗證詞頭字首搜尋的排序、分頁與所有者過濾。
	public partial Task<nil> PageSearchWhenPrefixMatchedShouldOrderByHeadAndSliceByPageQry(obj? O);
	/// 驗證詞頭完全命中排在同前綴的其他結果之前。
	public partial Task<nil> PageSearchWhenRawStrMatchesExactHeadAndHeadPrefixShouldOrderExactFirst(obj? O);
	/// 驗證 ID 命中後按接口優先級短路，不再返回詞頭命中結果。
	public partial Task<nil> PageSearchWhenRawStrMatchesIdAndHeadShouldReturnOnlyIdTier(obj? O);
	/// 驗證返回聚合詞不含其他已軟刪除資產。
	public partial Task<nil> PageSearchWhenExactPropHitReturnedJnWordShouldExcludeOtherSoftDeletedAssets(obj? O);
}
