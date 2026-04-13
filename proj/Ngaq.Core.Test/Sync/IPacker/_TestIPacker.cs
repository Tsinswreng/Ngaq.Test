using Ngaq.Core.Shared.Sync;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Core.Test.Sync.IPacker;

public partial class TestIPacker: ITester{
	readonly IPacker<SampleSyncObj> Packer = new Packer<SampleSyncObj>();

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

	public class SampleSyncObj{
		public i32 I {get;set;}
		public str? S {get;set;}
	}
}

