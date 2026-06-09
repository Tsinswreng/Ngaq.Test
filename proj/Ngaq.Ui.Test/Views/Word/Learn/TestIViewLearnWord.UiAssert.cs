using System.Collections.Concurrent;
using Avalonia.Threading;
using Tsinswreng.CsCore;

namespace Ngaq.Ui.Test.Views.Word.Learn;

public partial class TestIViewLearnWord{
	protected async Task<nil> AssertNoUnhandledUiException(Func<Task> Fn){
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
				}catch(Exception ex){
					tcs.SetException(ex);
				}
			});
			await tcs.Task;
			await Dispatcher.UIThread.InvokeAsync(() => { });
		}finally{
			Dispatcher.UIThread.UnhandledException -= OnUnhandledException;
		}

		if(exceptions.TryDequeue(out var firstEx)){
			var exList = new List<Exception>{ firstEx };
			while(exceptions.TryDequeue(out var ex)){
				exList.Add(ex);
			}
			throw new AggregateException("Unhandled UI exception captured during current test case.", exList);
		}

		return NIL;
	}

	protected async Task<T> RunOnUiAsync<T>(
		Func<T> Fn
	){
		return await Dispatcher.UIThread.InvokeAsync(Fn);
	}

	protected async Task WaitUntilUiAsync(
		Func<bool> Pred
		,str FailMsg
		,int TimeoutMs = 3000
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
}
