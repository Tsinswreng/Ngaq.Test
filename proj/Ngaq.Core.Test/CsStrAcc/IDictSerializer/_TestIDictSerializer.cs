using Tsinswreng.CsTreeTest;
using Tsinswreng.CsCore;

namespace Tsinswreng.CsStrAcc.Test.IDictSerializer;

public partial class TestIDictSerializer: ITester{
	
	[Doc(@$"
	#Params([src obj],[target type], [target obj])
	")]
	public Func<obj?, Type, obj?> Deserialize = null!;
	Func<obj?, Type, obj?> de=>Deserialize;
	public ITestNode RegisterTestsInto(ITestNode? Node){
		Node??=new TestNode();
		RegisterDeserialize(Node);

		return Node;
	}
}
