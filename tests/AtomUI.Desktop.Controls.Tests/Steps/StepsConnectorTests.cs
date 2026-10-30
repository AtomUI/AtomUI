using System.Reflection;
using AtomUI.Controls.Primitives;
using AtomUI.Desktop.Controls.DesignTokens;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Steps;

public class StepsConnectorTests
{
    static StepsConnectorTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Connector_Uses_The_Next_Items_Effective_Status_Color()
    {
        var steps = CreateSteps(Orientation.Horizontal);
        steps.Items[0].ShouldBeOfType<Desktop.Controls.StepsItem>().Status = Desktop.Controls.StepsStatus.Finish;
        steps.Items[1].ShouldBeOfType<Desktop.Controls.StepsItem>().Status = Desktop.Controls.StepsStatus.Error;

        ShowInWindow(steps, () =>
        {
            var first = steps.Items[0].ShouldBeOfType<Desktop.Controls.StepsItem>();
            var connector = FindConnector(first);
            var expected = GetThemeResource<IBrush>(StepsTokenKind.ErrorTailColor);

            first.ConnectorStatus.ShouldBe(Desktop.Controls.StepsStatus.Error);
            GetColor(connector.Background).ShouldBe(GetColor(expected));
        });
    }

    [Theory]
    [InlineData(Orientation.Horizontal)]
    [InlineData(Orientation.Vertical)]
    public void Connector_Uses_Orientation_And_Last_Item_Visibility(Orientation orientation)
    {
        var steps = CreateSteps(orientation);

        ShowInWindow(steps, () =>
        {
            var firstConnector = FindConnector(steps.Items[0].ShouldBeOfType<Desktop.Controls.StepsItem>());
            var lastConnector = FindConnector(steps.Items[2].ShouldBeOfType<Desktop.Controls.StepsItem>());

            firstConnector.IsVisible.ShouldBeTrue();
            lastConnector.IsVisible.ShouldBeFalse();
            if (orientation == Orientation.Horizontal)
            {
                firstConnector.Bounds.Width.ShouldBeGreaterThan(firstConnector.Bounds.Height);
            }
            else
            {
                firstConnector.Bounds.Height.ShouldBeGreaterThan(firstConnector.Bounds.Width);
            }
        });
    }

    [Theory]
    [InlineData(StepsType.Default, Orientation.Horizontal)]
    [InlineData(StepsType.Default, Orientation.Vertical)]
    [InlineData(StepsType.Dot, Orientation.Horizontal)]
    [InlineData(StepsType.OutlineDot, Orientation.Horizontal)]
    [InlineData(StepsType.Inline, Orientation.Horizontal)]
    public void Natural_Size_Reserves_Visible_Connectors_For_Short_Headings(
        StepsType type, Orientation titlePlacement)
    {
        var steps = new Desktop.Controls.Steps
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            Type = type,
            TitlePlacement = titlePlacement,
            SizeType = AtomUI.SizeType.Small
        };
        for (var index = 1; index <= 4; index++)
        {
            steps.Items.Add(new StepsItem { Header = index.ToString() });
        }

        ShowInWindow(steps, () =>
        {
            steps.Bounds.Width.ShouldBeLessThan(800);
            var items = steps.Items.Cast<StepsItem>().ToArray();
            foreach (var item in items.Take(3))
            {
                var connector = FindConnector(item);
                connector.IsVisible.ShouldBeTrue();
                connector.Bounds.Width.ShouldBeGreaterThan(connector.Bounds.Height);
                connector.Background.ShouldNotBeNull();
            }
            FindConnector(items[^1]).IsVisible.ShouldBeFalse();
        });
    }

    [Fact]
    public void Every_Remaining_Steps_Token_Has_A_Theme_Consumer()
    {
        var themeSource = string.Join(
            Environment.NewLine,
            ReadRepoFile("src/AtomUI.Desktop.Controls/Steps/Themes/StepsTheme.axaml"),
            ReadRepoFile("src/AtomUI.Desktop.Controls/Steps/Themes/StepsItemTheme.axaml"),
            ReadRepoFile("src/AtomUI.Desktop.Controls/Steps/Themes/StepsItemIndicatorTheme.axaml"));

        var unconsumed = typeof(Desktop.Controls.StepsToken)
                         .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                         .Select(property => property.Name)
                         .Where(name => !themeSource.Contains($"StepsTokenResource {name}", StringComparison.Ordinal))
                         .ToArray();

        unconsumed.ShouldBeEmpty();
    }

    private static Desktop.Controls.Steps CreateSteps(Orientation orientation)
    {
        var steps = new Desktop.Controls.Steps
        {
            Width       = 720,
            Current     = 0,
            Type        = Desktop.Controls.StepsType.Default,
            Orientation = orientation
        };
        steps.Items.Add(new Desktop.Controls.StepsItem { Header = "First", Content = "Content" });
        steps.Items.Add(new Desktop.Controls.StepsItem { Header = "Second", Content = "Content" });
        steps.Items.Add(new Desktop.Controls.StepsItem { Header = "Last", Content = "Content" });
        return steps;
    }

    private static PixelAlignedBorder FindConnector(Desktop.Controls.StepsItem item)
    {
        return item.GetVisualDescendants()
                   .OfType<PixelAlignedBorder>()
                   .Single(control => control.Name == "Connector");
    }

    private static T GetThemeResource<T>(object key)
    {
        var application = Application.Current.ShouldNotBeNull();
        application.TryGetResource(key, application.ActualThemeVariant, out var value).ShouldBeTrue();
        value.ShouldBeAssignableTo<T>();
        return (T)value!;
    }

    private static Color GetColor(IBrush? brush)
    {
        brush.ShouldNotBeNull();
        return ((ISolidColorBrush)brush!).Color;
    }

    private static string ReadRepoFile(string relativePath)
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null;
             directory = directory.Parent)
        {
            var path = Path.Combine(directory.FullName, relativePath);
            if (File.Exists(path))
            {
                return File.ReadAllText(path);
            }
        }

        throw new FileNotFoundException(relativePath);
    }

    private static void ShowInWindow(Control content, Action assertion)
    {
        var window = new AvaloniaWindow
        {
            Width   = 900,
            Height  = 420,
            Content = content
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            AvaloniaHeadlessPlatform.ForceRenderTimerTick(1);
            Dispatcher.UIThread.RunJobs();
            assertion();
        }
        finally
        {
            window.Close();
        }
    }
}
