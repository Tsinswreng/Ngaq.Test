using Ngaq.Core.Shared.Kv.Svc;
using Ngaq.Core.Shared.User.Models.Po.User;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Backend.Test.Domains.Kv;

public partial class TestISvcKv{
	/// <summary>
	/// 測試 ISvcKv.BatGetByOwnerEtKI64:
	/// 傳入不可重複消費的一次性 IAsyncEnumerable，驗證能正確返回且不拋重複消費錯誤。
	/// </summary>
	public void RegisterBatGetByOwnerEtKI64(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestISvcKv),
			[typeof(ISvcKv)],
			[nameof(ISvcKv.OrdGetByOwnerEtKI64)],
			nameof(TestISvcKv)
		);
		var R = register.Register;

		R("BatGetByOwnerEtKI64_OneShotSource_Should_WorkAndKeepOrder", async(o)=>{
			var input = new OneShotAsyE<(IdUser, long)>(
				(_ownerA, 11),
				(_ownerA, 999_999), // miss
				(_ownerB, 11),
				(new IdUser(), 11)   // miss
			);

			var got = await ToList(
				SvcKv.OrdGetByOwnerEtKI64(null, input, CT.None)
			);

			if(got.Count != 4){
				throw new Exception($"Expected 4 results, got {got.Count}");
			}
			if(got[0] is null || got[0]!.Id != _kvA_I64_11.Id){
				throw new Exception("Index 0 should match ownerA/key11.");
			}
			if(got[1] is not null){
				throw new Exception("Index 1 should be null for missing key.");
			}
			if(got[2] is null || got[2]!.Id != _kvB_I64_11.Id){
				throw new Exception("Index 2 should match ownerB/key11.");
			}
			if(got[3] is not null){
				throw new Exception("Index 3 should be null for missing owner.");
			}

			return NIL;
		});
	}
}
