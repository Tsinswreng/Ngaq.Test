using Ngaq.Ui.Views.Word.Learn;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Ui.Test.Views.Word.Learn;

public partial class TestIViewLearnWord{
	/// <summary>
	/// 註冊 `ClickWordCard` 接口測試。
	/// </summary>
	/// <param name="Node">當前測試節點。</param>
	public void RegisterClickWordCard(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestIViewLearnWord)
			,[typeof(IViewLearnWord)]
			,[nameof(IViewLearnWord.ClickWordCard)]
			,nameof(TestIViewLearnWord)
		);
		var R = register.Register;

		R("ClickWordCard_Pos0_Should_NotThrow_WhenListPrepared", async(o)=>{
			ViewLearnWord.WordListCards = [new FakeWordListCard{
				IndexText = "0",
				HeadText = "ut_i_view_learn_word_click_card",
				LangText = "en",
			}];

			await ViewLearnWord.ClickWordCard(0, default);

			_ = ViewLearnWord.WordInfo;
			Assert.IsTrue(true);
			return null;
		});
	}
}
