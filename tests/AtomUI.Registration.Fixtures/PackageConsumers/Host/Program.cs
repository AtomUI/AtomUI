using AtomUI;
using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Desktop.Controls;
using AtomUI.Desktop.Controls.Themes.AntDesign;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Threading;
using Avalonia.VisualTree;

AppBuilder.Configure<ConsumerApp>()
          .UseHeadless(new AvaloniaHeadlessPlatformOptions())
          .UseAtomUI(builder => builder.UseDesktopControls().UseAntDesign(language => language.AddDesktopControls()))
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
    // This project does not reference the AtomUI generator, so the hook is written by hand against the
    // public contract.
    public override void RegisterServices()
    {
        base.RegisterServices();
        ApplicationStartup.Attach(this);
    }
}
