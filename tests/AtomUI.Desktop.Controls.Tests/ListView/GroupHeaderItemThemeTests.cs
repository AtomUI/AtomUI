using AtomUI.Controls.Data;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomListView = AtomUI.Desktop.Controls.ListView;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.ListView;

public class GroupHeaderItemThemeTests
{
    static GroupHeaderItemThemeTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Group_Header_Background_Uses_The_ColorFillAlter_Token()
    {
        var listView = new AtomListView
        {
            IsGroupEnabled = true,
            Width         = 400,
            ItemsSource = new ListItemData[]
            {
                new() { Content = "Red", Group = "Basic Colors" },
                new() { Content = "Brown", Group = "Neutral Colors" }
            }
        };

        ShowInWindow(listView, () =>
        {
            var header = listView.GetVisualDescendants()
                                 .OfType<ListViewItem>()
                                 .First(static item => item.IsGroupItem);

            header.Background.ShouldBeAssignableTo<ISolidColorBrush>();
            var actual = ((ISolidColorBrush)header.Background!).Color;

            var application = Application.Current;
            application.ShouldNotBeNull();
            application!.TryGetResource(SharedTokenKind.ColorFillAlter, application.ActualThemeVariant, out var expected)
                        .ShouldBeTrue();
            var expectedColor = expected switch
            {
                Color color          => color,
                ISolidColorBrush brush => brush.Color,
                _ => throw new ShouldAssertException(
                    $"Unexpected resource value type '{expected!.GetType().FullName}'.")
            };

            actual.ShouldBe(expectedColor);
        });
    }

    private static void ShowInWindow(Control content, System.Action assertion)
    {
        var window = new AvaloniaWindow
        {
            Width   = 500,
            Height  = 400,
            Content = content
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            assertion();
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }
}
