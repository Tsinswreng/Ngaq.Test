using Ngaq.Core.Shared.Sync;
using Ngaq.Core.Shared.Word.Models;
using Ngaq.Core.Shared.Word.Models.Po.Word;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Core.Test.Sync.IPacker;

public partial class TestIPacker: ITester{
	readonly IPacker<JnWord> Packer = new Packer<JnWord>();

	public ITestNode RegisterTestsInto(ITestNode? Node){
		Node ??= new TestNode();
		RegisterPack(Node);
		RegisterUnpack(Node);
		return Node;
	}

	static async IAsyncEnumerable<T> AsyE<T>(params T[] Items){
		foreach(var I in Items){
			yield return I;
		}
	}

	static async Task<List<T>> ToList<T>(IAsyncEnumerable<T>? Asy){
		if(Asy is null){
			return [];
		}
		var R = new List<T>();
		await foreach(var x in Asy){
			R.Add(x);
		}
		return R;
	}

	static JnWord MkJnWord(str Head, str Lang){
		return new JnWord{
			Word = new PoWord{
				Id = new IdWord(),
				Head = Head,
				Lang = Lang,
			},
		};
	}
}
