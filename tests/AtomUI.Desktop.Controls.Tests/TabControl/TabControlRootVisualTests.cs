using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AtomUI.Controls.Primitives;
using Shouldly;
using Xunit;
using AtomUICardTabControl = AtomUI.Desktop.Controls.CardTabControl;
using AtomUITabControl = AtomUI.Desktop.Controls.TabControl;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.TabControl;

public class TabControlRootVisualTests
{
    static TabControlRootVisualTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void BorderDash_Properties_Are_Exposed_With_Expected_Defaults()
    {
        var tabControl = new AtomUITabControl();

        tabControl.BorderDashArray.ShouldBeNull();
        tabControl.BorderDashOffset.ShouldBe(0d);

        tabControl.BorderDashArray = new AvaloniaList<double> { 4, 2 };
        tabControl.BorderDashArray.ShouldBe(new double[] { 4, 2 });
        tabControl.BorderDashOffset = 1.5;
        tabControl.BorderDashOffset.ShouldBe(1.5d);
    }

    [Fact]
    public void Default_Theme_Leaves_Public_Border_Properties_Unset_And_Themes_The_Separator()
    {
        var tabControl = new AtomUITabControl();
        using (Show(tabControl))
        {
            tabControl.BorderThickness.ShouldBe(default(Thickness));
            tabControl.BorderBrush.ShouldBeNull();
            tabControl.SeparatorBorderThickness.ShouldNotBe(default(Thickness));
            tabControl.SeparatorBorderThickness.Left.ShouldBeGreaterThan(0);
            tabControl.SeparatorBorderBrush.ShouldNotBeNull();
        }
    }

    [Fact]
    public void Root_Frame_TemplateBinds_Control_Visual_Properties()
    {
        var tabControl = new AtomUITabControl
        {
            Background       = Brushes.Red,
            BorderBrush      = Brushes.Blue,
            BorderThickness  = new Thickness(2),
            CornerRadius     = new CornerRadius(4),
            Padding          = new Thickness(16),
            BorderDashArray  = new AvaloniaList<double> { 4, 2 },
            BorderDashOffset = 1.5
        };

        using (Show(tabControl))
        {
            var frame = tabControl.GetVisualChildren().OfType<PixelAlignedBorder>()
                                  .Single(static candidate => candidate.Name == "Frame");
            frame.Background.ShouldBeSameAs(tabControl.Background);
            frame.BorderBrush.ShouldBeSameAs(tabControl.BorderBrush);
            frame.BorderThickness.ShouldBe(new Thickness(2));
            frame.CornerRadius.ShouldBe(new CornerRadius(4));
            frame.Padding.ShouldBe(new Thickness(16));
            frame.StrokeDashArray.ShouldBe(new double[] { 4, 2 });
            frame.StrokeDaskOffset.ShouldBe(1.5d);
        }
    }

    [Fact]
    public void CardTabControl_Root_Frame_TemplateBinds_Control_Visual_Properties()
    {
        var cardTabControl = new AtomUICardTabControl
        {
            Background      = Brushes.Beige,
            BorderBrush     = Brushes.Teal,
            BorderThickness = new Thickness(2),
            CornerRadius    = new CornerRadius(4),
            Padding         = new Thickness(16),
            BorderDashArray = new AvaloniaList<double> { 4, 2 }
        };

        using (Show(cardTabControl))
        {
            var frame = cardTabControl.GetVisualChildren().OfType<PixelAlignedBorder>()
                                      .Single(static candidate => candidate.Name == "Frame");
            frame.Background.ShouldBeSameAs(cardTabControl.Background);
            frame.BorderBrush.ShouldBeSameAs(cardTabControl.BorderBrush);
            frame.BorderThickness.ShouldBe(new Thickness(2));
            frame.CornerRadius.ShouldBe(new CornerRadius(4));
            frame.Padding.ShouldBe(new Thickness(16));
            frame.StrokeDashArray.ShouldBe(new double[] { 4, 2 });
        }
    }

    private static IDisposable Show(Control content)
    {
        var window = new AvaloniaWindow { Width = 640, Height = 260, Content = content };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return new WindowLifetime(window);
    }

    private sealed class WindowLifetime(AvaloniaWindow window) : IDisposable
    {
        public void Dispose()
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }
}
