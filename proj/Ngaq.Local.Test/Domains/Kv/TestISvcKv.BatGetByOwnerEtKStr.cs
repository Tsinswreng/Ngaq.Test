using Ngaq.Core.Shared.Kv.Svc;
using Ngaq.Core.Shared.User.Models.Po.User;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Local.Test.Domains.Kv;

public partial class TestISvcKv{
	/// <summary>
	/// 測試 ISvcKv.BatGetByOwnerEtKStr:
	/// 傳入不可重複消費的一次性 IAsyncEnumerable，驗證能正確返回且不拋重複消費錯誤。
	/// </summary>
	public void RegisterBatGetByOwnerEtKStr(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestISvcKv),
			[typeof(ISvcKv)],
			[nameof(ISvcKv.BatGetByOwnerEtKStr)],
			nameof(TestISvcKv)
		);
		var R = register.Register;

		R("BatGetByOwnerEtKStr_OneShotSource_Should_WorkAndKeepOrder", async(o)=>{
			var keyA = _token + "_k_a";
			var keyMissing = _token + "_k_missing";
			var input = new OneShotAsyE<(IdUser, string)>(
				(_ownerA, keyA),
				(_ownerA, keyMissing), // miss
				(_ownerB, keyA),
				(new IdUser(), keyA)   // miss
			);

			var got = await ToList(
				SvcKv.BatGetByOwnerEtKStr(null, input, CT.None)
			);

			if(got.Count != 4){
				throw new Exception($"Expected 4 results, got {got.Count}");
			}
			if(got[0] is null || got[0]!.Id != _kvA_Str_A.Id){
				throw new Exception("Index 0 should match ownerA/keyA.");
			}
			if(got[1] is not null){
				throw new Exception("Index 1 should be null for missing key.");
			}
			if(got[2] is null || got[2]!.Id != _kvB_Str_A.Id){
				throw new Exception("Index 2 should match ownerB/keyA.");
			}
			if(got[3] is not null){
				throw new Exception("Index 3 should be null for missing owner.");
			}

			return NIL;
		});
	}
}
