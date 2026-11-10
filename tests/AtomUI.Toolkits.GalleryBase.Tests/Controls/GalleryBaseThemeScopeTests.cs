using AtomUI.Theme;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Schema;
using AtomUI.Toolkits.GalleryBase.Controls;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;

namespace AtomUI.Toolkits.GalleryBase.Tests.Controls;

public class GalleryBaseThemeScopeTests
{
    static GalleryBaseThemeScopeTests() => AvaloniaTestApp.EnsureInitialized();

    [Fact]
    public void Control_Override_Root_Override_And_Nested_Public_Control_Have_Distinct_Scopes()
    {
        var button = new AtomUI.Desktop.Controls.Button { Content = "Nested public Button" };
        var item = new ShowCaseItem { Title = "Sample", Content = button };
        var unrelated = new ShowCaseZoomOverlay { IsOpen = true, Title = "Other" };
        var provider = new ThemeConfigProvider { Child = new StackPanel { Children = { item, unrelated } } };
        var window = new Window { Width = 1100, Height = 760, Content = provider };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            var buttonBackground = button.Background;
            var unrelatedBackground = ReadColor(unrelated, "PART_CardBorder");
            provider.Config = new ThemeConfigBuilder().WithControl(new ControlTokenIdentity("AtomUI", "ShowCaseItem"),
                new ControlThemeConfigBuilder().WithToken("ColorBgContainer", "#FF123456").Build()).Build();
            Dispatcher.UIThread.RunJobs();
            ReadColor(item, "PART_RootBorder").ShouldBe(Color.Parse("#FF123456"));
            ReadColor(unrelated, "PART_CardBorder").ShouldBe(unrelatedBackground);
            button.Background.ShouldBe(buttonBackground);
            provider.Config = new ThemeConfigBuilder().WithToken("ColorBgContainer", "#FF123456").Build();
            Dispatcher.UIThread.RunJobs();
            ReadColor(item, "PART_RootBorder").ShouldBe(Color.Parse("#FF123456"));
            ReadColor(unrelated, "PART_CardBorder").ShouldBe(Color.Parse("#FF123456"));
        }
        finally
        {
            window.Close();
        }
    }

    private static Color ReadColor(Control root, string part)
    {
        var visual = part.Length == 0 ? root : root.GetVisualDescendants().OfType<Control>().Single(x => x.Name == part);
        IBrush? brush = visual switch
        {
            Border border => border.Background,
            GallerySelectableTextBlock text => text.SelectionBrush,
            Avalonia.Controls.Primitives.TemplatedControl control => control.Foreground,
            Avalonia.Controls.TextBlock text => text.Foreground,
            _ => throw new InvalidOperationException(visual.GetType().Name)
        };
        return ((ISolidColorBrush)brush!).Color;
    }
}
