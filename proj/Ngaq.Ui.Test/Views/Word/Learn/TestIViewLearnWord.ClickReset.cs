using Ngaq.Ui.Views.Word.Learn;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Ui.Test.Views.Word.Learn;

public partial class TestIViewLearnWord{
	/// <summary>
	/// 註冊 `ClickReset` 接口測試。
	/// </summary>
	/// <param name="Node">當前測試節點。</param>
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
