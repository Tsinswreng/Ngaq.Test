namespace Ngaq.Windows.Test.Frontend.User.Svc;

using Ngaq.Core.Frontend.User.Svc;
using Ngaq.Core.Infra.IF;
using Ngaq.Core.Shared.User.Models.Po.User;
using Tsinswreng.CsTempus;
using Tsinswreng.CsTreeTest;

/// 前端本地 token storage 的回歸測試：
/// 先寫入一筆 refresh token，再模擬 logout 寫回空值，驗證不會因同鍵重複寫入而拋唯一鍵例外。
/// 此測試依賴本地前端 DI，應放在客戶端專用測試程序集而非公共後端測試程序集。
public partial class TestSvcTokenStorage: ITester{
	readonly ISvcTokenStorage SvcTokenStorage;

	/// 注入本地前端 token storage 服務。
	/// <param name="SvcTokenStorage">客戶端本地 token storage 實現。</param>
	public partial TestSvcTokenStorage(ISvcTokenStorage SvcTokenStorage);

	/// 註冊 token storage 相關回歸測試。
	/// <param name="Node">當前測試節點。</param>
	/// <returns>已註冊子測試的節點。</returns>
	public partial ITestNode RegisterTestsInto(ITestNode? Node);
		
	
	public partial Task<nil> SetRefreshToken_Twice_Should_OverwriteExistingClientKv(obj? o);
}
