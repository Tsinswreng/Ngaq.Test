using Ngaq.Core.Shared.Word.Models;
using Ngaq.Core.Shared.Word.Models.Learn_;
using Ngaq.Core.Tools.Json;
using Tsinswreng.CsCore;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Core.Test.Tools.Json.AppJsonSerializerTests;

public partial class TestAppJsonSerializer{
	void RegisterParse(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestAppJsonSerializer),
			[typeof(IJsonSerializer)],
			[nameof(IJsonSerializer.Parse)],
			nameof(TestAppJsonSerializer) + ".Parse."
		);
		var R = register.Register;

		R("Parse_Should_RoundTrip_JnWord_And_Keep_FirstJsonEqualAfterReserialize", async(o)=>{
			var src = MkSampleJnWord(ELearn.Rmb);
			var json1 = JsonSerializer.Stringify(src);

			var got = JsonSerializer.Parse<JnWord>(json1);
			if(got is null){
				throw new Exception("IJsonSerializer.Parse<JnWord> should not return null.");
			}

			if(got.Word.Id.ToString() != src.Word.Id.ToString()){
				throw new Exception("PoWord.Id should be preserved after deserialize.");
			}
			if(got.Learns.Count != 1 || got.Learns[0].LearnResult != ELearn.Rmb){
				throw new Exception("Enum field should be preserved after deserialize.");
			}

			var json2 = JsonSerializer.Stringify(got);
			if(json2 != json1){
				throw new Exception("Reserialized json should equal first serialized json.");
			}
			return NIL;
		});

		R("Parse_By_Type_Should_RoundTrip_JnWord_And_Keep_FirstJsonEqualAfterReserialize", async(o)=>{
			var src = MkSampleJnWord(ELearn.Fgt);
			var json1 = JsonSerializer.Stringify(src);

			var parsed = JsonSerializer.Parse(json1, typeof(JnWord));
			if(parsed is not JnWord got){
				throw new Exception("IJsonSerializer.Parse(json, typeof(JnWord)) should return JnWord.");
			}
			if(got.Learns.Count != 1 || got.Learns[0].LearnResult != ELearn.Fgt){
				throw new Exception("Parse(Type) should restore enum field correctly.");
			}

			var json2 = JsonSerializer.Stringify(got);
			if(json2 != json1){
				throw new Exception("Reserialized json should equal first serialized json for Parse(Type).");
			}
			return NIL;
		});
	}
}
