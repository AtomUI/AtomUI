using AtomUI;
using AtomUI.Desktop.Controls;
using Avalonia;
using Avalonia.Browser;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives;
using Avalonia.VisualTree;
using Fixture.NuGetAuthor;

await AppBuilder.Configure<ConsumerApp>()
                .UseAtomUI(builder => builder.UseDesktopControls().UseAuthorControls())
                .StartBrowserAppAsync("out");
public sealed class ConsumerApp : Application
{
    // 本工程不引用 AtomUI 生成器，按公开契约手写挂接。
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
