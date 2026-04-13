using Ngaq.Core.Shared.Sync;
using Ngaq.Core.Infra;
using Tsinswreng.CsTreeTest;
using Tsinswreng.CsTextWithBlob;

namespace Ngaq.Core.Test.Sync.IPacker;

public partial class TestIPacker{
	void RegisterUnpack(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestIPacker),
			[typeof(IPacker<SampleSyncObj>)],
			[]
		);
		var R = register.Register;
		register.TesteeFnNames = [nameof(IPacker<SampleSyncObj>.Unpack)];

		R("Unpack_Should_ParseValidPackedData", async(o)=>{
			var info = new ObjPackInfo{
				PayloadTypeObj = "gzip+jsonl",
				CreatedAt = Tempus.Now(),
				ObjVer = new Version(1, 0),
			};
			var src = new[]{
				new SampleSyncObj{I = 10, S = "x"},
				new SampleSyncObj{I = 20, S = "y"},
			};
			var packed = Packer.Pack(AsyE(src), info, CT.None);
			var ans = Packer.Unpack(packed, CT.None);
			if(!ans.Ok || ans.Data is null){
				throw new Exception("Unpack should succeed for valid packed content");
			}
			var got = await ToList(ans.Data);
			if(got.Count != 2 || got[0].I != 10 || got[1].S != "y"){
				throw new Exception("Unpack should parse all objects from packed content");
			}
			return NIL;
		});

		R("Unpack_WhenMetaJsonInvalid_Should_ReturnNotOk", async(o)=>{
			var bad = TextWithStream.PackUtf8("not-json", new MemoryStream([1,2,3,4]));
			var ans = Packer.Unpack(bad, CT.None);
			if(ans.Ok){
				throw new Exception("Unpack should fail when metadata text is invalid json");
			}
			return NIL;
		});
	}
}
