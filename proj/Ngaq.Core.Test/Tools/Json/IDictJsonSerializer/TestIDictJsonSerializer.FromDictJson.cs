using Ngaq.Core.Shared.Word.Models;
using Ngaq.Core.Shared.Word.Models.Learn_;
using Tsinswreng.CsCore;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Core.Test.Tools.Json.IDictJsonSerializer;

public partial class TestIDictJsonSerializer{
	void RegisterFromDictJson(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestIDictJsonSerializer),
			[typeof(Ngaq.Core.Tools.Json.IDictJsonSerializer)],
			[nameof(Ngaq.Core.Tools.Json.IDictJsonSerializer.FromDictJson)],
			nameof(TestIDictJsonSerializer) + ".FromDictJson."
		);
		var R = register.Register;

		R("FromDictJson_Should_Convert_Nested_DictJson_To_JnWord", async(o)=>{
			var dict = MkSampleJnWordDict(head: "gamma", lang: "fr");
			var got = Ser.FromDictJson<JnWord>(dict);
			if(got is null){
				throw new Exception("FromDictJson<JnWord>(dict) should not return null.");
			}
			if(got.Word.Head != "gamma" || got.Word.Lang != "fr"){
				throw new Exception("Word basic fields were not restored from dict json.");
			}
			if(got.Props.Count != 1 || got.Learns.Count != 1){
				throw new Exception("Nested list fields Props/Learns were not restored.");
			}
			if(got.Learns[0].LearnResult != ELearn.Rmb){
				throw new Exception("Enum field ELearn should be restored from dict json.");
			}
			return NIL;
		});

		R("FromDictJson_Should_Return_Null_When_Source_Is_Null", async(o)=>{
			var got = Ser.FromDictJson<JnWord>(null);
			if(got is not null){
				throw new Exception("FromDictJson(null) should return null.");
			}
			return NIL;
		});

		R("FromDictJson_Should_Throw_When_Source_Is_Unsupported", async(o)=>{
			var thrown = false;
			try{
				_ = Ser.FromDictJson<JnWord>(123);
			}catch(NotSupportedException){
				thrown = true;
			}
			if(!thrown){
				throw new Exception("FromDictJson should throw NotSupportedException for unsupported source.");
			}
			return NIL;
		});
	}
}
