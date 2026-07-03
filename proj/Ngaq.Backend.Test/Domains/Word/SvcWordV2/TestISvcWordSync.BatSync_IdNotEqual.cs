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
	void RegisterSyncIdNotEqual(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestISvcWordV2),
			[typeof(ISvcWordSync)],
			[]
		);
		var R = register.Register;
		register.TesteeFnNames = [nameof(ISvcWordSync.OrdSync_IdNotEqual)];

		R("BatSync_IdNotEqual_Should_KeepSingleBizIdWord_AndSyncAssets", async(o)=>{
			if(SvcWordV2 is not ISvcWordSync sync){
				throw new Exception("ISvcWordV2 implementation should also implement ISvcWordSync");
			}
			var owner = new IdUser();
			var token = "ut_wsync_idne_" + Guid.NewGuid().ToString("N");
			var local = new PoWord{
				Id = new IdWord(),
				Owner = owner,
				Head = token + "_h1",
				Lang = "en",
			};
			var remote = MkRemoteDiffId(owner, local.Head, local.Lang, token + "_d1");
			try{
				await RunNoTxn(async(Ctx)=>{
					await RepoWord.OrdAdd(Ctx, AsyE(local), CT.None);
					return NIL;
				});

				var dto = new DtoJnWordSyncResult{
					DiffResult = EDiffByBizIdResultForSync.IdNotEqual,
					Local = new JnWord{Word = local},
					Remote = remote,
					SyncedRoot = remote,
				};
				await sync.OrdSync_IdNotEqual(MkUserCtx(owner), AsyE(dto), CT.None);

				await RunNoTxn(async(Ctx)=>{
					var words = (await ToList(RepoWord.GetAll(Ctx, CT.None)))
						.Where(x=>x.Owner == owner && x.Head == local.Head && x.Lang == local.Lang)
						.ToList();
					if(words.Count != 1){
						throw new Exception("BatSync_IdNotEqual should keep exactly one root for same biz-id");
					}

					var wid = words[0].Id;
					var props = (await ToList(RepoProp.GetAll(Ctx, CT.None))).Where(x=>x.WordId == wid).ToList();
					var learns = (await ToList(RepoLearn.GetAll(Ctx, CT.None))).Where(x=>x.WordId == wid).ToList();
					if(props.Count == 0 || learns.Count == 0){
						throw new Exception("BatSync_IdNotEqual should sync remote assets to final root id");
					}
					return NIL;
				});
				return NIL;
			}
			finally{
				await RunNoTxn(async(Ctx)=>{
					await RepoProp.OrdHardDelById(Ctx, AsyE(remote.Props.Select(x=>x.Id).ToArray()), CT.None);
					await RepoLearn.OrdHardDelById(Ctx, AsyE(remote.Learns.Select(x=>x.Id).ToArray()), CT.None);
					await RepoWord.OrdHardDelById(Ctx, AsyE(local.Id, remote.Id), CT.None);
					return NIL;
				});
			}
		});
	}

	static JnWord MkRemoteDiffId(IdUser owner, str head, str lang, str desc){
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
