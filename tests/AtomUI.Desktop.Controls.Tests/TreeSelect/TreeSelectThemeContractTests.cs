using AtomUI.Controls;
using Avalonia;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.TreeSelectControl;

public class TreeSelectThemeContractTests
{
    static TreeSelectThemeContractTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void TreeSelect_Multiple_Empty_Small_Uses_Small_Content_Padding()
    {
        var control = new global::AtomUI.Desktop.Controls.TreeSelect
        {
            Width = 240,
            IsMultiple = true,
            SizeType = CustomizableSizeType.Small
        };
        var window = new Avalonia.Controls.Window { Width = 320, Height = 120, Content = control };
        try
        {
            window.Show();
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            var frame = control.GetVisualDescendants().OfType<AddOnDecoratedBoxContentFrame>().Single();
            control.IsSelectionEmpty.ShouldBeTrue();
            control.Padding.ShouldBe(new Thickness(7, 0));
            frame.Padding.ShouldBe(new Thickness(7, 0));

            control.Padding = new Thickness(17, 9);
            window.UpdateLayout();
            frame.Padding.ShouldBe(new Thickness(17, 9));

            control.ClearValue(global::AtomUI.Desktop.Controls.TreeSelect.PaddingProperty);
            window.UpdateLayout();
            frame.Padding.ShouldBe(new Thickness(7, 0));
        }
        finally
        {
            window.Close();
        }
    }
}
