using Ngaq.Ui.Views.Word.Learn;
using Ngaq.Ui.Views.Word.WordCard;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Ui.Test.Views.Word.Learn;

public partial class TestIViewLearnWord{
	/// <summary>
	/// 註冊 `WordListCards` 屬性接口測試。
	/// </summary>
	/// <param name="Node">當前測試節點。</param>
	public void RegisterWordListCards(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestIViewLearnWord)
			,[typeof(IViewLearnWord)]
			,[nameof(IViewLearnWord.WordListCards)]
			,nameof(TestIViewLearnWord)
		);
		var R = register.Register;

		R("WordListCards_Should_Support_Assign_And_Read", async(o)=>{
			IList<IViewWordListCard> expected = [
				new FakeWordListCard{
					IndexText = "0",
					HeadText = "ut_i_view_learn_word_list_0",
					LangText = "en",
				},
				new FakeWordListCard{
					IndexText = "1",
					HeadText = "ut_i_view_learn_word_list_1",
					LangText = "ja",
				},
			];

			ViewLearnWord.WordListCards = expected;

			Assert.IsTrue(object.ReferenceEquals(expected, ViewLearnWord.WordListCards));
			Assert.IsTrue(ViewLearnWord.WordListCards?.Count == 2);
			return null;
		});
	}
}
