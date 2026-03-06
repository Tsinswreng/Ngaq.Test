using Ngaq.Core.Shared.Word.Models;
using Ngaq.Core.Infra.IF;
using Ngaq.Core.Infra;
using Ngaq.Core.Model.Po.Kv;
using Ngaq.Core.Model.Po.Learn_;
using Ngaq.Core.Shared.User.Models.Po.User;
using Ngaq.Core.Shared.Word.Models.Po.Kv;
using Ngaq.Core.Shared.Word.Models.Po.Learn;
using Ngaq.Core.Shared.Word.Models.Po.Word;

namespace Ngaq.Test.Word;


public class TestJnWord{
	str Doc = 
$"""
- {nameof(JnWord)}
- {nameof(ExtnJnWord)}
""";

	static JnWord MkJnWord(
		UInt128 wordId
		,UInt128 ownerId
		,string head
		,string lang
		,long bizCreatedAt
		,long bizUpdatedAt
	){
		return new JnWord{
			Word = new PoWord{
				Id = new IdWord(wordId),
				Owner = new IdUser(ownerId),
				Head = head,
				Lang = lang,
				BizCreatedAt = Tempus.FromUnixMs(bizCreatedAt),
				BizUpdatedAt = Tempus.FromUnixMs(bizUpdatedAt),
			}
		};
	}

	static PoWordProp MkProp(
		UInt128 id
		,UInt128 wordId
		,long bizCreatedAt
		,long bizUpdatedAt
		,string? vStr = null
	){
		return new PoWordProp{
			Id = new IdWordProp(id),
			WordId = new IdWord(wordId),
			BizCreatedAt = Tempus.FromUnixMs(bizCreatedAt),
			BizUpdatedAt = Tempus.FromUnixMs(bizUpdatedAt),
			VStr = vStr,
		};
	}

	static PoWordLearn MkLearn(
		UInt128 id
		,UInt128 wordId
		,long bizCreatedAt
		,long bizUpdatedAt
	){
		return new PoWordLearn{
			Id = new IdWordLearn(id),
			WordId = new IdWord(wordId),
			BizCreatedAt = Tempus.FromUnixMs(bizCreatedAt),
			BizUpdatedAt = Tempus.FromUnixMs(bizUpdatedAt),
		};
	}

	[Fact]
	public void SetIdEtEnsureFKey_ShouldUpdateAllForeignKeys(){
		var word = MkJnWord(1, 11, "hello", "en", 1000, 1000);
		word.Props.Add(MkProp(201, 999, 1001, 1001, "old-prop"));
		word.Learns.Add(MkLearn(301, 999, 1001, 1001));

		var r = word.SetIdEtEnsureFKey(new IdWord((UInt128)777));

		Assert.Same(word, r);
		Assert.Equal(new IdWord((UInt128)777), word.Word.Id);
		Assert.All(word.Props, p => Assert.Equal(word.Word.Id, p.WordId));
		Assert.All(word.Learns, l => Assert.Equal(word.Word.Id, l.WordId));
	}

	[Fact]
	public void GroupByHeadOfSameLang_ShouldGroupCorrectly(){
		var w1 = MkJnWord(1, 11, "apple", "en", 1000, 1000);
		var w2 = MkJnWord(2, 11, "apple", "en", 1000, 1000);
		var w3 = MkJnWord(3, 11, "banana", "en", 1000, 1000);

		var grouped = new IJnWord[]{w1, w2, w3}.GroupByHeadOfSameLang();

		Assert.Equal(2, grouped.Count);
		Assert.Equal(2, grouped["apple"].Count);
		Assert.Single(grouped["banana"]);
	}

	[Fact]
	public void GroupByHeadOfSameLang_ShouldThrow_WhenLangNotSame(){
		var w1 = MkJnWord(1, 11, "hello", "en", 1000, 1000);
		var w2 = MkJnWord(2, 11, "hello", "zh", 1000, 1000);

		Assert.ThrowsAny<Exception>(() => new IJnWord[]{w1, w2}.GroupByHeadOfSameLang());
	}

	[Fact]
	public void IsSynced_ShouldReturnTrue_WhenCoreStateEqual(){
		var a = MkJnWord(1, 11, "hello", "en", 1000, 2000);
		var b = MkJnWord(2, 11, "hello", "en", 1000, 2000);
		a.Props.Add(MkProp(101, 1, 1001, 1001));
		b.Props.Add(MkProp(102, 2, 1002, 1002));
		a.Learns.Add(MkLearn(201, 1, 1001, 1001));
		b.Learns.Add(MkLearn(202, 2, 1002, 1002));

		Assert.True(a.IsSynced(b));
		Assert.True(b.IsSynced(a));
	}

	[Fact]
	public void DiffById_ShouldReturnItemsOnlyInListA(){
		var listA = new List<PoWordProp>{
			MkProp(1, 10, 1000, 1000),
			MkProp(2, 10, 1000, 1000),
			MkProp(3, 10, 1000, 1000),
		};
		var listB = new List<PoWordProp>{
			MkProp(2, 10, 1000, 1000),
		};

		var diff = listA.DiffById<PoWordProp, IdWordProp>(listB);

		Assert.Equal(2, diff.Count);
		Assert.Contains(diff, x => x.Id == new IdWordProp((UInt128)1));
		Assert.Contains(diff, x => x.Id == new IdWordProp((UInt128)3));
	}
}
