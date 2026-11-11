using AtomUI.Controls;
using Avalonia;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;
using AvaloniaTextBox = Avalonia.Controls.TextBox;

namespace AtomUI.Desktop.Controls.Tests.AutoComplete;

public class AutoCompleteFocusTests
{
    static AutoCompleteFocusTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Filled_AutoComplete_Keeps_Focus_Style_After_First_Click()
    {
        var autoComplete = new AtomUI.Desktop.Controls.AutoComplete
        {
            Width           = 240,
            IsMotionEnabled = false,
            StyleVariant    = InputControlStyleVariant.Filled
        };

        var window = new AvaloniaWindow
        {
            Width   = 360,
            Height  = 160,
            Content = autoComplete
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var textBox = autoComplete.GetVisualDescendants()
                                      .OfType<AvaloniaTextBox>()
                                      .Single();
            var contentFrame = autoComplete.GetVisualDescendants()
                                           .OfType<global::AtomUI.Desktop.Controls.AddOnDecoratedBoxContentFrame>()
                                           .Single(x => x.Name == "PART_ContentFrame");
            var clickPoint = textBox.TranslatePoint(
                new Point(textBox.Bounds.Width / 2, textBox.Bounds.Height / 2),
                window);

            clickPoint.ShouldNotBeNull();

            window.MouseMove(clickPoint.Value);
            window.MouseDown(clickPoint.Value, MouseButton.Left);
            window.MouseUp(clickPoint.Value, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();

            textBox.IsFocused.ShouldBeTrue();
            autoComplete.IsKeyboardFocusWithin.ShouldBeTrue();
            GetSolidColor(contentFrame.Background).ShouldBe(
                Colors.Transparent,
                "Filled AutoComplete should keep the focus background after the first click.");
            GetSolidColor(contentFrame.BorderBrush).ShouldNotBe(
                Colors.Transparent,
                "Filled AutoComplete should show the focus border after the first click.");
            // Ant Design: only the outlined variant draws the focus outline shadow.
            contentFrame.BoxShadow.Count.ShouldBe(
                0,
                "Filled AutoComplete should not render a focus outline shadow.");
        }
        finally
        {
            window.Close();
        }
    }

    private static Color GetSolidColor(IBrush? brush)
    {
        if (brush is not ISolidColorBrush solidColorBrush)
        {
            throw new InvalidOperationException($"Expected a solid color brush, but got {brush?.GetType().Name ?? "null"}.");
        }

        return solidColorBrush.Color;
    }
}
