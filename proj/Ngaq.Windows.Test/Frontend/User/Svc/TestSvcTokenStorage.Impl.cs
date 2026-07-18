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
	

	/// 注入本地前端 token storage 服務。
	/// <param name="SvcTokenStorage">客戶端本地 token storage 實現。</param>
	public partial TestSvcTokenStorage(ISvcTokenStorage SvcTokenStorage){
		this.SvcTokenStorage = SvcTokenStorage;
	}

	/// 註冊 token storage 相關回歸測試。
	/// <param name="Node">當前測試節點。</param>
	/// <returns>已註冊子測試的節點。</returns>
	public partial ITestNode RegisterTestsInto(ITestNode? Node){
		Node ??= new TestNode();
		Node.Ordered = true;

		var register = Node.MkTestFnRegister(
			typeof(TestSvcTokenStorage)
			,[typeof(ISvcTokenStorage)]
			,[]
			,nameof(TestSvcTokenStorage)
		);
		var R = register.Register;

		R(nameof(SetRefreshToken_Twice_Should_OverwriteExistingClientKv), SetRefreshToken_Twice_Should_OverwriteExistingClientKv!);

		return Node;
	}
	
	public partial async Task<nil> SetRefreshToken_Twice_Should_OverwriteExistingClientKv(obj? o){
		var RefreshToken1 = "ut_refresh_token_" + Guid.NewGuid().ToString("N") + "_1";
		var RefreshToken2 = "ut_refresh_token_" + Guid.NewGuid().ToString("N") + "_2";

		// 先模擬首次登錄，把 refresh token 寫入本地 kv。
		await SvcTokenStorage.SetRefreshToken(new ReqSetRefreshToken{
			LoginUserId = new IdUser(),
			RefreshToken = RefreshToken1,
			RefreshTokenExpireAt = UnixMs.FromUnixMs(1000),
		}, CT.None);

		// 再模擬登出，驗證清空同鍵資料時不會因唯一鍵衝突而失敗。
		await SvcTokenStorage.SetRefreshToken(new ReqSetRefreshToken{
			LoginUserId = IdUser.Zero,
			RefreshToken = null!,
			RefreshTokenExpireAt = UnixMs.Zero,
		}, CT.None);

		var Got = await SvcTokenStorage.GetRefreshToken(CT.None);
		if(Got == RefreshToken1 || Got == RefreshToken2){
			throw new Exception($"Expected refresh token to be cleared after logout, got '{Got}'.");
		}

		// 最後模擬重新登錄，確認同一鍵可以被新 token 正常覆寫。
		await SvcTokenStorage.SetRefreshToken(new ReqSetRefreshToken{
			LoginUserId = new IdUser(),
			RefreshToken = RefreshToken2,
			RefreshTokenExpireAt = UnixMs.FromUnixMs(2000),
		}, CT.None);

		var GotAfterRelogin = await SvcTokenStorage.GetRefreshToken(CT.None);
		if(GotAfterRelogin != RefreshToken2){
			throw new Exception($"Expected refresh token to be overwritten by relogin, got '{GotAfterRelogin}'.");
		}

		return NIL;
	}
}
