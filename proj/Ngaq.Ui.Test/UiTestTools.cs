using System.Collections.Concurrent;
using System.ComponentModel;
using Avalonia.Threading;

namespace Ngaq.Ui.Test;

/// UI 測試通用輔助工具。
/// 集中放置 UI 線程調度、未處理異常捕獲、輪詢等待與事件等待，
/// 避免把測試基礎設施綁死在某個具體 tester 類中。
public static class UiTestTools{
	/// 在指定操作期間捕獲 Avalonia UI 線程上的未處理異常。
	/// 若操作或 UI 後續派發過程中出現未處理異常，則在此處統一轉成測試失敗。
	public static async Task<nil> AssertNoUnhandledUiException(Func<Task> Fn){
		var exceptions = new ConcurrentQueue<Exception>();
		void OnUnhandledException(object? Sender, DispatcherUnhandledExceptionEventArgs E){
			exceptions.Enqueue(E.Exception);
		}

		Dispatcher.UIThread.UnhandledException += OnUnhandledException;
		try{
			var tcs = new TaskCompletionSource<obj?>();
			Dispatcher.UIThread.Post(async ()=>{
				try{
					await Fn();
					tcs.SetResult(null);
				}catch(Exception Ex){
					tcs.SetException(Ex);
				}
			});
			await tcs.Task;
			await Dispatcher.UIThread.InvokeAsync(() => { });
		}finally{
			Dispatcher.UIThread.UnhandledException -= OnUnhandledException;
		}

		if(exceptions.TryDequeue(out var firstEx)){
			var exList = new List<Exception>{firstEx};
			while(exceptions.TryDequeue(out var Ex)){
				exList.Add(Ex);
			}
			throw new AggregateException("Unhandled UI exception captured during current test case.", exList);
		}

		return NIL;
	}

	/// 把委託切到 Avalonia UI 線程執行，並返回其結果。
	/// 供測試安全讀取控件屬性或操作 UI 對象。
	public static async Task<T> RunOnUiAsync<T>(Func<T> Fn){
		return await Dispatcher.UIThread.InvokeAsync(Fn);
	}

	/// 輪詢等待 UI 條件成立。
	/// 適用於目前尚未建立穩定事件信號、只能從最終可觀測狀態判斷完成的場景。
	public static async Task WaitUntilUiAsync(
		Func<bool> Pred
		,str FailMsg
		,i64 TimeoutMs = 3000
	){
		var startAt = Environment.TickCount64;
		while(true){
			if(await RunOnUiAsync(Pred)){
				return;
			}
			if(Environment.TickCount64 - startAt >= TimeoutMs){
				throw new TimeoutException(FailMsg);
			}
			await Task.Delay(20);
		}
	}

	/// 先訂閱 PropertyChanged，再執行操作，最後等待指定屬性的變化事件。
	/// 若超時仍未收到事件，則視為事件驅動契約未達成。
	public static async Task<PropertyChangedEventArgs> AwaitPropertyChangedAsync(
		INotifyPropertyChanged Source
		,str PropertyName
		,Func<Task> Act
		,int TimeoutMs = 3000
	){
		var tcs = new TaskCompletionSource<PropertyChangedEventArgs>();
		void OnChanged(object? Sender, PropertyChangedEventArgs E){
			if(E.PropertyName == PropertyName){
				tcs.TrySetResult(E);
			}
		}

		Source.PropertyChanged += OnChanged;
		try{
			await Act();
			var done = await Task.WhenAny(tcs.Task, Task.Delay(TimeoutMs));
			if(done != tcs.Task){
				throw new TimeoutException($"Expected PropertyChanged('{PropertyName}') within {TimeoutMs} ms.");
			}
			return await tcs.Task;
		}
		finally{
			Source.PropertyChanged -= OnChanged;
		}
	}

	/// 先訂閱普通 EventHandler 事件，再執行操作，最後等待事件到達。
	/// 適用於 View 契約中以 DoneXxx 表達“本次 UI 操作流程已結束”的場景。
	public static async Task<EventArgs> AwaitEventAsync(
		Action<EventHandler> Subscribe
		,Action<EventHandler> Unsubscribe
		,Func<Task> Act
		,int TimeoutMs = 3000
	){
		var tcs = new TaskCompletionSource<EventArgs>();
		void OnEvent(object? Sender, EventArgs E){
			tcs.TrySetResult(E);
		}

		Subscribe(OnEvent);
		try{
			await Act();
			var done = await Task.WhenAny(tcs.Task, Task.Delay(TimeoutMs));
			if(done != tcs.Task){
				throw new TimeoutException($"Expected event within {TimeoutMs} ms.");
			}
			return await tcs.Task;
		}
		finally{
			Unsubscribe(OnEvent);
		}
	}
}
