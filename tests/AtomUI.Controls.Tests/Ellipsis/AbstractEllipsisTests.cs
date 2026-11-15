using System.Globalization;
using AtomUI.Controls.Commons;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Presenters;
using Avalonia.Data;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Xunit;

namespace AtomUI.Controls.Tests.Ellipsis;

public class AbstractEllipsisTests
{
    private const string MixedText = "甲e\u0301😀👩‍💻乙丙東京漢字丁👨‍👩‍👧‍👦🙂o\u0308尾";
    private const string LongText = "alpha bravo charlie delta echo foxtrot golf hotel india juliet";

    private sealed class TestEllipsis : AbstractEllipsis
    {
        public Size Remeasure(double width) => base.MeasureOverride(new Size(width, double.PositiveInfinity));
    }

    [Fact]
    public void Defaults_and_short_text_have_no_action()
    {
        var ellipsis = new TestEllipsis();
        Assert.Null(ellipsis.Text);
        Assert.Equal(1, ellipsis.Rows);
        Assert.Equal(EllipsisDirection.End, ellipsis.Direction);
        Assert.Null(ellipsis.ExpandContent);
        Assert.Null(ellipsis.CollapseContent);
        Assert.False(ellipsis.IsExpanded);
        Assert.Equal(BindingMode.TwoWay, AbstractEllipsis.IsExpandedProperty.GetMetadata<AbstractEllipsis>().DefaultBindingMode);

        ellipsis.Rows = 0;
        ellipsis.ExpandContent = "more";
        ellipsis.CollapseContent = "less";
        Assert.Equal(default, Measure(ellipsis, 100));
        Assert.Empty(ellipsis.GetVisualChildren());
        Assert.Equal(0, ellipsis.TextLineCount);

        ellipsis.Text = "short";
        ellipsis.IsExpanded = true;
        Measure(ellipsis, 160);
        Assert.Equal("short", ellipsis.DisplayedText);
        Assert.Equal(1, ellipsis.TextLineCount);
        Assert.Empty(ellipsis.GetVisualChildren());

        ellipsis.IsExpanded = false;
        ellipsis.Text = "first\nsecond\nthird";
        Measure(ellipsis, double.PositiveInfinity);
        Assert.Equal(ellipsis.Text, ellipsis.DisplayedText);
        Assert.Empty(ellipsis.GetVisualChildren());

        ellipsis.ExpandContent = null;
        Measure(ellipsis, 100);
        Assert.Equal(1, ellipsis.TextLineCount);
        Assert.EndsWith("…", ellipsis.DisplayedText);
        Assert.Empty(ellipsis.GetVisualChildren());
    }

    [Theory]
    [InlineData(EllipsisDirection.Start)]
    [InlineData(EllipsisDirection.End)]
    [InlineData(EllipsisDirection.Middle)]
    public void All_directions_keep_complete_text_elements(EllipsisDirection direction)
    {
        var ellipsis = new TestEllipsis { Direction = direction, UseLayoutRounding = false };
        var samples = new[] { MixedText, LongText, "First line of text\nSecond line with 😀 words\nThird line" };
        foreach (var text in samples)
        {
            ellipsis.Text = text;
            for (var rows = 1; rows <= 2; rows++)
            {
                ellipsis.Rows = rows;
                var actionWidth = NaturalWidth(ellipsis, "+");
                ellipsis.ExpandContent = new Border { Width = actionWidth, Height = 10 };
                foreach (var width in new[]
                         {
                             NaturalWidth(ellipsis, "…甲😀"),
                             NaturalWidth(ellipsis, "bravo…"),
                             NaturalWidth(ellipsis, "alpha bravo…")
                         })
                {
                    Measure(ellipsis, width);
                    using var full = CreateLayout(ellipsis, text, width);
                    var expected = full.TextLines.Count <= rows
                        ? text
                        : FitByExhaustiveSearch(ellipsis, text, width, actionWidth);
                    Assert.Equal(expected, ellipsis.DisplayedText);
                    Assert.InRange(ellipsis.TextLineCount, 1, rows);
                }
            }
        }
    }

    [Theory]
    [InlineData(EllipsisDirection.Start)]
    [InlineData(EllipsisDirection.End)]
    [InlineData(EllipsisDirection.Middle)]
    public void A_longer_candidate_can_fit_after_wrapping(EllipsisDirection direction)
    {
        const char glyph = '界';
        var ellipsis = new TestEllipsis
        {
            Text = new string(glyph, 250),
            Direction = direction,
            Rows = 2,
            UseLayoutRounding = false
        };
        var width = NaturalWidth(ellipsis, new string(glyph, 100));
        var actionWidth = NaturalWidth(ellipsis, new string(glyph, 80));
        ellipsis.ExpandContent = new Border { Width = actionWidth, Height = 10 };

        var expected = FitByExhaustiveSearch(ellipsis, ellipsis.Text, width, actionWidth);
        Measure(ellipsis, width);

        Assert.True(expected.Length > 100, "the oracle should fit a candidate on the second line");
        Assert.Equal(expected, ellipsis.DisplayedText);
        Assert.Equal(2, ellipsis.TextLineCount);
    }

    [Fact]
    public void Action_width_changes_retained_text()
    {
        var ellipsis = new TestEllipsis { Text = MixedText, UseLayoutRounding = false, ExpandContent = "+" };
        var width = NaturalWidth(ellipsis, "甲e\u0301😀👩‍💻…++");
        Measure(ellipsis, width);
        var action = GetAction(ellipsis);
        var first = ellipsis.DisplayedText;

        ellipsis.ExpandContent = "++++";
        Measure(ellipsis, width);
        Assert.Same(action, GetAction(ellipsis));
        Assert.True(ellipsis.DisplayedText!.Length < first!.Length);
        Assert.Equal(FitByExhaustiveSearch(ellipsis, MixedText, width, action.DesiredSize.Width), ellipsis.DisplayedText);
    }

    [Fact]
    public void Collapse_action_moves_to_next_line_when_needed()
    {
        var ellipsis = new TestEllipsis { Text = LongText, UseLayoutRounding = false };
        var width = NaturalWidth(ellipsis, "alpha");
        var expand = new Border { Width = width * 2, Height = 10 };
        var collapse = new Border { Width = width * 2, Height = 10 };
        ellipsis.ExpandContent = expand;
        ellipsis.CollapseContent = collapse;
        var foldedSize = Measure(ellipsis, width);
        var presenter = GetAction(ellipsis);
        Assert.Equal("…", ellipsis.DisplayedText);
        Assert.Same(expand, presenter.Child);
        Assert.Equal(width * 2, presenter.Bounds.Width);
        using var marker = CreateLayout(ellipsis, "…", width);
        Assert.Equal((marker.Height - expand.Height) / 2, presenter.Bounds.Y, 5);

        ellipsis.IsExpanded = true;
        using var full = CreateLayout(ellipsis, LongText, width);
        var expandedSize = Measure(ellipsis, width);
        Assert.Same(presenter, GetAction(ellipsis));
        Assert.Same(collapse, presenter.Child);
        Assert.Equal(LongText, ellipsis.DisplayedText);
        Assert.Equal(0, presenter.Bounds.X, 5);
        Assert.True(presenter.Bounds.Y >= full.Height);
        Assert.True(expandedSize.Height > full.Height);
        Assert.True(expandedSize.Height > foldedSize.Height);
    }

    [Fact]
    public void Cache_tracks_layout_inputs_and_action_width()
    {
        var ellipsis = new TestEllipsis
        {
            Text = LongText,
            ExpandContent = new Border { Width = 12, Height = 10 },
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
            Width = 88
        };
        var host = new Panel { Children = { ellipsis } };
        var window = Show(host);
        using var hostLifetime = new WindowLifetime(window);
        var presenter = GetAction(ellipsis);
        var initial = ellipsis.DisplayedText;
        ellipsis.Remeasure(88);
        Assert.Same(initial, ellipsis.DisplayedText);

        var replacement = new Border { Width = 12, Height = 28 };
        ellipsis.ExpandContent = replacement;
        Update(window);
        Assert.Same(replacement, presenter.Child);
        Assert.Same(initial, ellipsis.DisplayedText);
        Assert.Equal(28, presenter.Bounds.Height);

        TextElement.SetFontSize(host, ellipsis.FontSize * 2);
        Update(window);
        Assert.True(ellipsis.DisplayedText!.Length < initial!.Length);
        var largeFont = ellipsis.DisplayedText;
        ellipsis.Width *= 2;
        Update(window);
        Assert.True(ellipsis.DisplayedText!.Length > largeFont!.Length);

        ellipsis.Text = "changed text " + LongText;
        Update(window);
        Assert.Equal(FitByExhaustiveSearch(ellipsis, ellipsis.Text, ellipsis.Width, presenter.DesiredSize.Width),
            ellipsis.DisplayedText);
        Assert.StartsWith("ch", ellipsis.DisplayedText);
        var beforePaint = ellipsis.DisplayedText;
        ellipsis.Foreground = Brushes.Red;
        Update(window);
        Assert.Same(beforePaint, ellipsis.DisplayedText);

        host.Children.Remove(ellipsis);
        Update(window);
        Assert.Null(ellipsis.DisplayedText);
        Assert.Empty(ellipsis.GetVisualChildren());
        Assert.Empty(ellipsis.GetLogicalChildren());
        Assert.Null(replacement.GetVisualParent());

        host.Children.Add(ellipsis);
        Update(window);
        Assert.Equal(beforePaint, ellipsis.DisplayedText);
        Assert.Same(replacement, GetAction(ellipsis).Child);
    }

    [Fact]
    public void Action_tap_preserves_binding_and_stops_bubbling()
    {
        var ellipsis = new TestEllipsis { Text = LongText, ExpandContent = "+", CollapseContent = "-", Width = 160 };
        var outer = new Border { Child = ellipsis };
        var window = Show(outer);
        using var hostLifetime = new WindowLifetime(window);
        var externalState = new CheckBox { IsChecked = false };
        using var binding = ellipsis.Bind(AbstractEllipsis.IsExpandedProperty,
            new Binding(nameof(CheckBox.IsChecked)) { Source = externalState });
        var expanded = 0;
        var content = 0;
        var outerTaps = 0;
        ellipsis.ExpandedChanged += (_, _) => expanded++;
        Point? tapPosition = null;
        ellipsis.ContentTapped += (_, e) => tapPosition = e.GetPosition(window);
        outer.AddHandler(AbstractEllipsis.ContentTappedEvent, (_, e) =>
        {
            content++;
            e.Handled = true;
        });
        outer.Tapped += (_, _) => outerTaps++;

        Click(window, GetAction(ellipsis));
        Assert.True(ellipsis.IsExpanded);
        Assert.True(externalState.IsChecked);
        Assert.Equal(1, expanded);
        Assert.Equal(0, content);
        Assert.Equal(0, outerTaps);

        externalState.IsChecked = false;
        Update(window);
        Assert.False(ellipsis.IsExpanded);
        Assert.Equal(2, expanded);
        var collapse = new Button { Content = "-" };
        ellipsis.CollapseContent = collapse;
        externalState.IsChecked = true;
        Update(window);
        var collapsePoint = collapse.TranslatePoint(new Rect(collapse.Bounds.Size).Center, window)!.Value;
        using (var finger = window.TouchBegin(collapsePoint))
        {
            window.TouchEnd(finger, collapsePoint);
        }
        Update(window);
        Assert.False(ellipsis.IsExpanded);
        Assert.False(externalState.IsChecked);
        Assert.Equal(4, expanded);
        Assert.Equal(0, content);
        Assert.Equal(0, outerTaps);

        var bodyPoint = ellipsis.TranslatePoint(new Point(3, ellipsis.FontSize / 2), window)!.Value;
        Click(window, bodyPoint);
        Assert.Equal(1, content);
        Assert.Equal(1, outerTaps);
        Assert.Equal(bodyPoint, tapPosition);
    }

    private static Size Measure(TestEllipsis ellipsis, double width)
    {
        ellipsis.Measure(new Size(width, double.PositiveInfinity));
        var desiredSize = ellipsis.DesiredSize;
        ellipsis.Arrange(new Rect(new Size(double.IsFinite(width) ? width : desiredSize.Width, desiredSize.Height)));
        return desiredSize;
    }

    private static TextLayout CreateLayout(TestEllipsis ellipsis, string text, double width) =>
        new(text, new Typeface(ellipsis.FontFamily, ellipsis.FontStyle, ellipsis.FontWeight, ellipsis.FontStretch),
            ellipsis.FontSize, textWrapping: TextWrapping.Wrap, maxWidth: width);

    private static double NaturalWidth(TestEllipsis ellipsis, string text)
    {
        using var layout = CreateLayout(ellipsis, text, double.PositiveInfinity);
        return layout.WidthIncludingTrailingWhitespace;
    }

    private static string FitByExhaustiveSearch(TestEllipsis ellipsis, string text, double width, double actionWidth)
    {
        var starts = StringInfo.ParseCombiningCharacters(text);
        var end = starts.Length;
        var max = ellipsis.Direction == EllipsisDirection.Middle ? end / 2 : end;
        for (var retain = max; retain >= 0; retain--)
        {
            var leading = retain == end ? text.Length : starts[retain];
            var trailing = retain == 0 ? text.Length : starts[end - retain];
            var candidate = ellipsis.Direction switch
            {
                EllipsisDirection.Start => "…" + text[trailing..],
                EllipsisDirection.End => text[..leading] + "…",
                _ => text[..leading] + "…" + text[trailing..]
            };
            using var layout = CreateLayout(ellipsis, candidate, width);
            if (layout.TextLines.Count <= ellipsis.Rows &&
                layout.TextLines[^1].WidthIncludingTrailingWhitespace + actionWidth <= width)
            {
                return candidate;
            }
        }
        return "…";
    }

    private static ContentPresenter GetAction(TestEllipsis ellipsis) =>
        Assert.IsType<ContentPresenter>(Assert.Single(ellipsis.GetVisualChildren()));

    private sealed class WindowLifetime(Window window) : IDisposable
    {
        public void Dispose() => window.Close();
    }

    private static Window Show(Control content)
    {
        var window = new Window { Width = 400, Height = 300, Content = content };
        window.Show();
        Update(window);
        return window;
    }

    private static void Update(Window window)
    {
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
        AvaloniaHeadlessPlatform.ForceRenderTimerTick();
        Dispatcher.UIThread.RunJobs();
    }

    private static void Click(Window window, Visual target) =>
        Click(window, target.TranslatePoint(new Rect(target.Bounds.Size).Center, window)!.Value);

    private static void Click(Window window, Point point)
    {
        window.MouseDown(point, MouseButton.Left);
        window.MouseUp(point, MouseButton.Left);
        Update(window);
    }
}
