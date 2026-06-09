#if false
測試方法:
+ 直接在Ngaq.Windows.Test中執行 dotnet run (非AOT編譯)
+ 執行以下腳本 在AOT還境下測試
```bash
dotnet publish -c Release -r win-x64
./bin/Release/net10.0/win-x64/publish/Ngaq.Windows.Test.exe
```
#endif

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Ngaq.Backend;
using Ngaq.Backend.Di;
using Ngaq.Core;
using Ngaq.Core.Shared.Audio;
using Ngaq.Ui;
using Ngaq.Ui.Infra;
using Ngaq.Ui.Views;
using Ngaq.Ui.Views.Word.Learn;
using Tsinswreng.CsCore;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Windows.Test;

internal class Program{
	public static IServiceCollection SvcColct = new ServiceCollection();
	public static IServiceProvider SvcProvdr = null!;

	[STAThread]
	public static void Main(string[] args){
		var lifetime = new ClassicDesktopStyleApplicationLifetime(){ Args = args };
		AppBuilder.Configure<TestApp>()
			.UsePlatformDetect()
			.SetupWithLifetime(lifetime);

		SvcColct
			.SetupCore()
			.SetupLocal()
			.SetupLocalFrontend()
			.SetupUi()
		;
		SvcColct.AddSingleton<IAudioPlayer, FakeAudioPlayer>();

		var mgr = WindowsTestMgr.Inst;
		SvcProvdr = mgr.InitSvc(SvcColct, sc => {
			var sp = sc.BuildServiceProvider();
			App.SetSvcProvider(sp);
			AppIniter.Inst.Sp = sp;
			_ = AppIniter.Inst.Init(default).Result;

			var view = new ViewLearnWords();
			sc.AddSingleton<IViewLearnWord>(view);
			var sp2 = sc.BuildServiceProvider();
			App.SetSvcProvider(sp2);
			return sp2;
		});

		lifetime.MainWindow = new MainWindow();

		Dispatcher.UIThread.UnhandledException += (s, e) => {
			Console.Error.WriteLine($"[TEST] Unhandled UI exception: {e.Exception?.GetBaseException()?.Message}");
			e.Handled = true;
		};

		Dispatcher.UIThread.Post(async () => {
			try{
				_ = MainView.Inst;
				if(SvcProvdr.GetService<IViewLearnWord>() is Control learnView){
					MgrViewNavi.Inst.ViewNavi?.GoTo(learnView);
					await Dispatcher.UIThread.InvokeAsync(() => { });
				}
				ITestExecutor executor = new TreeTestExecutor();
				await executor.RunEtPrint(mgr.TestNode);
			}catch(Exception ex){
				Console.Error.WriteLine(ex);
			}
			lifetime.Shutdown();
		});

		lifetime.Start(args);
	}

	private sealed class TestApp: Application{
		public override void Initialize(){}
		public override void OnFrameworkInitializationCompleted(){}
	}
}
