using Ngaq.Core.Infra;

namespace Ngaq.Test;

public class TestTypeHelper{
	public void Test(){
		var LInt = new List<int>();
		var LString = new List<string>();
		var DString_Int = new Dictionary<string, int>();
		var Obj = new Object();
		{
			var kv = ListDictOfT.GetKvOfIDict(DString_Int.GetType());
			Assert.NotNull(kv);
			Assert.Equal(typeof(string), kv.Value.key);
			Assert.Equal(typeof(int), kv.Value.value);
		}
		{
			var t = ListDictOfT.GetTOfIList(LInt.GetType());
			Assert.NotNull(t);
			Assert.Equal(typeof(int), t);
		}
		{
			var t = ListDictOfT.GetTOfIList(LString.GetType());
			Assert.NotNull(t);
			Assert.Equal(typeof(string), t);
		}
		{
			var t = ListDictOfT.GetTOfIList(Obj.GetType());
			Assert.Null(t);
		}

	}
}
