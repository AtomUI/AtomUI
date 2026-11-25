using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.DataGrid.Theme;

public class DataGridRowExpanderFocusTests
{
    static DataGridRowExpanderFocusTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    // Ant Design's table expand icon shows focus by recoloring its border; no outline is added on top.
    [Fact]
    public void Keyboard_Focus_Recolors_The_Expander_Without_An_Outline()
    {
        var expander = new DataGridRowExpander();
        var window = new Window
        {
            Width   = 200,
            Height  = 120,
            Content = new VisualLayerManager
            {
                Child = new StackPanel { Margin = new Thickness(20), Children = { expander } }
            }
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            var idleBorder = (expander.BorderBrush as ISolidColorBrush)?.Color;

            expander.Focus(NavigationMethod.Tab).ShouldBeTrue();
            Dispatcher.UIThread.RunJobs();

            var focusedBorder = (expander.BorderBrush as ISolidColorBrush)?.Color;
            focusedBorder.ShouldNotBeNull();
            focusedBorder.ShouldNotBe(idleBorder);
            var adornerLayer = AdornerLayer.GetAdornerLayer(expander);
            adornerLayer.ShouldNotBeNull();
            adornerLayer!.Children.ShouldBeEmpty();
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }
}
