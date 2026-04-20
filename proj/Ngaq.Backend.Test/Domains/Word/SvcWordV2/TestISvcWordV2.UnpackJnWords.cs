using Ngaq.Core.Shared.Sync;
using Ngaq.Core.Shared.User.Models.Po.User;
using Ngaq.Core.Shared.Word.Models;
using Ngaq.Core.Shared.Word.Models.Po.Word;
using Ngaq.Core.Shared.Word.Svc;
using Ngaq.Core.Tools.Json;
using Tsinswreng.CsTools;
using Tsinswreng.CsTextWithBlob;
using Tsinswreng.CsTreeTest;
using Tsinswreng.CsTempus;

namespace Ngaq.Backend.Test.Domains.Word;

public partial class TestISvcWordV2{
	void RegisterUnpackJnWords(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestISvcWordV2),
			[typeof(ISvcWordV2)],
			[]
		);
		var R = register.Register;
		register.TesteeFnNames = [nameof(ISvcWordV2.UnpackJnWords)];

		R("UnpackJnWords_Should_RestorePackedWords", async(o)=>{
			var owner = new IdUser();
			var token = "ut_wv2_unpack_" + Guid.NewGuid().ToString("N");
			var w1 = MkSyncInput(owner, token + "_h1", "en", token + "_d1");
			var w2 = MkSyncInput(owner, token + "_h2", "fr", token + "_d2");

			var packer = new Packer<JnWord>{JsonS = AppJsonSerializer.Inst};
			var packInfo = new ObjPackInfo{
				PayloadTypeObj = nameof(GZipLinesUtf8),
				CreatedAt = Tempus.Now(),
			};
			using var stream = packer.Pack(AsyE(w1, w2), packInfo, CT.None).ToStream();

			var got = await ToList(SvcWordV2.UnpackJnWords(stream, CT.None));
			if(got.Count != 2){
				throw new Exception("UnpackJnWords should return all packed words.");
			}
			var gotHeads = got.Select(x=>x.Word.Head).ToHashSet();
			if(!gotHeads.Contains(w1.Word.Head) || !gotHeads.Contains(w2.Word.Head)){
				throw new Exception("UnpackJnWords should preserve word heads.");
			}
			return NIL;
		});
	}
}
