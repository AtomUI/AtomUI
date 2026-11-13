using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaGrid = Avalonia.Controls.Grid;
using AvaloniaScrollViewer = Avalonia.Controls.ScrollViewer;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Input;

public class TextAreaLinesLayoutTests
{
    private static readonly string LongText =
        string.Join("\n", Enumerable.Range(1, 60).Select(index => $"Line {index:00}"));

    static TextAreaLinesLayoutTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Viewport_Fills_Star_Row_When_Height_Is_Not_Set()
    {
        var textArea = CreateTextArea(LongText);
        var host     = new AvaloniaGrid { RowDefinitions = new RowDefinitions("Auto,*") };
        host.Children.Add(new Border { Height = 20 });
        AvaloniaGrid.SetRow(textArea, 1);
        host.Children.Add(textArea);
        var window = CreateWindow(host);

        try
        {
            textArea.Bounds.Height.ShouldBe(380, 0.5);
            AssertViewportFillsFrame(textArea);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Viewport_Fills_Flex_Grow_Slot_When_Height_Is_Not_Set()
    {
        var textArea = CreateTextArea(LongText);
        Flex.SetGrow(textArea, 1);
        var host = new FlexPanel { Direction = FlexDirection.Column };
        host.Children.Add(new Border { Height = 20 });
        host.Children.Add(textArea);
        var window = CreateWindow(host);

        try
        {
            textArea.Bounds.Height.ShouldBe(380, 0.5);
            AssertViewportFillsFrame(textArea);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Natural_Height_Follows_Lines_Not_Content()
    {
        var shortArea = CreateTextArea("short");
        var longArea  = CreateTextArea(LongText);
        var emptyArea = CreateTextArea(string.Empty);
        var fourLines = CreateTextArea(LongText);
        fourLines.Lines = 4;
        var host = new StackPanel { Children = { shortArea, longArea, emptyArea, fourLines } };
        var window = CreateWindow(host);

        try
        {
            longArea.Bounds.Height.ShouldBe(shortArea.Bounds.Height, 0.5);
            emptyArea.Bounds.Height.ShouldBe(shortArea.Bounds.Height, 0.5);

            var lineHeight = FindPart<TextPresenter>(longArea).TextLayout.TextLines[0].Height;
            FindPart<AvaloniaScrollViewer>(longArea).Viewport.Height.ShouldBe(lineHeight * 2, 1);
            FindPart<AvaloniaScrollViewer>(fourLines).Viewport.Height.ShouldBe(lineHeight * 4, 1);
        }
        finally
        {
            window.Close();
        }
    }

    private static void AssertViewportFillsFrame(AtomUI.Desktop.Controls.TextArea textArea)
    {
        var frame     = textArea.GetVisualDescendants().OfType<Control>().Single(item => item.Name == "PART_AddOnDecoratedBox");
        var viewer    = FindPart<AvaloniaScrollViewer>(textArea);
        var presenter = FindPart<TextPresenter>(textArea);

        viewer.Viewport.Height.ShouldBeGreaterThan(frame.Bounds.Height * 0.9);
        var firstLineTop = presenter.TranslatePoint(new Point(0, 0), frame)!.Value.Y;
        firstLineTop.ShouldBeLessThan(16);
    }

    private static AtomUI.Desktop.Controls.TextArea CreateTextArea(string text)
    {
        return new AtomUI.Desktop.Controls.TextArea
        {
            Text            = text,
            IsMotionEnabled = false
        };
    }

    private static T FindPart<T>(Control control)
        where T : Control
    {
        return control.GetVisualDescendants().OfType<T>().First();
    }

    private static AvaloniaWindow CreateWindow(Control content)
    {
        var window = new AvaloniaWindow
        {
            Width   = 360,
            Height  = 400,
            Content = content
        };

        window.Show();
        Dispatcher.UIThread.RunJobs();
        return window;
    }
}
