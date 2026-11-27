using AtomUI;
using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Desktop.Controls;
using AtomUI.Desktop.Controls.Themes.AntDesign;
using Avalonia;
using Avalonia.Browser;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives;
using Avalonia.VisualTree;
using Fixture.NuGetAuthor;

await AppBuilder.Configure<ConsumerApp>()
                .UseAtomUI(builder => builder.UseDesktopControls().UseAuthorControls().UseAntDesign(language => language.AddDesktopControls()))
                .StartBrowserAppAsync("out");
public sealed class ConsumerApp : Application
{
    // This project does not reference the AtomUI generator, so the hook is written by hand against the
    // public contract.
    public override void RegisterServices()
    {
        base.RegisterServices();
        ApplicationStartup.Attach(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is ISingleViewApplicationLifetime single)
        {
            var control = Fixture.BinaryAdapter.Adapter.CreateControl();
            var author = new AuthorControl();
            single.MainView = new StackPanel { Children = { control, author } };
            control.Loaded += (_, _) =>
            {
                if (control.GetType().Name != "SearchEdit" || control is not TemplatedControl { Template: not null } || !control.GetVisualDescendants().Any())
                    throw new Exception("COLD_BROWSER_FAIL ordinary binary adapter template");
                if (author.TryFindResource("FixtureDesktopOverlay", out _) ||
                    !author.TryFindResource(typeof(AuthorControl), out _))
                    throw new Exception("COLD_BROWSER_FAIL resource platform restriction changed the portable target");
                Console.WriteLine("COLD_BROWSER_PASS ordinary binary adapter template");
            };
        }
        base.OnFrameworkInitializationCompleted();
    }
}
