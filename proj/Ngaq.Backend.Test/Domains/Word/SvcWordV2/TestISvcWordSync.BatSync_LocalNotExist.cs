using Ngaq.Core.Model.Po.Learn_;
using Ngaq.Core.Model.Po.Kv;
using Ngaq.Core.Shared.Sync;
using Ngaq.Core.Shared.User.Models.Po.User;
using Ngaq.Core.Shared.Word.Models;
using Ngaq.Core.Shared.Word.Models.Dto;
using Ngaq.Core.Shared.Word.Models.Learn_;
using Ngaq.Core.Shared.Word.Models.Po.Kv;
using Ngaq.Core.Shared.Word.Models.Po.Learn;
using Ngaq.Core.Shared.Word.Models.Po.Word;
using Ngaq.Core.Shared.Word.Svc;
using Tsinswreng.CsSql;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Backend.Test.Domains.Word;

public partial class TestISvcWordV2{
	void RegisterSyncLocalNotExist(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestISvcWordV2),
			[typeof(ISvcWordSync)],
			[]
		);
		var R = register.Register;
		register.TesteeFnNames = [nameof(ISvcWordSync.BatSync_LocalNotExist)];

		R("BatSync_LocalNotExist_Should_InsertRemoteWordAndAssets", async(o)=>{
			if(SvcWordV2 is not ISvcWordSync sync){
				throw new Exception("ISvcWordV2 implementation should also implement ISvcWordSync");
			}
			var owner = new IdUser();
			var token = "ut_wsync_localnotexist_" + Guid.NewGuid().ToString("N");
			var remote = MkRemote(owner, token + "_h1", "en", token + "_d1");
			try{
				var dto = new DtoJnWordSyncResult{
					DiffResult = EDiffByBizIdResultForSync.LocalNotExist,
					Remote = remote,
					SyncedRoot = remote,
				};
				await sync.BatSync_LocalNotExist(MkUserCtx(owner), AsyE(dto), CT.None);

				await RunNoTxn(async(Ctx)=>{
					var roots = (await ToList(RepoWord.GetAll(Ctx, CT.None)))
						.Where(x=>x.Owner == owner && x.Head == remote.Head && x.Lang == remote.Lang)
						.ToList();
					if(roots.Count != 1){
						throw new Exception("BatSync_LocalNotExist should insert remote root");
					}
					var wid = roots[0].Id;
					var props = (await ToList(RepoProp.GetAll(Ctx, CT.None))).Where(x=>x.WordId == wid).ToList();
					var learns = (await ToList(RepoLearn.GetAll(Ctx, CT.None))).Where(x=>x.WordId == wid).ToList();
					if(props.Count == 0 || learns.Count == 0){
						throw new Exception("BatSync_LocalNotExist should insert remote assets");
					}
					return NIL;
				});
				return NIL;
			}
			finally{
				await TryCleanupByHeadOwner(owner, token);
			}
		});
	}

	static JnWord MkRemote(IdUser owner, str head, str lang, str desc){
		var id = new IdWord();
		return new JnWord{
			Word = new PoWord{
				Id = id,
				Owner = owner,
				Head = head,
				Lang = lang,
			},
			Props = [
				new PoWordProp{
					Id = new IdWordProp(),
					WordId = id,
					KType = EKvType.Str,
					KStr = KeysProp.Inst.description,
					VType = EKvType.Str,
					VStr = desc,
				},
			],
			Learns = [
				new PoWordLearn{
					Id = new IdWordLearn(),
					WordId = id,
					LearnResult = ELearn.Add,
				},
			],
		};
	}
}
