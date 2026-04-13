using Ngaq.Core.Shared.Sync;
using Ngaq.Core.Infra;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Core.Test.Sync.IPacker;

public partial class TestIPacker{
	void RegisterPack(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestIPacker),
			[typeof(IPacker<SampleSyncObj>)],
			[]
		);
		var R = register.Register;
		register.TesteeFnNames = [nameof(IPacker<SampleSyncObj>.Pack)];

		R("Pack_Should_OutputTextWithStream_And_BeRoundTripReadable", async(o)=>{
			var info = new ObjPackInfo{
				PayloadTypeObj = "gzip+jsonl",
				CreatedAt = Tempus.Now(),
				ObjVer = new Version(1, 0),
			};
			var src = new[]{
				new SampleSyncObj{I = 1, S = "a"},
				new SampleSyncObj{I = 2, S = "b"},
			};

			var tws = Packer.Pack(AsyE(src), info, CT.None);
			if(string.IsNullOrWhiteSpace(tws.Text)){
				throw new Exception("Pack should output metadata text");
			}
			if(tws.Payload is null){
				throw new Exception("Pack should output payload stream");
			}

			var ans = Packer.Unpack(tws, CT.None);
			if(!ans.Ok || ans.Data is null){
				throw new Exception("Pack output should be readable by Unpack");
			}
			var got = await ToList(ans.Data);
			if(got.Count != 2 || got[0].I != 1 || got[1].S != "b"){
				throw new Exception("Pack should preserve object sequence for roundtrip");
			}
			return NIL;
		});
	}
}
