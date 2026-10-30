using AtomUI.Controls.Primitives;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;
using AtomUISplitter = AtomUI.Desktop.Controls.Splitter;

namespace AtomUI.Desktop.Controls.Tests.Splitter;

public class SplitterBorderDashTests
{
    static SplitterBorderDashTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Splitter_Border_Dash_Style_Forwards_To_Root_Frame()
    {
        var splitter = new AtomUISplitter
        {
            BorderBrush      = new SolidColorBrush(Color.Parse("#E0000000")),
            BorderThickness  = new Thickness(2),
            BorderDashArray  = new double[] { 4, 2 },
            BorderDashOffset = 2
        };

        splitter.Children.Add(new Border { Background = Brushes.White });
        splitter.Children.Add(new Border { Background = Brushes.White });

        var window = new AvaloniaWindow
        {
            Width   = 480,
            Height  = 320,
            Content = splitter
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var frame = splitter.GetVisualDescendants()
                                .OfType<PixelAlignedBorder>()
                                .Single();
            frame.BorderBrush.ShouldBe(splitter.BorderBrush);
            frame.BorderThickness.ShouldBe(new Thickness(2));
            frame.StrokeDashArray.ShouldNotBeNull();
            frame.StrokeDashArray.ShouldBe(new double[] { 4, 2 }, ignoreOrder: false);
            frame.StrokeDaskOffset.ShouldBe(2);
        }
        finally
        {
            window.Close();
        }
    }
}
