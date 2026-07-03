using Ngaq.Core.Shared.Sync;
using Ngaq.Core.Shared.User.Models.Po.User;
using Ngaq.Core.Shared.Word.Models;
using Ngaq.Core.Shared.Word.Models.Dto;
using Ngaq.Core.Shared.Word.Models.Po.Word;
using Ngaq.Core.Shared.Word.Svc;
using Ngaq.Core.Tools.Json;
using Tsinswreng.CsTools;
using Tsinswreng.CsTextWithBlob;
using Tsinswreng.CsTreeTest;
using Tsinswreng.CsTempus;

namespace Ngaq.Backend.Test.Domains.Word;

public partial class TestISvcWordV2{
	void RegisterBizSyncJnWordByBizIdFromStream(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestISvcWordV2),
			[typeof(ISvcWordV2)],
			[]
		);
		var R = register.Register;
		register.TesteeFnNames = [nameof(ISvcWordV2.OrdSyncJnWordByBizIdFromStream)];

		R("BatSyncJnWordByBizIdFromStream_Should_InsertRemoteWords", async(o)=>{
			var owner = new IdUser();
			var token = "ut_wv2_sync_stream_" + Guid.NewGuid().ToString("N");
			var remote1 = MkSyncInput(owner, token + "_h1", "en", token + "_d1");
			var remote2 = MkSyncInput(owner, token + "_h2", "en", token + "_d2");

			var packer = new Packer<JnWord>{JsonS = AppJsonSerializer.Inst};
			var packInfo = new ObjPackInfo{
				PayloadTypeObj = nameof(GZipLinesUtf8),
				CreatedAt = UnixMs.Now(),
			};

			try{
				using var stream = packer.Pack(AsyE(remote1, remote2), packInfo, CT.None).ToStream();
				var dtos = await ToList(SvcWordV2.OrdSyncJnWordByBizIdFromStream(MkUserCtx(owner), stream, CT.None));
				Assert.IsTrue(dtos.Count == 2, "BatSyncJnWordByBizIdFromStream should return one dto per remote word.");
				Assert.IsTrue(dtos.All(x => x.DiffResult == EDiffByBizIdResultForSync.LocalNotExist), "BatSyncJnWordByBizIdFromStream should mark fresh remotes as LocalNotExist.");

				await RunNoTxn(async(Ctx)=>{
					var words = (await ToList(RepoWord.GetAll(Ctx, CT.None)))
						.Where(x=>x.Owner == owner && x.Head.StartsWith(token))
						.ToList();
					Assert.IsTrue(words.Count == 2, "BatSyncJnWordByBizIdFromStream should insert all streamed words.");
					return NIL;
				});
				return NIL;
			}
			finally{
				await TryCleanupByHeadOwner(owner, token);
			}
		});
	}
}
