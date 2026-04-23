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

namespace Ngaq.Core.Test.Tools.Json.IDictJsonSerializer;

public partial class TestIDictJsonSerializer: ITester{
	private static AppJsonSerializer Ser => AppJsonSerializer.Inst;

	private static JnWord MkSampleJnWord(
		str head = "alpha",
		str lang = "en"
	){
		var id = new IdWord();
		var owner = new IdUser();
		var now = UnixMs.Now().Value;
		return new JnWord{
			Word = new PoWord{
				Id = id,
				Owner = owner,
				Head = head,
				Lang = lang,
				StoredAt = UnixMs.FromUnixMs(now),
				BizCreatedAt = UnixMs.FromUnixMs(now - 200),
				BizUpdatedAt = UnixMs.FromUnixMs(now - 100),
				DbCreatedAt = UnixMs.FromUnixMs(now - 300),
				DbUpdatedAt = UnixMs.FromUnixMs(now - 50),
			},
			Props = [
				new PoWordProp{
					WordId = id,
					KType = EKvType.Str,
					KStr = "mean",
					VType = EKvType.Str,
					VStr = "value",
				}
			],
			Learns = [
				new PoWordLearn{
					WordId = id,
					LearnResult = ELearn.Rmb,
				}
			]
		};
	}

	private static Dictionary<str, obj?> MkSampleJnWordDict(
		str? idSerialized = null,
		str head = "beta",
		str lang = "ja"
	){
		idSerialized ??= new IdWord().ToString();
		var owner = new IdUser().ToString();
		var now = UnixMs.Now().Value;

		return new Dictionary<str, obj?>{
			[nameof(JnWord.Word)] = new Dictionary<str, obj?>{
				[nameof(PoWord.Id)] = idSerialized,
				[nameof(PoWord.Owner)] = owner,
				[nameof(PoWord.Head)] = head,
				[nameof(PoWord.Lang)] = lang,
				[nameof(PoWord.StoredAt)] = now,
				[nameof(PoWord.BizCreatedAt)] = now - 20,
				[nameof(PoWord.BizUpdatedAt)] = now - 10,
				[nameof(PoWord.DbCreatedAt)] = now - 30,
				[nameof(PoWord.DbUpdatedAt)] = now - 5,
			},
			[nameof(JnWord.Props)] = new List<obj?>{
				new Dictionary<str, obj?>{
					[nameof(PoWordProp.WordId)] = idSerialized,
					[nameof(PoWordProp.KType)] = nameof(EKvType.Str),
					[nameof(PoWordProp.KStr)] = "mean",
					[nameof(PoWordProp.VType)] = nameof(EKvType.Str),
					[nameof(PoWordProp.VStr)] = "value",
				}
			},
			[nameof(JnWord.Learns)] = new List<obj?>{
				new Dictionary<str, obj?>{
					[nameof(PoWordLearn.WordId)] = idSerialized,
					[nameof(PoWordLearn.LearnResult)] = nameof(ELearn.Rmb),
				}
			},
		};
	}

	public ITestNode RegisterTestsInto(ITestNode? Node){
		Node ??= new TestNode();
		RegisterToDictJson(Node);
		RegisterFromDictJson(Node);
		return Node;
	}
}
