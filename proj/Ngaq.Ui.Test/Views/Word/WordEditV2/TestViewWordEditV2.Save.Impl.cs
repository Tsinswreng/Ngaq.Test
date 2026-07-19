namespace Ngaq.Ui.Test.Views.Word.WordEditV2;

using Ngaq.Core.Shared.Word.Models;
using Ngaq.Core.Shared.Word.Models.Po.Word;
using Ngaq.Core.Tools;
using Ngaq.Ui.Tools;
using Ngaq.Ui.Views.Word.WordEditV2;
using Tsinswreng.CsTempus;
using Tsinswreng.CsTools;
using Tsinswreng.CsTreeTest;

/// 實現單詞編輯頁的新增、修改及輸入校驗功能測試。
public partial class TestViewWordEditV2{
	/// 註冊以 `VmWordEditV2.Save` 為核心的完整表單操作用例。
	public partial void RegisterSave(ITestNode Node){
		var Register = Node.MkTestFnRegister(
			typeof(TestViewWordEditV2)
			,[typeof(ViewWordEditV2), typeof(VmWordEditV2)]
			,[nameof(VmWordEditV2.Save)]
			,nameof(TestViewWordEditV2)
		);
		var R = Register.Register;
		R(nameof(Add_New_Word_Through_Form_Should_Persist_Input), Add_New_Word_Through_Form_Should_Persist_Input!);
		R(nameof(Edit_Existing_Word_Through_Form_Should_Update_Seed_Data), Edit_Existing_Word_Through_Form_Should_Update_Seed_Data!);
		R(nameof(Save_With_Empty_Head_Should_Reject_Operation_Without_Data_Change), Save_With_Empty_Head_Should_Reject_Operation_Without_Data_Change!);
	}

	/// 由真實輸入控件提交新增資料，並以資料庫內容作為最終成功條件。
	public partial async Task<nil> Add_New_Word_Through_Form_Should_Persist_Input(obj? O){
		var Token = "ut_ui_word_add_" + Guid.NewGuid().ToString("N");
		var SavedId = default(IdWord);
		try{
			var View = await MkView(CT.None);
			await UiTestTools.RunOnUiAsync(()=>{
				View.Ctx?.InitFreeAddDraft("en");
				View.PoWordEdit!.HeadCtrl!.Text = Token;
				View.PoWordEdit.LangCtrl!.Text = "ja";
				SavedId = View.Ctx?.Draft?.Word.Id ?? default;
				return NIL;
			});
			await UiTestTools.AssertNoUnhandledUiException(async()=>{
				await View.PoWordEdit!.SaveBtn!.ClickAndWaitDone(CT.None);
			});

			var Got = await RunNoTxn(async Ctx=>{
				return await RepoWord.OrdGetByIdWithDel(Ctx, ToolAsyE.ToAsyE([SavedId]), CT.None).FirstOrDefaultAsync(CT.None);
			});
			Assert.IsTrue(Got is not null, "Saving a new word through the form should create a database row.");
			Assert.IsTrue(Got!.Head == Token, "Persisted head should equal the value entered in HeadCtrl.");
			Assert.IsTrue(Got.Lang == "ja", "Persisted language should equal the value entered in LangCtrl.");
			return NIL;
		}finally{
			if(!SavedId.IsNullOrDefault()){
				await RunNoTxn(async Ctx=>{
					await RepoWord.HardDelAggInId<JnWord>(Ctx, ToolAsyE.ToAsyE([SavedId]), CT.None);
					return NIL;
				});
			}
		}
	}

	/// 以既有種子資料進入編輯頁，確認保存會更新原記錄而不是另建副本。
	public partial async Task<nil> Edit_Existing_Word_Through_Form_Should_Update_Seed_Data(obj? O){
		var Owner = UserCtxMgr.GetUserCtx().UserId;
		var Token = "ut_ui_word_edit_" + Guid.NewGuid().ToString("N");
		var Seed = new PoWord{
			Id = new IdWord(), Owner = Owner, Head = Token + "_before", Lang = "en",
			StoredAt = UnixMs.Now(), BizCreatedAt = UnixMs.Now(), BizUpdatedAt = UnixMs.Now(),
		};
		try{
			await RunNoTxn(async Ctx=>{
				await RepoWord.OrdAdd(Ctx, ToolAsyE.ToAsyE([Seed]), CT.None);
				return NIL;
			});
			var View = await MkView(CT.None);
			await UiTestTools.RunOnUiAsync(()=>{
				View.Ctx?.FromJnWord(new JnWord{Word = Seed, Props = [], Learns = []});
				View.PoWordEdit!.HeadCtrl!.Text = Token + "_after";
				View.PoWordEdit.LangCtrl!.Text = "de";
				return NIL;
			});
			await UiTestTools.AssertNoUnhandledUiException(async()=>{
				await View.PoWordEdit!.SaveBtn!.ClickAndWaitDone(CT.None);
			});

			var Got = await RunNoTxn(async Ctx=>{
				return await RepoWord.OrdGetByIdWithDel(Ctx, ToolAsyE.ToAsyE([Seed.Id]), CT.None).FirstOrDefaultAsync(CT.None);
			});
			Assert.IsTrue(Got is not null, "Editing should preserve the seeded word row.");
			Assert.IsTrue(Got!.Head == Token + "_after", "Editing should persist the new head on the original row.");
			Assert.IsTrue(Got.Lang == "de", "Editing should persist the new language on the original row.");
			return NIL;
		}finally{
			await RunNoTxn(async Ctx=>{
				await RepoWord.HardDelAggInId<JnWord>(Ctx, ToolAsyE.ToAsyE([Seed.Id]), CT.None);
				return NIL;
			});
		}
	}

	/// 提交非法表單，驗證校驗錯誤可見且不會產生任何持久化副作用。
	public partial async Task<nil> Save_With_Empty_Head_Should_Reject_Operation_Without_Data_Change(obj? O){
		var DraftId = default(IdWord);
		try{
			var View = await MkView(CT.None);
			await UiTestTools.RunOnUiAsync(()=>{
				View.Ctx?.InitFreeAddDraft("en");
				View.PoWordEdit!.HeadCtrl!.Text = "";
				View.PoWordEdit.LangCtrl!.Text = "en";
				DraftId = View.Ctx?.Draft?.Word.Id ?? default;
				return NIL;
			});
			await UiTestTools.AssertNoUnhandledUiException(async()=>{
				await View.PoWordEdit!.SaveBtn!.ClickAndWaitDone(CT.None);
			});

			var LastError = await UiTestTools.RunOnUiAsync(()=>View.Ctx?.LastError ?? "");
			var Got = await RunNoTxn(async Ctx=>{
				return await RepoWord.OrdGetByIdWithDel(Ctx, ToolAsyE.ToAsyE([DraftId]), CT.None).FirstOrDefaultAsync(CT.None);
			});
			Assert.IsTrue(!str.IsNullOrWhiteSpace(LastError), "Invalid form submission should expose a validation error.");
			Assert.IsTrue(Got is null, "Invalid form submission must not create a database row.");
			return NIL;
		}finally{
			if(!DraftId.IsNullOrDefault()){
				await RunNoTxn(async Ctx=>{
					await RepoWord.HardDelAggInId<JnWord>(Ctx, ToolAsyE.ToAsyE([DraftId]), CT.None);
					return NIL;
				});
			}
		}
	}
}
