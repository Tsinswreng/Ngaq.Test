using Ngaq.Ui.Views.Word.Learn;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Ui.Test.Views.Word.Learn;

public partial class TestIViewLearnWord{
	public void RegisterClickWordCard(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestIViewLearnWord)
			,[typeof(IViewLearnWord)]
			,[nameof(IViewLearnWord.ClickWordCard)]
			,nameof(TestIViewLearnWord)
		);
		var R = register.Register;

		R("ClickWordCard_Pos0_Should_NotThrow", async(o)=>{
			await UiTestTools.AssertNoUnhandledUiException(async ()=>{
				await ViewLearnWord.ClickWordCard(0, default);
			});
			_ = ViewLearnWord.WordInfo;
			Assert.IsTrue(true);
			return null;
		});
	}
}
