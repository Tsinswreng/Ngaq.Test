using Ngaq.Core.Shared.Word.Models;
using Ngaq.Core.Shared.Base.Models.Po;
using Ngaq.Core.Shared.Word.Models.Po.Word;
using Ngaq.Core.Tools;
using Ngaq.Ui.Views.Word.WordEditV2;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Ui.Test.Views.Word.WordEditV2;

public partial class TestIViewWordEditV2{
	public void RegisterClickDelete(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestIViewWordEditV2),
			[typeof(IViewWordEditV2)],
			[nameof(IViewWordEditV2.ClickDelete), nameof(IViewWordEditV2.DoneDelete)],
			nameof(TestIViewWordEditV2)
		);
		var R = register.Register;

		R("ClickDelete_Should_Raise_DoneDelete_And_SoftDelete_SavedWord", async(o)=>{
			var concrete = ViewWordEditV2 as ViewWordEditV2;
			Assert.IsTrue(concrete is not null, "Current test host should provide concrete ViewWordEditV2.");

			var token = "ut_ui_word_edit_delete_" + Guid.NewGuid().ToString("N");
			IdWord savedId = default;
			try{
				await UiTestTools.RunOnUiAsync(()=>{
					concrete!.Ctx?.InitFreeAddDraft("en");
					if(ViewWordEditV2.PoWordEdit is not null){
						ViewWordEditV2.PoWordEdit.Head = token;
						ViewWordEditV2.PoWordEdit.Lang = "en";
					}
					return NIL;
				});

				savedId = await UiTestTools.RunOnUiAsync(()=>concrete!.Ctx?.Draft?.Word.Id ?? default);
				await UiTestTools.AwaitEventAsync(
					h=>ViewWordEditV2.DoneSave += h,
					h=>ViewWordEditV2.DoneSave -= h,
					()=>UiTestTools.AssertNoUnhandledUiException(async ()=>{
						await ViewWordEditV2.ClickSave(default);
					})
				);

				await UiTestTools.AwaitEventAsync(
					h=>ViewWordEditV2.DoneDelete += h,
					h=>ViewWordEditV2.DoneDelete -= h,
					()=>UiTestTools.AssertNoUnhandledUiException(async ()=>{
						await ViewWordEditV2.ClickDelete(default);
					})
				);

				var got = await RunNoTxn(async(Ctx)=>{
					return await RepoWord.BatGetByIdWithDel(Ctx, AsyE(savedId), CT.None).FirstOrDefaultAsync(CT.None);
				});

				Assert.IsTrue(got is not null, "ClickDelete should keep soft-deleted root queryable with-del.");
				Assert.IsTrue(got!.IsDeleted(), "ClickDelete should soft delete current word.");
				return null;
			}
			finally{
				if(!savedId.IsNullOrDefault()){
					await RunNoTxn(async(Ctx)=>{
						await RepoWord.HardDelAggInId<JnWord>(Ctx, AsyE(savedId), CT.None);
						return NIL;
					});
				}
			}
		});
	}
}
