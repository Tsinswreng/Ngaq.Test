using Ngaq.Core.Infra;
using Ngaq.Core.Shared.User.Models.Po.User;
using Ngaq.Core.Shared.Word.Models;
using Ngaq.Core.Shared.Word.Models.Learn_;
using Ngaq.Core.Shared.Word.Models.Po.Kv;
using Ngaq.Core.Shared.Word.Models.Po.Learn;
using Ngaq.Core.Shared.Word.Models.Po.Word;
using Ngaq.Core.Tools.Json;
using Tsinswreng.CsTempus;
using Tsinswreng.CsTreeTest;
using Tsinswreng.CsTools;

namespace Ngaq.Core.Test.Tools.Json.AppJsonSerializerTests;

public partial class TestAppJsonSerializer: ITester{
	private readonly IJsonSerializer JsonSerializer;

	public TestAppJsonSerializer(IJsonSerializer jsonSerializer){
		JsonSerializer = jsonSerializer;
	}

	private static JnWord MkSampleJnWord(
		ELearn learnResult = ELearn.Add
	){
		var wordId = new IdWord();
		var owner = new IdUser();
		const i64 t = 1776575044094;
		return new JnWord{
			Word = new PoWord{
				Id = wordId,
				Owner = owner,
				Head = "alpha",
				Lang = "en",
				StoredAt = Tempus.FromUnixMs(t + 1),
				BizCreatedAt = Tempus.FromUnixMs(t + 2),
				BizUpdatedAt = Tempus.FromUnixMs(t + 3),
				DbCreatedAt = Tempus.FromUnixMs(t + 4),
				DbUpdatedAt = Tempus.FromUnixMs(t + 5),
			},
			Props = [
				new PoWordProp{
					WordId = wordId,
					KType = EKvType.Str,
					KStr = "mean",
					VType = EKvType.Str,
					VStr = "value",
					BizCreatedAt = Tempus.FromUnixMs(t + 10),
				}
			],
			Learns = [
				new PoWordLearn{
					WordId = wordId,
					LearnResult = learnResult,
					BizCreatedAt = Tempus.FromUnixMs(t),
					BizUpdatedAt = Tempus.FromUnixMs(t + 20),
				}
			]
		};
	}

	private static IDictionary<str, obj?> MustParseJsonToDict(str json){
		var dict = ToolJson.JsonStrToDict(json);
		if(dict is null){
			throw new Exception("ToolJson.JsonStrToDict should not return null for valid json object.");
		}
		return dict;
	}

	public ITestNode RegisterTestsInto(ITestNode? Node){
		Node ??= new TestNode();
		RegisterStringify(Node);
		RegisterParse(Node);
		return Node;
	}
}
