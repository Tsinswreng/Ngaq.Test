namespace Ngaq.Ui.Test.Views.Word.WordEditV2;

using Ngaq.Core.Shared.Word.Models;
using Ngaq.Core.Shared.Word.Models.Po.Word;
using Ngaq.Core.Tools;
using Ngaq.Ui.Tools;
using Ngaq.Ui.Views.Word.WordEditV2;
using Tsinswreng.CsTempus;
using Tsinswreng.CsTools;
using Tsinswreng.CsTreeTest;

/// 實現單詞編輯頁的刪除功能測試。
public partial class TestViewWordEditV2{
	/// 註冊以 `VmWordEditV2.Delete` 為核心的真實按鈕操作用例。
	public partial void RegisterDelete(ITestNode Node){
		var Register = Node.MkTestFnRegister(
			typeof(TestViewWordEditV2)
			,[typeof(ViewWordEditV2), typeof(VmWordEditV2)]
			,[nameof(VmWordEditV2.Delete)]
			,nameof(TestViewWordEditV2)
		);
		Register.Register(
			nameof(Delete_Existing_Word_Through_Button_Should_Soft_Delete_Seed_Data)
			,Delete_Existing_Word_Through_Button_Should_Soft_Delete_Seed_Data!
		);
	}

	/// 由真實刪除按鈕軟刪種子資料，並以普通查詢及含刪除查詢交叉驗證結果。
	public partial async Task<nil> Delete_Existing_Word_Through_Button_Should_Soft_Delete_Seed_Data(obj? O){
		var ct = new CT();
		var Owner = UserCtxMgr.GetUserCtx().UserId;
		var Token = "ut_ui_word_delete_" + Guid.NewGuid().ToString("N");
		var Seed = new PoWord{
			Id = new IdWord(), Owner = Owner, Head = Token, Lang = "en",
			StoredAt = UnixMs.Now(), BizCreatedAt = UnixMs.Now(), BizUpdatedAt = UnixMs.Now(),
		};
		try{
			await RunNoTxn(async Ctx=>{
				await RepoWord.OrdAdd(Ctx, ToolAsyE.ToAsyE([Seed]), CT.None);
				return NIL;
			});
			var View = await MkView(CT.None);
			await UiTestTools.RunOnUi(()=>{
				View.Ctx?.FromJnWord(new JnWord{Word = Seed, Props = [], Learns = []});
				return NIL;
			}, ct);
			await UiTestTools.AssertNoUnhandledUiException(async()=>{
				await View.PoWordEdit!.DeleteBtn!.ClickAndWaitDone(CT.None);
			});

			var Results = await RunNoTxn(async Ctx=>{
				var Visible = await RepoWord.OrdGetById(Ctx, ToolAsyE.ToAsyE([Seed.Id]), CT.None).FirstOrDefaultAsync(CT.None);
				var WithDeleted = await RepoWord.OrdGetByIdWithDel(Ctx, ToolAsyE.ToAsyE([Seed.Id]), CT.None).FirstOrDefaultAsync(CT.None);
				return (Visible, WithDeleted);
			});
			Assert.IsTrue(Results.Visible is null, "Soft-deleted word should disappear from ordinary queries.");
			Assert.IsTrue(Results.WithDeleted is not null, "Soft-deleted word should remain available to queries including deleted rows.");
			Assert.IsTrue(!Results.WithDeleted!.DelAt.IsNullOrDefault(), "Delete button should set the word deletion timestamp.");
			return NIL;
		}finally{
			await RunNoTxn(async Ctx=>{
				await RepoWord.HardDelAggInId<JnWord>(Ctx, ToolAsyE.ToAsyE([Seed.Id]), CT.None);
				return NIL;
			});
		}
	}
}
