using Ngaq.Core.Infra;
using Ngaq.Core.Shared.Word.Models.Po.Word;
using Ngaq.Ui.Views.Word.Learn;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Ui.Test.Views.Word.Learn;

public partial class TestIViewLearnWord{
	public void RegisterEventDrivenExamples(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestIViewLearnWord),
			[typeof(IViewLearnWord)],
			[
				nameof(IViewLearnWord.ClickStart),
				nameof(IViewLearnWord.ClickReset),
				nameof(IViewLearnWord.WordInfo),
			],
			nameof(TestIViewLearnWord) + ".EventDriven"
		);
		var R = register.Register;

		R("EventDriven_ClickReset_Should_Raise_WordInfo_PropertyChanged", async(o)=>{
			await UiTestTools.AwaitPropertyChangedAsync(
				ViewLearnWord,
				nameof(IViewLearnWord.WordInfo),
				()=>UiTestTools.AssertNoUnhandledUiException(async ()=>{
					await ViewLearnWord.ClickReset(default);
				})
			);

			var wordInfo = await UiTestTools.RunOnUiAsync(()=>ViewLearnWord.WordInfo);
			Assert.IsTrue(wordInfo is not null, "ClickReset should keep WordInfo readable.");
			return null;
		});

		R("EventDriven_WordInfo_Should_Raise_When_Reset_Changes_Output", async(o)=>{
			var evt = await UiTestTools.AwaitPropertyChangedAsync(
				ViewLearnWord,
				nameof(IViewLearnWord.WordInfo),
				()=>UiTestTools.AssertNoUnhandledUiException(async ()=>{
					await ViewLearnWord.ClickReset(default);
				})
			);

			Assert.IsTrue(evt.PropertyName == nameof(IViewLearnWord.WordInfo), "Reset should raise WordInfo PropertyChanged.");
			return null;
		});

		R("EventDriven_ClickStart_Should_Raise_WordInfo_PropertyChanged_WhenSeedDataPrepared", async(o)=>{
			var userCtx = UserCtxMgr.GetDbUserCtx();
			var owner = UserCtxMgr.GetUserCtx().UserId;
			var token = "ut_ui_learn_start_evt_" + Guid.NewGuid().ToString("N");
			var words = new[] {
				new PoWord{Id = new IdWord(), Owner = owner, Head = token + "_1", Lang = "en"},
				new PoWord{Id = new IdWord(), Owner = owner, Head = token + "_2", Lang = "en"},
			};

			try{
				await SvcStudyPlan.RestoreBuiltinStudyPlan(userCtx, CT.None);
				await RunNoTxn(async(Ctx)=>{
					await RepoWord.OrdAdd(Ctx, AsyE(words), CT.None);
					return NIL;
				});

				var evt = await UiTestTools.AwaitPropertyChangedAsync(
					ViewLearnWord,
					nameof(IViewLearnWord.WordInfo),
					()=>UiTestTools.AssertNoUnhandledUiException(async ()=>{
						await ViewLearnWord.ClickReset(default);
						await ViewLearnWord.ClickStart(default);
					})
				);

				Assert.IsTrue(evt.PropertyName == nameof(IViewLearnWord.WordInfo), "ClickStart should raise WordInfo PropertyChanged.");
				return null;
			}
			finally{
				await UiTestTools.AssertNoUnhandledUiException(async ()=>{
					await ViewLearnWord.ClickReset(default);
				});
				await RunNoTxn(async(Ctx)=>{
					await RepoWord.OrdHardDelById(Ctx, AsyE(words.Select(x=>x.Id).ToArray()), CT.None);
					return NIL;
				});
			}
		});
	}
}
