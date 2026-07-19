// using Ngaq.Ui.Views.Word.Learn;
// using Tsinswreng.CsTreeTest;

// namespace Ngaq.Ui.Test.Views.Word.Learn;

// public partial class TestIViewLearnWord{
// 	/// <summary>
// 	/// 註冊 `WordInfo` 屬性接口測試。
// 	/// </summary>
// 	/// <param name="Node">當前測試節點。</param>
// 	public void RegisterWordInfo(ITestNode Node){
// 		var register = Node.MkTestFnRegister(
// 			typeof(TestIViewLearnWord)
// 			,[typeof(IViewLearnWord)]
// 			,[nameof(IViewLearnWord.WordInfo)]
// 			,nameof(TestIViewLearnWord)
// 		);
// 		var R = register.Register;

// 		R("WordInfo_Should_BeReadable", async(o)=>{
// 			_ = ViewLearnWord.WordInfo;
// 			Assert.IsTrue(true);
// 			return null;
// 		});
// 	}
// }
