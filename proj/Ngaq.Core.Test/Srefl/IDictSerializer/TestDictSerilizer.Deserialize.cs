using Tsinswreng.CsTreeTest;
using Tsinswreng.CsCore;
using Ngaq.Core.Infra;
using Ngaq.Core.Shared.User.Models.Po.User;
using Ngaq.Core.Shared.Word.Models;
using Ngaq.Core.Shared.Word.Models.Learn_;
using Ngaq.Core.Shared.Word.Models.Po.Kv;
using Ngaq.Core.Shared.Word.Models.Po.Word;
using Tsinswreng.CsTempus;

namespace Tsinswreng.Srefl.Test.IDictSerializer;

public partial class TestIDictSerializer: ITester{

	private static Dictionary<str, obj?> MkWordDict(
		str? Id = null
		,str Head = "head-default"
		,str Lang = "en"
		,str? Owner = null
		,i64? StoredAtMs = null
		,i64? BizCreatedAtMs = null
		,i64? BizUpdatedAtMs = null
		,i64? DbCreatedAtMs = null
		,i64? DbUpdatedAtMs = null
	){
		return new Dictionary<str, obj?>{
			[nameof(PoWord.Id)] = Id,
			[nameof(PoWord.Head)] = Head,
			[nameof(PoWord.Lang)] = Lang,
			[nameof(PoWord.Owner)] = Owner,
			[nameof(PoWord.StoredAt)] = StoredAtMs,
			[nameof(PoWord.BizCreatedAt)] = BizCreatedAtMs,
			[nameof(PoWord.BizUpdatedAt)] = BizUpdatedAtMs,
			[nameof(PoWord.DbCreatedAt)] = DbCreatedAtMs,
			[nameof(PoWord.DbUpdatedAt)] = DbUpdatedAtMs,
		};
	}

	private static Dictionary<str, obj?> MkPropDict(
		str? WordId = null
		,obj? KType = null
		,obj? VType = null
	){
		return new Dictionary<str, obj?>{
			[nameof(PoWordProp.WordId)] = WordId,
			[nameof(PoWordProp.KType)] = KType ?? nameof(EKvType.Str),
			[nameof(PoWordProp.KStr)] = "mean",
			[nameof(PoWordProp.VType)] = VType ?? nameof(EKvType.Str),
			[nameof(PoWordProp.VStr)] = "value",
		};
	}

	private static Dictionary<str, obj?> MkLearnDict(
		str? WordId = null
		,obj? LearnResult = null
	){
		return new Dictionary<str, obj?>{
			[nameof(Ngaq.Core.Shared.Word.Models.Po.Learn.PoWordLearn.WordId)] = WordId,
			[nameof(Ngaq.Core.Shared.Word.Models.Po.Learn.PoWordLearn.LearnResult)] = LearnResult ?? nameof(ELearn.Rmb),
		};
	}
	
	public ITestNode RegisterDeserialize(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestIDictSerializer)
			,[typeof(TestIDictSerializer)]
			,[nameof(Deserialize)]
			,nameof(TestIDictSerializer) + "."
		);
		var R = register.Register;

		R("Deserialize_Should_ReturnNull_When_SourceIsNull", async(o)=>{
			var got = de(null, typeof(JnWord));
			if(got is not null){
				throw new Exception("Deserialize(null, targetType) should return null.");
			}
			return NIL;
		});

		R("Deserialize_Should_Map_Dict_To_JnWord_WithNestedLists", async(o)=>{
			var id = new IdWord();
			var idSerialized = id.ToString();
			var owner = new IdUser().ToString();
			var storedAtMs = Tempus.Now().Value;
			var bizCreatedAtMs = Tempus.Now().Value - 20;
			var bizUpdatedAtMs = Tempus.Now().Value - 10;
			var dbCreatedAtMs = Tempus.Now().Value - 30;
			var dbUpdatedAtMs = Tempus.Now().Value - 5;

			var src = new Dictionary<str, obj?>{
				[nameof(JnWord.Word)] = MkWordDict(
					idSerialized, "alpha", "en", owner,
					storedAtMs, bizCreatedAtMs, bizUpdatedAtMs, dbCreatedAtMs, dbUpdatedAtMs
				),
				[nameof(JnWord.Props)] = new List<obj?>{
					MkPropDict(idSerialized, nameof(EKvType.Str), nameof(EKvType.Str))
				},
				[nameof(JnWord.Learns)] = new List<obj?>{
					MkLearnDict(idSerialized, nameof(ELearn.Rmb))
				},
			};

			var got = de(src, typeof(JnWord));
			if(got is not JnWord word){
				throw new Exception("Deserialize(dict, typeof(JnWord)) should return JnWord instance.");
			}
			if(word.Word.Head != "alpha" || word.Word.Lang != "en"){
				throw new Exception("Word basic fields were not deserialized correctly.");
			}
			if(word.Word.Owner.ToString() != owner){
				throw new Exception("PoWord.Owner(IdUser) was not deserialized correctly from string.");
			}
			if(
				word.Word.StoredAt.Value != storedAtMs
				|| word.Word.BizCreatedAt.Value != bizCreatedAtMs
				|| word.Word.BizUpdatedAt.Value != bizUpdatedAtMs
				|| word.Word.DbCreatedAt.Value != dbCreatedAtMs
				|| word.Word.DbUpdatedAt.Value != dbUpdatedAtMs
			){
				throw new Exception("PoWord.Tempus fields were not deserialized correctly from unix-ms values.");
			}
			if(word.Props.Count != 1 || word.Learns.Count != 1){
				throw new Exception("Nested list fields Props/Learns should each contain one element.");
			}
			if(word.Props[0].KStr != "mean" || word.Props[0].VStr != "value"){
				throw new Exception("PoWordProp fields were not deserialized correctly.");
			}
			if(word.Learns[0].LearnResult != ELearn.Rmb){
				throw new Exception("PoWordLearn.LearnResult was not deserialized correctly.");
			}
			return NIL;
		});

		R("Deserialize_Should_Parse_IdWord_From_ToStringSerializedValue", async(o)=>{
			var expectedId = new IdWord();
			var serializedId = expectedId.ToString(); // IdWord 序列化後應該是 ToString() 的字符串

			var src = new Dictionary<str, obj?>{
				[nameof(JnWord.Word)] = MkWordDict(serializedId, "beta", "ja"),
				[nameof(JnWord.Props)] = new List<obj?>{ MkPropDict(serializedId) },
				[nameof(JnWord.Learns)] = new List<obj?>{ MkLearnDict(serializedId) },
			};

			var got = de(src, typeof(JnWord));
			if(got is not JnWord word){
				throw new Exception("Deserialize should return JnWord.");
			}

			if(word.Word.Id.ToString() != serializedId){
				throw new Exception("PoWord.Id should be restored from ToString() serialized IdWord string.");
			}
			if(word.Props[0].WordId.ToString() != serializedId){
				throw new Exception("PoWordProp.WordId should be restored from ToString() serialized IdWord string.");
			}
			if(word.Learns[0].WordId.ToString() != serializedId){
				throw new Exception("PoWordLearn.WordId should be restored from ToString() serialized IdWord string.");
			}
			return NIL;
		});

		R("Deserialize_Should_Parse_Enums_From_IntValue", async(o)=>{
			var idSerialized = new IdWord().ToString();
			var src = new Dictionary<str, obj?>{
				[nameof(JnWord.Word)] = MkWordDict(idSerialized, "gamma", "fr"),
				[nameof(JnWord.Props)] = new List<obj?>{
					MkPropDict(idSerialized, (int)EKvType.I64, (int)EKvType.F64)
				},
				[nameof(JnWord.Learns)] = new List<obj?>{
					MkLearnDict(idSerialized, (int)ELearn.Fgt)
				},
			};

			var got = de(src, typeof(JnWord));
			if(got is not JnWord word){
				throw new Exception("Deserialize should return JnWord.");
			}
			if(word.Props[0].KType != EKvType.I64 || word.Props[0].VType != EKvType.F64){
				throw new Exception("Enum EKvType should be parsed from integer values.");
			}
			if(word.Learns[0].LearnResult != ELearn.Fgt){
				throw new Exception("Enum ELearn should be parsed from integer value.");
			}
			return NIL;
		});

		R("Deserialize_Should_Ignore_UnknownKeys", async(o)=>{
			var idSerialized = new IdWord().ToString();
			var wordDict = MkWordDict(idSerialized, "delta", "de");
			wordDict["NoSuchWordProp"] = 12345;

			var src = new Dictionary<str, obj?>{
				[nameof(JnWord.Word)] = wordDict,
				[nameof(JnWord.Props)] = new List<obj?>{ MkPropDict(idSerialized) },
				[nameof(JnWord.Learns)] = new List<obj?>{ MkLearnDict(idSerialized) },
				["NoSuchTopProp"] = "ignored",
			};

			var got = de(src, typeof(JnWord));
			if(got is not JnWord word){
				throw new Exception("Deserialize should still succeed when unknown keys exist.");
			}
			if(word.Word.Head != "delta" || word.Word.Lang != "de"){
				throw new Exception("Known fields should still be deserialized when unknown keys exist.");
			}
			return NIL;
		});

		R("Deserialize_Should_ReturnSameInstance_When_SourceAlreadyAssignable", async(o)=>{
			var src = new JnWord();
			src.Word.Head = "self";
			src.Word.Lang = "en";

			var got = de(src, typeof(JnWord));
			if(!ReferenceEquals(src, got)){
				throw new Exception("When source is already assignable to target type, deserialize should return source instance.");
			}
			return NIL;
		});

		R("Deserialize_Should_Throw_When_SourceTypeIsUnsupported", async(o)=>{
			var unsupported = 123;
			var thrown = false;
			try{
				_ = de(unsupported, typeof(JnWord));
			}
			catch{
				thrown = true;
			}
			if(!thrown){
				throw new Exception("Deserialize should throw for unsupported source type -> JnWord.");
			}
			return NIL;
		});
		
		return Node;
	}
}
