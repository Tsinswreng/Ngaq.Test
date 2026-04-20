using Ngaq.Core.Shared.Word.Models;
using Ngaq.Core.Shared.Word.Models.Learn_;
using Ngaq.Core.Shared.Word.Models.Po.Learn;
using Ngaq.Core.Shared.Word.Models.Po.Word;
using Ngaq.Core.Tools.Json;
using Tsinswreng.CsCore;
using Tsinswreng.CsTreeTest;
using Tsinswreng.CsTools;

namespace Ngaq.Core.Test.Tools.Json.AppJsonSerializerTests;

public partial class TestAppJsonSerializer{
	void RegisterStringify(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestAppJsonSerializer),
			[typeof(IJsonSerializer)],
			[nameof(IJsonSerializer.Stringify)],
			nameof(TestAppJsonSerializer) + ".Stringify."
		);
		var R = register.Register;

		R("Stringify_Should_Map_Enum_And_StrongTypes_As_Scalar_Strings", async(o)=>{
			var src = MkSampleJnWord(ELearn.Add);
			var json = JsonSerializer.Stringify(src);
			var root = MustParseJsonToDict(json);

			var wordId = root.GetValueByPath([nameof(JnWord.Word), nameof(PoWord.Id)]);
			if(wordId is not str idStr || idStr != src.Word.Id.ToString()){
				throw new Exception("PoWord.Id should be serialized as scalar string.");
			}

			var learnsObj = root.GetValueByPath([nameof(JnWord.Learns)]);
			if(learnsObj is not IList<obj?> learns || learns.Count == 0){
				throw new Exception("JnWord.Learns should be serialized as non-empty list.");
			}
			if(learns[0] is not IDictionary<str, obj?> firstLearn){
				throw new Exception("JnWord.Learns[0] should be dictionary.");
			}

			var learnResult = firstLearn.GetValueByPath([nameof(PoWordLearn.LearnResult)]);
			if(learnResult is not str learnResultStr || learnResultStr != nameof(ELearn.Add)){
				throw new Exception("PoWordLearn.LearnResult should be serialized as enum name string.");
			}

			var learnWordId = firstLearn.GetValueByPath([nameof(PoWordLearn.WordId)]);
			if(learnWordId is not str learnWordIdStr || learnWordIdStr != src.Word.Id.ToString()){
				throw new Exception("PoWordLearn.WordId should be serialized as scalar string.");
			}

			var bizCreatedAt = firstLearn.GetValueByPath([nameof(PoWordLearn.BizCreatedAt)]);
			if(bizCreatedAt is not i64 tempusStr || tempusStr != 1776575044094){
				throw new Exception("bizCreatedAt is not i64 tempusStr || tempusStr != 1776575044094");
			}

			return NIL;
		});
	}
}
