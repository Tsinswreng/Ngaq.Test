using Ngaq.Core.Shared.Word.Models;
using Ngaq.Core.Shared.Word.Models.Po.Kv;
using Ngaq.Core.Shared.Word.Models.Po.Word;
using Tsinswreng.CsCore;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Core.Test.Tools.Json.IDictJsonSerializer;

public partial class TestIDictJsonSerializer{
	void RegisterToDictJson(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestIDictJsonSerializer),
			[typeof(Ngaq.Core.Tools.Json.IDictJsonSerializer)],
			[nameof(Ngaq.Core.Tools.Json.IDictJsonSerializer.ToDictJson)],
			nameof(TestIDictJsonSerializer) + ".ToDictJson."
		);
		var R = register.Register;

		R("ToDictJson_Should_Convert_JnWord_To_Nested_DictJson", async(o)=>{
			var src = MkSampleJnWord();
			var got = Ser.ToDictJson(src);
			if(got is not IDictionary<str, obj?> root){
				throw new Exception("ToDictJson(JnWord) should return IDictionary<string, object?>.");
			}

			if(root[nameof(JnWord.Word)] is not IDictionary<str, obj?> wordDict){
				throw new Exception("Root[Word] should be a nested dictionary.");
			}
			if((str?)wordDict[nameof(PoWord.Head)] != src.Word.Head){
				throw new Exception("PoWord.Head should be preserved in dict json.");
			}
			if((str?)wordDict[nameof(PoWord.Lang)] != src.Word.Lang){
				throw new Exception("PoWord.Lang should be preserved in dict json.");
			}

			if(root[nameof(JnWord.Props)] is not IList<obj?> props || props.Count != 1){
				throw new Exception("Root[Props] should be a list and contain one item.");
			}
			if(props[0] is not IDictionary<str, obj?> propDict){
				throw new Exception("Props[0] should be a dictionary.");
			}
			if((str?)propDict[nameof(PoWordProp.KStr)] != "mean"){
				throw new Exception("PoWordProp.KStr should be serialized to dict json.");
			}
			return NIL;
		});

		R("RoundTrip_Should_Keep_Core_Fields_For_JnWord", async(o)=>{
			var src = MkSampleJnWord(head: "delta", lang: "de");
			var dict = Ser.ToDictJson(src);
			var got = Ser.FromDictJson<JnWord>(dict);
			if(got is null){
				throw new Exception("Round-trip result should not be null.");
			}
			if(got.Word.Id.ToString() != src.Word.Id.ToString()){
				throw new Exception("PoWord.Id should be preserved after round-trip.");
			}
			if(got.Word.Head != src.Word.Head || got.Word.Lang != src.Word.Lang){
				throw new Exception("PoWord.Head/Lang should be preserved after round-trip.");
			}
			if(got.Props[0].KStr != src.Props[0].KStr || got.Props[0].VStr != src.Props[0].VStr){
				throw new Exception("PoWordProp key/value should be preserved after round-trip.");
			}
			return NIL;
		});
	}
}
