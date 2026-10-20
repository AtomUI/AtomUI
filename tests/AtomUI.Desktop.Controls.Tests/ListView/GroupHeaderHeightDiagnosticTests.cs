using AtomUI.Controls.Data;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomListView = AtomUI.Desktop.Controls.ListView;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.ListView;

public class GroupHeaderHeightDiagnosticTests
{
    static GroupHeaderHeightDiagnosticTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void First_Group_Header_Has_The_Same_Height_As_The_Second()
    {
        var listView = new AtomListView
        {
            IsGroupEnabled = true,
            Height         = 240,
            Width          = 400,
            ItemsSource = new ListItemData[]
            {
                new() { Content = "Olivia", Group = "Design" },
                new() { Content = "Liam", Group = "Design" },
                new() { Content = "Emma", Group = "Design" },
                new() { Content = "Noah", Group = "Engineering" },
                new() { Content = "Ava", Group = "Engineering" },
                new() { Content = "Ethan", Group = "Engineering" }
            }
        };

        ShowInWindow(listView, () =>
        {
            var headers = listView.GetVisualDescendants()
                                  .OfType<ListViewItem>()
                                  .Where(i => i.IsGroupItem)
                                  .ToArray();
            headers.Length.ShouldBe(2);

            headers[0].Bounds.Height.ShouldBe(headers[1].Bounds.Height);
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
