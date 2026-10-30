using AtomUI.Controls;
using AtomUI.Controls.Primitives;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Headless;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Input;

public class LineEditBorderRenderTests
{
    static LineEditBorderRenderTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Outlined_LineEdit_AddOn_Frames_Keep_Their_Border_Segments_At_Fractional_Scale()
    {
        var lineEdit = new LineEdit
        {
            Width      = 300,
            Height     = 40,
            LeftAddOn  = "https://",
            RightAddOn = ".com",
            IsMotionEnabled = false
        };
        var host = new AvaloniaWindow
        {
            Width   = 360,
            Height  = 100,
            Content = lineEdit
        };

        try
        {
            host.SetRenderScaling(5d / 3d);
            host.Show();
            lineEdit.ApplyTemplate();
            host.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            var frames = lineEdit.GetVisualDescendants()
                                 .OfType<PixelAlignedBorder>()
                                 .Where(border => border.Name is
                                     "PART_LeftAddOn" or "PART_ContentFrame" or "PART_RightAddOn")
                                 .ToDictionary(border => border.Name!);
            var left    = frames["PART_LeftAddOn"];
            var content = frames["PART_ContentFrame"];
            var right   = frames["PART_RightAddOn"];
            var expectedBorderBrush = GetThemeResource<IBrush>(SharedTokenKind.ColorBorder);
            var expectedPadding = GetThemeResource<Thickness>(SharedTokenKind.InputPadding);

            left.StyleKey.ShouldBe(left.GetType());
            content.StyleKey.ShouldBe(content.GetType());
            right.StyleKey.ShouldBe(right.GetType());
            BrushShouldHaveSameColor(left.BorderBrush, expectedBorderBrush);
            BrushShouldHaveSameColor(content.BorderBrush, expectedBorderBrush);
            BrushShouldHaveSameColor(right.BorderBrush, expectedBorderBrush);
            left.BorderThickness.ShouldBe(new Thickness(1, 1, 0, 1));
            content.BorderThickness.ShouldBe(new Thickness(1));
            right.BorderThickness.ShouldBe(new Thickness(0, 1, 1, 1));
            left.Padding.ShouldBe(expectedPadding);
            content.Padding.ShouldBe(expectedPadding);
            right.Padding.ShouldBe(expectedPadding);

            var physicalPixel = 1 / host.RenderScaling;
            Math.Abs(left.Bounds.Right - content.Bounds.Left).ShouldBeLessThanOrEqualTo(physicalPixel + 0.001);
            Math.Abs(content.Bounds.Right - right.Bounds.Left).ShouldBeLessThanOrEqualTo(physicalPixel + 0.001);
            left.Bounds.Height.ShouldBe(content.Bounds.Height, 0.001);
            right.Bounds.Height.ShouldBe(content.Bounds.Height, 0.001);

            lineEdit.Status = InputControlStatus.Error;
            Dispatcher.UIThread.RunJobs();

            var errorBrush = GetThemeResource<IBrush>(SharedTokenKind.ColorError);
            BrushShouldHaveSameColor(left.BorderBrush, errorBrush);
            BrushShouldHaveSameColor(content.BorderBrush, errorBrush);
            BrushShouldHaveSameColor(right.BorderBrush, errorBrush);

            lineEdit.Status       = InputControlStatus.Default;
            lineEdit.StyleVariant = InputControlStyleVariant.Filled;
            Dispatcher.UIThread.RunJobs();

            var filledBrush = GetThemeResource<IBrush>(SharedTokenKind.ColorFillTertiary);
            BrushShouldHaveSameColor(left.Background, filledBrush);
            BrushShouldHaveSameColor(left.BorderBrush, filledBrush);
            BrushShouldHaveSameColor(right.Background, filledBrush);
            BrushShouldHaveSameColor(right.BorderBrush, filledBrush);

            lineEdit.StyleVariant = InputControlStyleVariant.Outlined;
            lineEdit.IsEnabled    = false;
            Dispatcher.UIThread.RunJobs();

            var disabledBackground = GetThemeResource<IBrush>(SharedTokenKind.ColorBgContainerDisabled);
            BrushShouldHaveSameColor(left.Background, disabledBackground);
            BrushShouldHaveSameColor(left.BorderBrush, expectedBorderBrush);
            BrushShouldHaveSameColor(right.Background, disabledBackground);
            BrushShouldHaveSameColor(right.BorderBrush, expectedBorderBrush);
        }
        finally
        {
            host.Close();
        }
    }

    private static T GetThemeResource<T>(object key)
    {
        var application = Application.Current;
        application.ShouldNotBeNull();
        application!.TryGetResource(key, application.ActualThemeVariant, out var value).ShouldBeTrue();
        value.ShouldBeAssignableTo<T>();
        return (T)value!;
    }

    private static void BrushShouldHaveSameColor(IBrush? actual, IBrush? expected)
    {
        GetSolidBrushColor(actual).ShouldBe(GetSolidBrushColor(expected));
    }

    private static Color GetSolidBrushColor(IBrush? brush)
    {
        brush.ShouldNotBeNull();
        brush.ShouldBeAssignableTo<ISolidColorBrush>();
        return ((ISolidColorBrush)brush!).Color;
    }
}
