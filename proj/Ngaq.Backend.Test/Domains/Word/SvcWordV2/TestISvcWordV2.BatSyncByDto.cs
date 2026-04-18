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
	void RegisterBatSyncByDto(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestISvcWordV2),
			[typeof(ISvcWordV2)],
			[]
		);
		var R = register.Register;
		register.TesteeFnNames = [nameof(ISvcWordV2.BatSyncByDto)];

		R("BatSyncByDto_Should_Handle_AllDiffCategoriesInOneBatch", async(o)=>{
			var owner = new IdUser();
			var token = "ut_wv2_syncdto_" + Guid.NewGuid().ToString("N");
			var noChangeRoot = new PoWord{Id = new IdWord(), Owner = owner, Head = token + "_nochange", Lang = "en"};
			var remoteOlderRoot = new PoWord{Id = new IdWord(), Owner = owner, Head = token + "_remoteolder", Lang = "en"};
			var idOldRoot = new PoWord{Id = new IdWord(), Owner = owner, Head = token + "_idnotequal", Lang = "en"};
			try{
				await RunNoTxn(async(Ctx)=>{
					await RepoWord.BatAdd(Ctx, AsyE(noChangeRoot, remoteOlderRoot, idOldRoot), CT.None);
					return NIL;
				});

				var localNotExistRemote = MkSyncJnWord(owner, token + "_localnotexist", "en", token + "_d1");
				var idNotEqualRemote = MkSyncJnWord(owner, idOldRoot.Head, idOldRoot.Lang, token + "_idne_d1");

				var dtoNoChange = new DtoJnWordSyncResult{
					DiffResult = EDiffByBizIdResultForSync.NoChange,
					Local = new JnWord{Word = noChangeRoot},
					Remote = new JnWord{Word = noChangeRoot},
				};
				var dtoRemoteOlder = new DtoJnWordSyncResult{
					DiffResult = EDiffByBizIdResultForSync.RemoteIsOlder,
					Local = new JnWord{Word = remoteOlderRoot},
					Remote = new JnWord{Word = new PoWord{
						Id = remoteOlderRoot.Id,
						Owner = owner,
						Head = remoteOlderRoot.Head,
						Lang = remoteOlderRoot.Lang,
					}},
				};
				var dtoLocalNotExist = new DtoJnWordSyncResult{
					DiffResult = EDiffByBizIdResultForSync.LocalNotExist,
					Remote = localNotExistRemote,
					SyncedPoWord = localNotExistRemote,
				};
				var dtoIdNotEqual = new DtoJnWordSyncResult{
					DiffResult = EDiffByBizIdResultForSync.IdNotEqual,
					Local = new JnWord{Word = idOldRoot},
					Remote = idNotEqualRemote,
					SyncedPoWord = idNotEqualRemote,
				};

				await SvcWordV2.BatSyncByDto(
					MkUserCtx(owner),
					AsyE(dtoNoChange, dtoRemoteOlder, dtoLocalNotExist, dtoIdNotEqual),
					CT.None
				);

				await RunNoTxn(async(Ctx)=>{
					var words = (await ToList(RepoWord.GetAll(Ctx, CT.None)))
						.Where(x=>x.Owner == owner && x.Head.StartsWith(token))
						.ToList();
					if(!words.Any(x=>x.Head == noChangeRoot.Head)){
						throw new Exception("BatSyncByDto NoChange case should keep local word");
					}
					if(!words.Any(x=>x.Head == remoteOlderRoot.Head)){
						throw new Exception("BatSyncByDto RemoteIsOlder case should keep local word");
					}
					if(!words.Any(x=>x.Head == localNotExistRemote.Head)){
						throw new Exception("BatSyncByDto LocalNotExist case should insert remote word");
					}
					var idNeWords = words.Where(x=>x.Head == idOldRoot.Head && x.Lang == idOldRoot.Lang).ToList();
					if(idNeWords.Count != 1){
						throw new Exception("BatSyncByDto IdNotEqual case should leave one merged biz-id row");
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

	static JnWord MkSyncJnWord(IdUser owner, str head, str lang, str desc){
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
