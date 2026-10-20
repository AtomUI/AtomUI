using AtomUI.Desktop.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUIColorPicker = AtomUI.Desktop.Controls.ColorPicker;
using AtomUIGradientColorPicker = AtomUI.Desktop.Controls.GradientColorPicker;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUIGallery.Tests.Controls;

public class ColorPickerCustomSizeTests
{
    static ColorPickerCustomSizeTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void ColorPicker_Custom_SizeType_Keeps_Preview_Inset_When_Padding_Is_Set()
    {
        AssertCustomColorIndicatorUsesFixedInset(
            new AtomUIColorPicker(),
            new AtomUIColorPicker());
    }

    [Fact]
    public void GradientColorPicker_Custom_SizeType_Keeps_Preview_Inset_When_Padding_Is_Set()
    {
        AssertCustomColorIndicatorUsesFixedInset(
            new AtomUIGradientColorPicker(),
            new AtomUIGradientColorPicker());
    }

    [Fact]
    public void ColorPicker_Color_Indicator_Returns_To_Middle_Size_When_Custom_Height_Is_Cleared()
    {
        var middle = new AtomUIColorPicker();
        var custom = new AtomUIColorPicker
        {
            SizeType = CustomizableSizeType.Custom,
            Width    = 48,
            Height   = 48,
            Padding  = new Thickness(8)
        };
        var host = new StackPanel
        {
            Children =
            {
                middle,
                custom
            }
        };

        ShowInWindow(host, () =>
        {
            var middleIndicator = FindColorIndicator(middle);
            var customIndicator = FindColorIndicator(custom);

            custom.Height = double.NaN;
            Dispatcher.UIThread.RunJobs();

            customIndicator.Bounds.Width.ShouldBe(middleIndicator.Bounds.Width, 0.001);
            customIndicator.Bounds.Height.ShouldBe(middleIndicator.Bounds.Height, 0.001);
        });
    }

    private static void AssertCustomColorIndicatorUsesFixedInset(AbstractColorPicker expected, AbstractColorPicker actual)
    {
        expected.SizeType = CustomizableSizeType.Custom;
        expected.Width    = 48;
        expected.Height   = 48;
        actual.SizeType   = CustomizableSizeType.Custom;
        actual.Width      = 48;
        actual.Height     = 48;
        actual.Padding    = new Thickness(10, 6);
        var host = new StackPanel
        {
            Children =
            {
                expected,
                actual
            }
        };

        ShowInWindow(host, () =>
        {
            var expectedIndicator = FindColorIndicator(expected);
            var actualIndicator   = FindColorIndicator(actual);
            var expectedOffset    = GetOffsetFromFrame(expected, expectedIndicator);
            var actualOffset      = GetOffsetFromFrame(actual, actualIndicator);

            actualIndicator.Bounds.Width.ShouldBe(expectedIndicator.Bounds.Width, 0.001);
            actualIndicator.Bounds.Height.ShouldBe(expectedIndicator.Bounds.Height, 0.001);
            actualOffset.X.ShouldBe(expectedOffset.X, 0.001);
            actualOffset.Y.ShouldBe(expectedOffset.Y, 0.001);
        });
    }

    private static Vector GetOffsetFromFrame(Control picker, Control indicator)
    {
        var frame = picker.GetVisualDescendants()
                          .OfType<Control>()
                          .Single(part => part.Name == "PART_Frame");
        var frameOrigin     = frame.TranslatePoint(new Point(0, 0), picker);
        var indicatorOrigin = indicator.TranslatePoint(new Point(0, 0), picker);

        frameOrigin.ShouldNotBeNull();
        indicatorOrigin.ShouldNotBeNull();
        return indicatorOrigin.Value - frameOrigin.Value;
    }

    private static Control FindColorIndicator(Control control)
    {
        return control.GetVisualDescendants()
                      .OfType<Control>()
                      .Single(part => part.Name == "PART_ColorIndicator");
    }

    private static void ShowInWindow(Control content, Action assertion)
    {
        var window = new AvaloniaWindow
        {
            Width   = 160,
            Height  = 120,
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
}
