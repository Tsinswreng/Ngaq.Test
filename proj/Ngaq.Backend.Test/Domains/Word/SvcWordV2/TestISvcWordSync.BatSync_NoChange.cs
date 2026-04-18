using Ngaq.Core.Shared.Sync;
using Ngaq.Core.Shared.User.Models.Po.User;
using Ngaq.Core.Shared.Word.Models;
using Ngaq.Core.Shared.Word.Models.Dto;
using Ngaq.Core.Shared.Word.Models.Po.Word;
using Ngaq.Core.Shared.Word.Svc;
using Tsinswreng.CsSql;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Backend.Test.Domains.Word;

public partial class TestISvcWordV2{
	void RegisterSyncNoChange(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestISvcWordV2),
			[typeof(ISvcWordSync)],
			[]
		);
		var R = register.Register;
		register.TesteeFnNames = [nameof(ISvcWordSync.BatSync_NoChange)];

		R("BatSync_NoChange_Should_NoOp", async(o)=>{
			if(SvcWordV2 is not ISvcWordSync sync){
				throw new Exception("ISvcWordV2 implementation should also implement ISvcWordSync");
			}
			var owner = new IdUser();
			var token = "ut_wsync_nochange_" + Guid.NewGuid().ToString("N");
			var root = new PoWord{Id = new IdWord(), Owner = owner, Head = token + "_h1", Lang = "en"};
			try{
				await RunNoTxn(async(Ctx)=>{
					await RepoWord.BatAdd(Ctx, AsyE(root), CT.None);
					return NIL;
				});

				var dto = new DtoJnWordSyncResult{
					DiffResult = EDiffByBizIdResultForSync.NoChange,
					Local = new JnWord{Word = root},
					Remote = new JnWord{Word = root},
				};
				await sync.BatSync_NoChange(MkUserCtx(owner), AsyE(dto), CT.None);

				await RunNoTxn(async(Ctx)=>{
					var got = await ToList(RepoWord.BatGetByIdWithDel(Ctx, AsyE(root.Id), CT.None));
					if(got.Count != 1 || got[0] is null || got[0]!.Head != root.Head){
						throw new Exception("BatSync_NoChange should keep local root unchanged");
					}
					return NIL;
				});
				return NIL;
			}
			finally{
				await RunNoTxn(async(Ctx)=>{
					await RepoWord.BatHardDelById(Ctx, AsyE(root.Id), CT.None);
					return NIL;
				});
			}
		});
	}
}

