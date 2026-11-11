using AtomUI.Controls.Commons;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Styling;
using Avalonia.Threading;
using Shouldly;
using Xunit;

namespace AtomUI.Controls.Tests.Skeleton;

public class SkeletonSinkTests
{
    static SkeletonSinkTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    private sealed class TestParagraph : AbstractSkeletonParagraph
    {
        public readonly Dictionary<Control, Dimension?> AppliedWidths = new();

        public TestParagraph()
        {
            Theme = new ControlTheme(typeof(TestParagraph))
            {
                Setters =
                {
                    new Setter(TemplateProperty, new FuncControlTemplate<TestParagraph>((_, scope) =>
                    {
                        var layout = new StackPanel { Name = LineLayoutPart };
                        return layout.RegisterInNameScope(scope);
                    }))
                }
            };
        }

        public Panel Layout => (Panel)this.GetTemplateDescendants().OfType<Control>().Single(c => c.Name == LineLayoutPart);

        public IReadOnlyList<Border> Lines => Layout.Children.Cast<Border>().ToList();

        protected override Control CreateLine()
        {
            return new Border();
        }

        protected override void ApplyLineWidth(Control line, Dimension? width)
        {
            AppliedWidths[line] = width;
        }

        protected override void ApplyLineState(Control line)
        {
            ((Border)line).Background = Background;
        }
    }

    private sealed class TestCompositeSkeleton : AbstractCompositeSkeleton
    {
    }

    private sealed class TestLine : AbstractSkeletonLine
    {
    }

    private static Window Host(Control content)
    {
        var window = new Window { Width = 400, Height = 200, Content = content };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
        return window;
    }

    private static Dimension Percent(double value)
    {
        return new Dimension(value, DimensionUnitType.Percentage);
    }

    [Fact]
    public void Paragraph_factory_builds_rows_with_widths()
    {
        var paragraph = new TestParagraph
        {
            Rows          = 3,
            LineWidths    = new List<Dimension> { Percent(40) },
            LastLineWidth = Percent(65)
        };
        var window = Host(paragraph);
        try
        {
            var lines = paragraph.Lines;
            lines.Count.ShouldBe(3);
            paragraph.AppliedWidths[lines[0]].ShouldBe(Percent(40));
            paragraph.AppliedWidths[lines[1]].ShouldBeNull();
            paragraph.AppliedWidths[lines[2]].ShouldBe(Percent(65));

            paragraph.Rows = 5;
            Dispatcher.UIThread.RunJobs();

            var rebuilt = paragraph.Lines;
            rebuilt.Count.ShouldBe(5);
            paragraph.AppliedWidths[rebuilt[0]].ShouldBe(Percent(40));
            paragraph.AppliedWidths[rebuilt[1]].ShouldBeNull();
            paragraph.AppliedWidths[rebuilt[3]].ShouldBeNull();
            paragraph.AppliedWidths[rebuilt[4]].ShouldBe(Percent(65));
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Paragraph_width_and_state_changes_update_existing_lines()
    {
        var paragraph = new TestParagraph { Rows = 3 };
        var window    = Host(paragraph);
        try
        {
            var original  = paragraph.Lines.ToList();

            paragraph.LastLineWidth = Percent(30);
            paragraph.LineWidths    = new List<Dimension> { Percent(80), Percent(70) };
            paragraph.Background    = Avalonia.Media.Brushes.Red;
            Dispatcher.UIThread.RunJobs();

            var lines = paragraph.Lines;
            lines.Count.ShouldBe(3);
            for (var i = 0; i < 3; i++)
            {
                lines[i].ShouldBeSameAs(original[i]);
            }
            paragraph.AppliedWidths[lines[0]].ShouldBe(Percent(80));
            paragraph.AppliedWidths[lines[1]].ShouldBe(Percent(70));
            paragraph.AppliedWidths[lines[2]].ShouldBe(Percent(30));
            lines.ShouldAllBe(line => line.Background == Avalonia.Media.Brushes.Red);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Paragraph_rejects_rows_below_one()
    {
        var paragraph = new TestParagraph();
        Should.Throw<ArgumentException>(() => paragraph.Rows = 0);
    }

    [Fact]
    public void Shared_defaults_match_the_desktop_contract()
    {
        var paragraph = new TestParagraph();
        paragraph.Rows.ShouldBe(2);
        paragraph.LastLineWidth.ShouldBe(Percent(100));
        paragraph.LineWidths.ShouldBeNull();
        paragraph.IsRound.ShouldBeFalse();

        var line = new TestLine();
        line.LineWidth.ShouldBe(Percent(100));
        line.IsRound.ShouldBeFalse();

        var composite = new TestCompositeSkeleton();
        composite.IsLoading.ShouldBeFalse();
        composite.IsShowAvatar.ShouldBeFalse();
        composite.IsShowTitle.ShouldBeTrue();
        composite.IsShowParagraph.ShouldBeTrue();
        composite.TitleWidth.ShouldBe(Percent(50));
        composite.ParagraphRows.ShouldBe(2);
        composite.ParagraphLastLineWidth.ShouldBe(Percent(61));
        composite.ParagraphLineWidths.ShouldBeNull();
        composite.AvatarShape.ShouldBe(AvatarShape.Circle);
        double.IsNaN(composite.AvatarSize).ShouldBeTrue();
        composite.IsContentVisible.ShouldBeTrue();
    }

    [Fact]
    public void Composite_content_visibility_follows_is_loading()
    {
        var composite = new TestCompositeSkeleton { IsLoading = true };
        composite.IsContentVisible.ShouldBeFalse();
        composite.IsLoading = false;
        composite.IsContentVisible.ShouldBeTrue();
    }

    [Fact]
    public void Composite_content_is_maintained_as_logical_child()
    {
        var composite = new TestCompositeSkeleton();
        var first     = new Border();
        var second    = new Border();

        composite.Content = first;
        ((Avalonia.LogicalTree.ILogical)composite).LogicalChildren.ShouldContain(first);

        composite.Content = second;
        var children = ((Avalonia.LogicalTree.ILogical)composite).LogicalChildren;
        children.ShouldContain(second);
        children.ShouldNotContain(first);
    }
}
