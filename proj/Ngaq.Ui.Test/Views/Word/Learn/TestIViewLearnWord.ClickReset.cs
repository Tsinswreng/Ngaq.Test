using Ngaq.Ui.Views.Word.Learn;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Ui.Test.Views.Word.Learn;

public partial class TestIViewLearnWord{
	public void RegisterClickReset(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestIViewLearnWord)
			,[typeof(IViewLearnWord)]
			,[nameof(IViewLearnWord.ClickReset)]
			,nameof(TestIViewLearnWord)
		);
		var R = register.Register;

		R("ClickReset_Should_NotThrow", async(o)=>{
			await ViewLearnWord.ClickReset(default);
			_ = ViewLearnWord.WordInfo;
			_ = ViewLearnWord.WordListCards;
			Assert.IsTrue(true);
			return null;
		});
	}
}
