using AtomUI.Controls;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using Shouldly;
using Xunit;
using AtomUIButton = AtomUI.Desktop.Controls.Button;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Icons;

public class IconPresenterBrushTests
{
    static IconPresenterBrushTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void IconPresenter_Synchronizes_IconBrush_To_All_Icon_Brush_Slots()
    {
        var icon      = new MultiBrushTestIcon();
        var iconBrush = new SolidColorBrush(Colors.Orange);
        var presenter = new IconPresenter
        {
            Width     = 16,
            Height    = 16,
            Icon      = icon,
            IconBrush = iconBrush
        };

        ShowInWindow(presenter, () =>
        {
            AssertIconBrushes(icon, iconBrush);

            var nextBrush = new SolidColorBrush(Colors.DeepSkyBlue);
            presenter.IconBrush = nextBrush;
            Dispatcher.UIThread.RunJobs();

            AssertIconBrushes(icon, nextBrush);
        });
    }

    [Fact]
    public void Button_Synchronizes_Foreground_To_All_Icon_Brush_Slots()
    {
        var icon = new MultiBrushTestIcon();
        var button = new AtomUIButton
        {
            Content         = "Click me",
            Icon            = icon,
            IsMotionEnabled = false
        };

        ShowInWindow(button, () =>
        {
            AssertIconBrushColors(icon, button.Foreground);

            SetPseudoClass(button, StdPseudoClass.PointerOver, true);
            Dispatcher.UIThread.RunJobs();

            AssertIconBrushColors(icon, button.Foreground);
        });
    }

    private static void AssertIconBrushes(MultiBrushTestIcon icon, IBrush expectedBrush)
    {
        icon.StrokeBrush.ShouldBeSameAs(expectedBrush);
        icon.FillBrush.ShouldBeSameAs(expectedBrush);
        icon.SecondaryStrokeBrush.ShouldBeSameAs(expectedBrush);
        icon.SecondaryFillBrush.ShouldBeSameAs(expectedBrush);
        icon.FallbackBrush.ShouldBeSameAs(expectedBrush);
    }

    private static void AssertIconBrushColors(MultiBrushTestIcon icon, IBrush? expectedBrush)
    {
        var expectedColor = GetSolidBrushColor(expectedBrush);
        GetSolidBrushColor(icon.StrokeBrush).ShouldBe(expectedColor);
        GetSolidBrushColor(icon.FillBrush).ShouldBe(expectedColor);
        GetSolidBrushColor(icon.SecondaryStrokeBrush).ShouldBe(expectedColor);
        GetSolidBrushColor(icon.SecondaryFillBrush).ShouldBe(expectedColor);
        GetSolidBrushColor(icon.FallbackBrush).ShouldBe(expectedColor);
    }

    private static Color GetSolidBrushColor(IBrush? brush)
    {
        brush.ShouldNotBeNull();
        brush.ShouldBeAssignableTo<ISolidColorBrush>();
        return ((ISolidColorBrush)brush!).Color;
    }

    private static void SetPseudoClass(Control control, string pseudoClass, bool value)
    {
        ((IPseudoClasses)control.Classes).Set(pseudoClass, value);
    }

    private static void ShowInWindow(Control content, Action assertion)
    {
        var window = new AvaloniaWindow
        {
            Width   = 320,
            Height  = 240,
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
        }
    }

    private sealed class MultiBrushTestIcon : AtomUI.Controls.Icon
    {
    }
}
