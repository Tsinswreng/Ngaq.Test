// using Ngaq.Ui.Views.Word.PoWordEdit;
// using Ngaq.Ui.Views.Word.WordEditV2;
// using Tsinswreng.CsTreeTest;

// namespace Ngaq.Ui.Test.Views.Word.WordEditV2;

// public partial class TestIViewWordEditV2{
// 	public void RegisterPoWordEdit(ITestNode Node){
// 		var register = Node.MkTestFnRegister(
// 			typeof(TestIViewWordEditV2),
// 			[typeof(IViewWordEditV2)],
// 			[nameof(IViewWordEditV2.PoWordEdit)],
// 			nameof(TestIViewWordEditV2)
// 		);
// 		var R = register.Register;

// 		R("PoWordEdit_Should_Expose_Editable_Fields_After_FreeDraftInitialized", async(o)=>{
// 			var concrete = ViewWordEditV2 as ViewWordEditV2;
// 			Assert.IsTrue(concrete is not null, "Current test host should provide concrete ViewWordEditV2.");

// 			var token = "ut_ui_word_edit_prop_" + Guid.NewGuid().ToString("N");
// 			await UiTestTools.RunOnUiAsync(()=>{
// 				concrete!.Ctx?.InitFreeAddDraft("en");
// 				return NIL;
// 			});

// 			var po = await UiTestTools.RunOnUiAsync(()=>ViewWordEditV2.PoWordEdit);
// 			Assert.IsTrue(po is not null, "WordEditV2 should expose PoWordEdit sub view.");

// 			await UiTestTools.RunOnUiAsync(()=>{
// 				po!.Head = token;
// 				po.Lang = "en";
// 				return NIL;
// 			});

// 			var gotHead = await UiTestTools.RunOnUiAsync(()=>po!.Head);
// 			var gotLang = await UiTestTools.RunOnUiAsync(()=>po!.Lang);
// 			var gotStoredAt = await UiTestTools.RunOnUiAsync(()=>po!.StoredAt);
// 			var gotCreatedAt = await UiTestTools.RunOnUiAsync(()=>po!.BizCreatedAt);
// 			var gotUpdatedAt = await UiTestTools.RunOnUiAsync(()=>po!.BizUpdatedAt);

// 			Assert.IsTrue(gotHead == token, "PoWordEdit.Head should be writable through interface.");
// 			Assert.IsTrue(gotLang == "en", "PoWordEdit.Lang should be writable through interface.");
// 			Assert.IsTrue(!str.IsNullOrWhiteSpace(gotStoredAt), "Free draft should initialize StoredAt.");
// 			Assert.IsTrue(!str.IsNullOrWhiteSpace(gotCreatedAt), "Free draft should initialize BizCreatedAt.");
// 			Assert.IsTrue(!str.IsNullOrWhiteSpace(gotUpdatedAt), "Free draft should initialize BizUpdatedAt.");
// 			return null;
// 		});
// 	}
// }
