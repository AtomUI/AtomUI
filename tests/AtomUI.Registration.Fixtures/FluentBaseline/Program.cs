using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Themes.Fluent;
using Avalonia.Threading;
using Avalonia.VisualTree;

AppBuilder.Configure<BaselineApp>().UsePlatformDetect().WithInterFont().SetupWithoutStarting();
var button = new Button { Content = "TypeMap product" };
var stack = new StackPanel();
stack.Children.Add(button);
var window = new Window { Content = stack, Width = 600, Height = 500 };
window.Show();
Dispatcher.UIThread.RunJobs();
if (!button.GetVisualDescendants().OfType<ContentPresenter>().Any())
    throw new InvalidOperationException("BASELINE_FAIL Fluent Button template did not execute");
window.Close();
Console.WriteLine("BASELINE_PASS fluent");

internal sealed class BaselineApp : Application
{
    public override void Initialize() => Styles.Add(new FluentTheme());
}
