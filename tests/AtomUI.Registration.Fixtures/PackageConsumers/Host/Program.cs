using AtomUI;
using AtomUI.Desktop.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Threading;
using Avalonia.VisualTree;

AppBuilder.Configure<ConsumerApp>()
          .UseHeadless(new AvaloniaHeadlessPlatformOptions())
          .UseAtomUI(builder => builder.UseDesktopControls())
          .SetupWithoutStarting();
var control = Fixture.BinaryAdapter.Adapter.CreateControl();
var window = new Avalonia.Controls.Window { Content = control };
window.Show();
Dispatcher.UIThread.RunJobs();
if (control.GetType().Name != "SearchEdit" || control is not TemplatedControl { Template: not null } || !control.GetVisualDescendants().Any())
    throw new Exception("COLD_CONSUMER_FAIL ordinary adapter template");
window.Close();
Console.WriteLine("COLD_CONSUMER_PASS ordinary adapter template, trimmed=" + AtomUI.Registration.ControlRegistrationRuntime.IsTrimmed);

public sealed class ConsumerApp : Application
{
    // 本工程不引用 AtomUI 生成器，按公开契约手写挂接。
    public override void RegisterServices()
    {
        base.RegisterServices();
        AtomUIStartup.Attach(this);
    }
}
