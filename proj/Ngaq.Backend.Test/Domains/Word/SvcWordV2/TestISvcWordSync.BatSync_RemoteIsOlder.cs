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
	void RegisterSyncRemoteIsOlder(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestISvcWordV2),
			[typeof(ISvcWordSync)],
			[]
		);
		var R = register.Register;
		register.TesteeFnNames = [nameof(ISvcWordSync.OrdSync_RemoteIsOlder)];

		R("BatSync_RemoteIsOlder_Should_NoOp", async(o)=>{
			if(SvcWordV2 is not ISvcWordSync sync){
				throw new Exception("ISvcWordV2 implementation should also implement ISvcWordSync");
			}
			var owner = new IdUser();
			var token = "ut_wsync_remoteolder_" + Guid.NewGuid().ToString("N");
			var root = new PoWord{Id = new IdWord(), Owner = owner, Head = token + "_h1", Lang = "en"};
			try{
				await RunNoTxn(async(Ctx)=>{
					await RepoWord.OrdAdd(Ctx, AsyE(root), CT.None);
					return NIL;
				});

				var dto = new DtoJnWordSyncResult{
					DiffResult = EDiffByBizIdResultForSync.RemoteIsOlder,
					Local = new JnWord{Word = root},
					Remote = new JnWord{
						Word = new PoWord{Id = root.Id, Owner = owner, Head = root.Head, Lang = root.Lang},
					},
				};
				await sync.OrdSync_RemoteIsOlder(MkUserCtx(owner), AsyE(dto), CT.None);

				await RunNoTxn(async(Ctx)=>{
					var got = await ToList(RepoWord.OrdGetByIdWithDel(Ctx, AsyE(root.Id), CT.None));
					if(got.Count != 1 || got[0] is null || got[0]!.Head != root.Head){
						throw new Exception("BatSync_RemoteIsOlder should keep local root unchanged");
					}
					return NIL;
				});
				return NIL;
			}
			finally{
				await RunNoTxn(async(Ctx)=>{
					await RepoWord.OrdHardDelById(Ctx, AsyE(root.Id), CT.None);
					return NIL;
				});
			}
		});
	}
}

