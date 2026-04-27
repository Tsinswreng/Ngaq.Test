namespace Ngaq.Backend.Test.Frontend.User.Svc;

using Ngaq.Core.Frontend.User.Svc;
using Ngaq.Core.Infra.IF;
using Ngaq.Core.Shared.User.Models.Po.User;
using Tsinswreng.CsTempus;
using Tsinswreng.CsTreeTest;

/// <summary>
/// 前端本地 token storage 的回歸測試：
/// 先寫入一筆 refresh token，再模擬 logout 寫回空值，驗證不會因同鍵重複寫入而拋唯一鍵例外。
/// </summary>
public partial class TestSvcTokenStorage: ITester{
	readonly ISvcTokenStorage SvcTokenStorage;

	public TestSvcTokenStorage(ISvcTokenStorage SvcTokenStorage){
		this.SvcTokenStorage = SvcTokenStorage;
	}

	public ITestNode RegisterTestsInto(ITestNode? Node){
		Node ??= new TestNode();
		Node.Ordered = true;

		var register = Node.MkTestFnRegister(
			typeof(TestSvcTokenStorage)
			,[typeof(ISvcTokenStorage)]
			,[]
			,nameof(TestSvcTokenStorage)
		);
		var R = register.Register;

		R("SetRefreshToken_Twice_Should_OverwriteExistingClientKv", async(o)=>{
			var RefreshToken1 = "ut_refresh_token_" + Guid.NewGuid().ToString("N") + "_1";
			var RefreshToken2 = "ut_refresh_token_" + Guid.NewGuid().ToString("N") + "_2";

			await SvcTokenStorage.SetRefreshToken(new ReqSetRefreshToken{
				LoginUserId = new IdUser(),
				RefreshToken = RefreshToken1,
				RefreshTokenExpireAt = UnixMs.FromUnixMs(1000),
			}, CT.None);

			await SvcTokenStorage.SetRefreshToken(new ReqSetRefreshToken{
				LoginUserId = IdUser.Zero,
				RefreshToken = null!,
				RefreshTokenExpireAt = UnixMs.Zero,
			}, CT.None);

			var Got = await SvcTokenStorage.GetRefreshToken(CT.None);
			if(Got == RefreshToken1 || Got == RefreshToken2){
				throw new Exception($"Expected refresh token to be cleared after logout, got '{Got}'.");
			}

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
		});

		return Node;
	}
}
