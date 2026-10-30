using Avalonia;
using Avalonia.Browser;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using AtomUI.Registration.Fixtures;

await AppBuilder.Configure<BrowserApp>().StartBrowserAppAsync("out");

public sealed class BrowserApp : ProductApp
{
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is ISingleViewApplicationLifetime single)
        {
            single.MainView = CreateContent();
            single.MainView.Loaded += (_, _) => Dispatcher.UIThread.Post(async () =>
            {
                try { await VerifyAsync(); Console.WriteLine("PRODUCT_PASS browser"); }
                catch (Exception e) { Console.WriteLine(e.ToString()); throw; }
            }, DispatcherPriority.Loaded);
        }
        base.OnFrameworkInitializationCompleted();
    }
}
