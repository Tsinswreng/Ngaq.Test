using Ngaq.Ui.Views.Word.Learn;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Ui.Test.Views.Word.Learn;

public partial class TestIViewLearnWord{
	public void RegisterClickStart(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestIViewLearnWord)
			,[typeof(IViewLearnWord)]
			,[nameof(IViewLearnWord.ClickStart)]
			,nameof(TestIViewLearnWord)
		);
		var R = register.Register;

		R("ClickStart_Should_NotThrow_And_KeepContractReachable", async(o)=>{
			await ViewLearnWord.ClickStart(default);
			_ = ViewLearnWord.WordInfo;
			_ = ViewLearnWord.WordListCards;
			Assert.IsTrue(true);
			return null;
		});
	}
}
