// using Avalonia.Threading;
// using Ngaq.Core.Infra;
// using Ngaq.Core.Shared.Word.Models.Po.Word;
// using Ngaq.Ui.Views.Word.Learn;
// using Tsinswreng.CsTreeTest;

// namespace Ngaq.Ui.Test.Views.Word.Learn;

// public partial class TestIViewLearnWord{
// 	public void RegisterClickStart(ITestNode Node){
// 		var register = Node.MkTestFnRegister(
// 			typeof(TestIViewLearnWord)
// 			,[typeof(IViewLearnWord)]
// 			,[nameof(IViewLearnWord.ClickStart)]
// 			,nameof(TestIViewLearnWord)
// 		);
// 		var R = register.Register;

// 		R("ClickStart_Should_LoadWordCards_WhenSeedDataPrepared", async(o)=>{
// 			var userCtx = UserCtxMgr.GetDbUserCtx();
// 			var owner = UserCtxMgr.GetUserCtx().UserId;
// 			var token = "ut_ui_learn_start_" + Guid.NewGuid().ToString("N");
// 			var words = new[]{
// 				new PoWord{Id = new IdWord(), Owner = owner, Head = token + "_1", Lang = "en"},
// 				new PoWord{Id = new IdWord(), Owner = owner, Head = token + "_2", Lang = "en"},
// 			};

// 			try{
// 				await SvcStudyPlan.RestoreBuiltinStudyPlan(userCtx, CT.None);
// 				await RunNoTxn(async(Ctx)=>{
// 					await RepoWord.OrdAdd(Ctx, AsyE(words), CT.None);
// 					return NIL;
// 				});

// 				await UiTestTools.AssertNoUnhandledUiException(async ()=>{
// 					await ViewLearnWord.ClickReset(default);
// 					await ViewLearnWord.ClickStart(default);
// 				});
// 				try{
// 					await UiTestTools.WaitUntilUiAsync(
// 						()=>ViewLearnWord.WordListCards?.Count > 0,
// 						"ClickStart should render word cards after seeded data is prepared."
// 					);
// 				}catch(TimeoutException){
// 					await PrintClickStartDiag();
// 					throw;
// 				}
// 				var cards = await UiTestTools.RunOnUiAsync(()=>ViewLearnWord.WordListCards);
// 				if(cards is null){
// 					await PrintClickStartDiag();
// 					throw new Exception("ClickStart should expose WordListCards after start.");
// 				}
// 				if(cards.Count == 0){
// 					await PrintClickStartDiag();
// 					throw new Exception("ClickStart should render word cards after seeded data is prepared.");
// 				}
// 				return null;
// 			}
// 			finally{
// 				await UiTestTools.AssertNoUnhandledUiException(async ()=>{
// 					await ViewLearnWord.ClickReset(default);
// 				});
// 				await RunNoTxn(async(Ctx)=>{
// 					await RepoWord.OrdHardDelById(Ctx, AsyE(words.Select(x=>x.Id).ToArray()), CT.None);
// 					return NIL;
// 				});
// 			}

// 			async Task PrintClickStartDiag(){
// 				if(ViewLearnWord is not ViewLearnWords concrete){
// 					Console.Error.WriteLine("[TEST][ClickStartDiag] view is not ViewLearnWords");
// 					return;
// 				}
// 				await Dispatcher.UIThread.InvokeAsync(()=>{
// 					var itemSrcCnt = concrete.WordListItemsCtrl?.ItemsSource is System.Collections.ICollection coll
// 						? coll.Count
// 						: -1;
// 					Console.Error.WriteLine(
// 						$"[TEST][ClickStartDiag] IsLoaded={concrete.IsLoaded}; " +
// 						$"VmWordCards={concrete.Ctx?.WordCards?.Count ?? -1}; " +
// 						$"ItemsSourceCount={itemSrcCnt}; " +
// 						$"WordCardCtrls={concrete.WordCardCtrls.Count}"
// 					);
// 				});
// 			}
// 		});
// 	}
// }
