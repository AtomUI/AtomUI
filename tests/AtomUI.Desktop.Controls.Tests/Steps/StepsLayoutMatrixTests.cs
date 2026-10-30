using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Headless;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUISizeType = AtomUI.SizeType;
using AvaloniaTextBlock = Avalonia.Controls.TextBlock;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Steps;

public class StepsLayoutMatrixTests
{
    static StepsLayoutMatrixTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    public static IEnumerable<object[]> LayoutTypes
    {
        get
        {
            foreach (var type in Enum.GetValues<Desktop.Controls.StepsType>())
            {
                yield return [type];
            }
        }
    }

    [Theory]
    [MemberData(nameof(LayoutTypes))]
    public void Every_Layout_Combination_Uses_One_Finite_Semantic_Tree(Desktop.Controls.StepsType type)
    {
        foreach (var orientation in Enum.GetValues<Orientation>())
        foreach (var titlePlacement in Enum.GetValues<Orientation>())
        foreach (var sizeType in new[] { AtomUISizeType.Middle, AtomUISizeType.Small })
        {
            try
            {
                AssertLayoutCombination(type, orientation, titlePlacement, sizeType);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(
                    $"Steps layout failed for type={type}, orientation={orientation}, titlePlacement={titlePlacement}, sizeType={sizeType}.",
                    exception);
            }
        }
    }

    private static void AssertLayoutCombination(
        Desktop.Controls.StepsType type,
        Orientation orientation,
        Orientation titlePlacement,
        AtomUISizeType sizeType)
    {
        var steps = CreateSteps(type, orientation, titlePlacement, sizeType);

        ShowInWindow(steps, () =>
        {
            foreach (var item in steps.Items.Cast<Desktop.Controls.StepsItem>())
            {
                var indicator = item.GetVisualDescendants()
                                    .OfType<Desktop.Controls.StepsItemIndicator>()
                                    .Single();
                var header = FindControl(item, "HeaderPresenter");
                var subHeader = FindControl(item, "SubHeaderPresenter");
                var content = FindControl(item, "ContentPresenter");
                var connector = FindControl(item, "Connector");
                var arrow = FindControl(item, "NavigationArrow");
                var panelArrow = FindControl(item, "PanelArrow");

                AssertFinite(indicator);
                AssertFinite(header);
                AssertFinite(subHeader);
                AssertFinite(content);
                AssertFinite(connector);
                AssertFinite(arrow);
                AssertFinite(panelArrow);

                indicator.IsVisible.ShouldBe(type != Desktop.Controls.StepsType.Panel);
                if (type != Desktop.Controls.StepsType.Panel)
                {
                    indicator.Bounds.Width.ShouldBeGreaterThan(0);
                    indicator.Bounds.Height.ShouldBeGreaterThan(0);
                }
                AssertNoInteriorOverlap(indicator, header, item);
                AssertNoInteriorOverlap(indicator, subHeader, item);
                AssertNoInteriorOverlap(indicator, content, item);
                AssertNoInteriorOverlap(header, content, item);
                AssertNoInteriorOverlap(subHeader, content, item);

                content.IsVisible.ShouldBe(type != Desktop.Controls.StepsType.Inline);
                connector.IsVisible.ShouldBe(
                    type != Desktop.Controls.StepsType.Navigation &&
                    type != Desktop.Controls.StepsType.Panel &&
                    !item.IsLast);
                arrow.IsVisible.ShouldBe(type == Desktop.Controls.StepsType.Navigation && !item.IsLast);
                panelArrow.IsVisible.ShouldBe(type == Desktop.Controls.StepsType.Panel && !item.IsLast);
            }
        });
    }

    [Fact]
    public void Runtime_Type_Orientation_TitlePlacement_And_Size_Changes_Rearrange_The_Same_Nodes()
    {
        var steps = CreateSteps(
            Desktop.Controls.StepsType.Default,
            Orientation.Horizontal,
            Orientation.Horizontal,
            AtomUISizeType.Middle);

        ShowInWindow(steps, () =>
        {
            var item = steps.Items[0].ShouldBeOfType<Desktop.Controls.StepsItem>();
            var indicator = item.GetVisualDescendants()
                                .OfType<Desktop.Controls.StepsItemIndicator>()
                                .Single();
            var header = FindControl(item, "HeaderPresenter");
            var connector = FindControl(item, "Connector");
            var arrow = FindControl(item, "NavigationArrow");
            var middleWidth = indicator.Bounds.Width;

            GetBoundsInItem(header, item).X.ShouldBeGreaterThanOrEqualTo(indicator.Bounds.Right);

            steps.Type = Desktop.Controls.StepsType.Dot;
            Relayout();
            GetBoundsInItem(header, item).Y.ShouldBeGreaterThanOrEqualTo(indicator.Bounds.Bottom);

            steps.Type = Desktop.Controls.StepsType.Navigation;
            steps.Orientation = Orientation.Vertical;
            steps.TitlePlacement = Orientation.Vertical;
            Relayout();
            connector.IsVisible.ShouldBeFalse();
            arrow.IsVisible.ShouldBeTrue();
            arrow.Bounds.Y.ShouldBeGreaterThanOrEqualTo(GetBoundsInItem(header, item).Bottom);

            steps.Type = Desktop.Controls.StepsType.Default;
            steps.Orientation = Orientation.Horizontal;
            steps.TitlePlacement = Orientation.Horizontal;
            steps.SizeType = AtomUISizeType.Small;
            Relayout();
            indicator.Bounds.Width.ShouldBeLessThan(middleWidth);
        });
    }

    [Theory]
    [InlineData(Desktop.Controls.StepsType.Dot)]
    [InlineData(Desktop.Controls.StepsType.OutlineDot)]
    public void Dot_Types_Reserve_Current_Size_And_Inline_Content_Is_Removed_From_Layout(
        Desktop.Controls.StepsType type)
    {
        var steps = CreateSteps(
            type,
            Orientation.Horizontal,
            Orientation.Horizontal,
            AtomUISizeType.Middle);

        ShowInWindow(steps, () =>
        {
            var first = steps.Items[0].ShouldBeOfType<Desktop.Controls.StepsItem>();
            var second = steps.Items[1].ShouldBeOfType<Desktop.Controls.StepsItem>();
            var currentIndicator = GetIndicator(first);
            var waitingIndicator = GetIndicator(second);
            var currentFrame = GetIndicatorFrame(currentIndicator);
            var waitingFrame = GetIndicatorFrame(waitingIndicator);

            currentIndicator.Bounds.Width.ShouldBe(waitingIndicator.Bounds.Width);
            currentIndicator.Bounds.Height.ShouldBe(waitingIndicator.Bounds.Height);
            currentFrame.Bounds.Width.ShouldBeGreaterThan(waitingFrame.Bounds.Width);
            currentFrame.Bounds.Height.ShouldBeGreaterThan(waitingFrame.Bounds.Height);

            steps.Type = Desktop.Controls.StepsType.Inline;
            Relayout();
            FindControl(first, "ContentPresenter").IsVisible.ShouldBeFalse();
            GetIndicator(first).Bounds.Width.ShouldBe(GetIndicator(second).Bounds.Width);
        });
    }

    [Theory]
    [InlineData(AtomUISizeType.Middle)]
    [InlineData(AtomUISizeType.Small)]
    public void Horizontal_Default_Connector_Has_Symmetric_Endpoint_Gaps(AtomUISizeType sizeType)
    {
        var steps = CreateSteps(
            Desktop.Controls.StepsType.Default,
            Orientation.Horizontal,
            Orientation.Horizontal,
            sizeType);
        // The rail geometry assertions assume an uncompressed item; keep the
        // container wide enough that the items never shrink below their content.
        steps.Width = 1400;
        steps.Items[0].ShouldBeOfType<Desktop.Controls.StepsItem>().Content =
            "A description that is deliberately wider than the title row";

        ShowInWindow(steps, () =>
        {
            var first = steps.Items[0].ShouldBeOfType<Desktop.Controls.StepsItem>();
            var layoutPanel = GetLayoutPanel(first);
            var header = FindControl(first, "HeaderPresenter");
            var subHeader = FindControl(first, "SubHeaderPresenter");
            var connector = FindControl(first, "Connector");
            // The heading presenters live inside the item's section panel, so their
            // Bounds are section-relative; compare the row edge in item coordinates.
            var headerRowRight = Math.Max(
                GetBoundsInItem(header, first).Right,
                GetBoundsInItem(subHeader, first).Right);

            connector.Margin.Left.ShouldBeGreaterThan(0);
            connector.Margin.Right.ShouldBe(connector.Margin.Left);
            connector.Bounds.X.ShouldBe(headerRowRight + connector.Margin.Left, 0.01);
            (layoutPanel.Bounds.Width - connector.Bounds.Right).ShouldBe(connector.Margin.Right, 0.01);
        });
    }

    [Theory]
    [InlineData(AtomUISizeType.Middle)]
    [InlineData(AtomUISizeType.Small)]
    public void Horizontal_Default_Connector_Center_Matches_Indicator_Center(AtomUISizeType sizeType)
    {
        var steps = CreateSteps(
            Desktop.Controls.StepsType.Default,
            Orientation.Horizontal,
            Orientation.Horizontal,
            sizeType);

        ShowInWindow(steps, () =>
        {
            foreach (var item in steps.Items.Cast<Desktop.Controls.StepsItem>().Take(2))
            {
                var indicator = GetIndicator(item);
                var connector = FindControl(item, "Connector");

                connector.Bounds.Center.Y.ShouldBe(indicator.Bounds.Center.Y, 0.01);
            }
        });
    }

    [Theory]
    [InlineData(AtomUISizeType.Middle)]
    [InlineData(AtomUISizeType.Small)]
    public void Horizontal_Default_Header_Row_Centers_Stay_Aligned_When_SubHeader_Is_Present(AtomUISizeType sizeType)
    {
        var steps = CreateSteps(
            Desktop.Controls.StepsType.Default,
            Orientation.Horizontal,
            Orientation.Horizontal,
            sizeType);

        ShowInWindow(steps, () =>
        {
            LayoutAtNaturalWidth(steps);
            var items = steps.Items.Cast<Desktop.Controls.StepsItem>().ToArray();
            var centerY = GetIndicator(items[0]).Bounds.Center.Y;

            foreach (var item in items)
            {
                GetIndicator(item).Bounds.Center.Y.ShouldBe(centerY, 0.01);
                FindControl(item, "HeaderPresenter").Bounds.Center.Y.ShouldBe(centerY, 0.01);
            }

            FindControl(items[1], "SubHeaderPresenter").Bounds.Center.Y.ShouldBe(centerY, 0.01);

            foreach (var item in items.Take(2))
            {
                FindControl(item, "Connector").Bounds.Center.Y.ShouldBe(centerY, 0.01);
            }
        });
    }

    [Theory]
    [InlineData(AtomUISizeType.Middle)]
    [InlineData(AtomUISizeType.Small)]
    public void Horizontal_Default_Content_Row_Starts_At_Same_Y_When_SubHeader_Is_Present(AtomUISizeType sizeType)
    {
        var steps = CreateSteps(
            Desktop.Controls.StepsType.Default,
            Orientation.Horizontal,
            Orientation.Horizontal,
            sizeType);

        ShowInWindow(steps, () =>
        {
            LayoutAtNaturalWidth(steps);
            var items = steps.Items.Cast<Desktop.Controls.StepsItem>().ToArray();
            var contentY = FindControl(items[0], "ContentPresenter").Bounds.Y;

            foreach (var item in items)
            {
                FindControl(item, "ContentPresenter").Bounds.Y.ShouldBe(contentY, 0.01);
            }
        });
    }

    [Theory]
    [InlineData(AtomUISizeType.Middle)]
    [InlineData(AtomUISizeType.Small)]
    public void Horizontal_Default_SubHeader_Margin_Is_Inline_Start_Only(AtomUISizeType sizeType)
    {
        var steps = CreateSteps(
            Desktop.Controls.StepsType.Default,
            Orientation.Horizontal,
            Orientation.Horizontal,
            sizeType);

        ShowInWindow(steps, () =>
        {
            LayoutAtNaturalWidth(steps);
            var second = steps.Items[1].ShouldBeOfType<Desktop.Controls.StepsItem>();
            var header = FindControl(second, "HeaderPresenter");
            var subHeader = FindControl(second, "SubHeaderPresenter");

            subHeader.IsVisible.ShouldBeTrue();
            subHeader.Margin.Left.ShouldBeGreaterThan(0);
            subHeader.Margin.Top.ShouldBe(0);
            subHeader.Margin.Right.ShouldBe(0);
            subHeader.Margin.Bottom.ShouldBe(0);
            subHeader.Bounds.Center.Y.ShouldBe(header.Bounds.Center.Y, 0.01);
        });
    }

    [Theory]
    [InlineData(AtomUISizeType.Middle)]
    [InlineData(AtomUISizeType.Small)]
    public void Horizontal_Default_Items_Without_SubHeader_Do_Not_Reserve_SubHeader_Margin(AtomUISizeType sizeType)
    {
        var steps = CreateSteps(
            Desktop.Controls.StepsType.Default,
            Orientation.Horizontal,
            Orientation.Horizontal,
            sizeType);
        steps.Items[0].ShouldBeOfType<Desktop.Controls.StepsItem>().SubHeader = null;
        steps.Items[2].ShouldBeOfType<Desktop.Controls.StepsItem>().SubHeader = null;
        // The heading-row geometry assertions assume uncompressed items; keep the
        // container wide enough that no header wraps.
        steps.Width = 1400;

        ShowInWindow(steps, () =>
        {
            var items = steps.Items.Cast<Desktop.Controls.StepsItem>().ToArray();
            var rowCenterY = GetIndicator(items[0]).Bounds.Center.Y;
            var contentY = FindControl(items[0], "ContentPresenter").Bounds.Y;

            FindControl(items[0], "SubHeaderPresenter").IsVisible.ShouldBeFalse();
            FindControl(items[2], "SubHeaderPresenter").IsVisible.ShouldBeFalse();

            foreach (var item in items)
            {
                GetIndicator(item).Bounds.Center.Y.ShouldBe(rowCenterY, 0.01);
                FindControl(item, "HeaderPresenter").Bounds.Center.Y.ShouldBe(rowCenterY, 0.01);
                FindControl(item, "ContentPresenter").Bounds.Y.ShouldBe(contentY, 0.01);
            }

            var firstHeader = FindControl(items[0], "HeaderPresenter");
            var firstConnector = FindControl(items[0], "Connector");
            firstConnector.Bounds.X.ShouldBe(
                GetBoundsInItem(firstHeader, items[0]).Right + firstConnector.Margin.Left, 0.01);
        });
    }

    [Theory]
    [InlineData(AtomUISizeType.Middle)]
    [InlineData(AtomUISizeType.Small)]
    public void Vertical_Default_Content_Text_Starts_At_Header_Text_Start_When_SubHeader_Is_Present(AtomUISizeType sizeType)
    {
        var steps = CreateSteps(
            Desktop.Controls.StepsType.Default,
            Orientation.Vertical,
            Orientation.Horizontal,
            sizeType);

        ShowInWindow(steps, () =>
        {
            var second = steps.Items[1].ShouldBeOfType<Desktop.Controls.StepsItem>();
            var header = FindControl(second, "HeaderPresenter").ShouldBeOfType<ContentPresenter>();
            var content = FindControl(second, "ContentPresenter").ShouldBeOfType<ContentPresenter>();
            var headerText = header.Content.ShouldBeOfType<AvaloniaTextBlock>();
            var contentText = content.Content.ShouldBeOfType<AvaloniaTextBlock>();

            content.Margin.Left.ShouldBe(0);
            content.Padding.Left.ShouldBe(0);
            GetLeftInItem(headerText, second).ShouldBe(GetLeftInItem(contentText, second), 0.01);
        });
    }

    [Fact]
    public void Horizontal_Default_Item_Row_Is_Centered_When_Steps_Is_Stretched()
    {
        var steps = CreateSteps(
            Desktop.Controls.StepsType.Default,
            Orientation.Horizontal,
            Orientation.Horizontal,
            AtomUISizeType.Middle);

        ShowInWindow(steps, () =>
        {
            var first = steps.Items[0].ShouldBeOfType<Desktop.Controls.StepsItem>();

            first.Bounds.Y.ShouldBeGreaterThan(0);
            first.Bounds.Center.Y.ShouldBe(steps.Bounds.Center.Y, 0.01);
        });
    }

    [Fact]
    public void Default_Indicator_Displays_The_OneBased_Step_Number()
    {
        var steps = CreateSteps(
            Desktop.Controls.StepsType.Default,
            Orientation.Horizontal,
            Orientation.Horizontal,
            AtomUISizeType.Middle);
        steps.Initial = 4;
        steps.Current = 4;

        ShowInWindow(steps, () =>
        {
            var first = steps.Items[0].ShouldBeOfType<Desktop.Controls.StepsItem>();
            var number = GetIndicator(first).GetVisualDescendants()
                                            .OfType<Avalonia.Controls.TextBlock>()
                                            .Single(control => control.Name == "StepNumberText");

            number.Text.ShouldBe("5");
        });
    }

    [Fact]
    public void Offset_Is_Bound_To_Items_Panel()
    {
        var steps = CreateSteps(
            Desktop.Controls.StepsType.Inline,
            Orientation.Horizontal,
            Orientation.Vertical,
            AtomUISizeType.Middle);
        steps.Offset = 2;

        ShowInWindow(steps, () =>
        {
            var panel = steps.GetVisualDescendants().OfType<Desktop.Controls.StepsPanel>().Single();

            panel.Offset.ShouldBe(2);
        });
    }

    [Fact]
    public void Horizontal_Content_Alignment_Is_Bound_To_Items_Panel()
    {
        var steps = CreateSteps(
            Desktop.Controls.StepsType.Navigation,
            Orientation.Vertical,
            Orientation.Horizontal,
            AtomUISizeType.Middle);
        steps.HorizontalContentAlignment = HorizontalAlignment.Right;

        ShowInWindow(steps, () =>
        {
            var panel = steps.GetVisualDescendants().OfType<Desktop.Controls.StepsPanel>().Single();

            panel.HorizontalContentAlignment.ShouldBe(HorizontalAlignment.Right);
        });
    }

    private static Desktop.Controls.Steps CreateSteps(
        Desktop.Controls.StepsType type,
        Orientation orientation,
        Orientation titlePlacement,
        AtomUISizeType sizeType)
    {
        var steps = new Desktop.Controls.Steps
        {
            Width           = 720,
            Current         = 0,
            Type            = type,
            Orientation     = orientation,
            TitlePlacement  = titlePlacement,
            SizeType        = sizeType,
            IsItemClickable = true
        };
        steps.Items.Add(new Desktop.Controls.StepsItem
        {
            Header = "First step",
            SubHeader = "00:01",
            Content = "First step content"
        });
        steps.Items.Add(new Desktop.Controls.StepsItem
        {
            Header = "Second step",
            SubHeader = "00:02",
            Content = "Second step content"
        });
        steps.Items.Add(new Desktop.Controls.StepsItem
        {
            Header = "Last step",
            SubHeader = "00:03",
            Content = "Last step content"
        });
        return steps;
    }

    private static Desktop.Controls.StepsItemIndicator GetIndicator(Desktop.Controls.StepsItem item)
    {
        return item.GetVisualDescendants().OfType<Desktop.Controls.StepsItemIndicator>().Single();
    }

    private static Desktop.Controls.StepsItemLayoutPanel GetLayoutPanel(Desktop.Controls.StepsItem item)
    {
        return item.GetVisualDescendants().OfType<Desktop.Controls.StepsItemLayoutPanel>().Single();
    }

    private static Control GetIndicatorFrame(Desktop.Controls.StepsItemIndicator indicator)
    {
        return indicator.GetVisualDescendants().OfType<Control>().Single(control => control.Name == "Frame");
    }

    private static Control FindControl(Desktop.Controls.StepsItem item, string name)
    {
        return item.GetVisualDescendants().OfType<Control>().Single(control => control.Name == name);
    }

    private static double GetLeftInItem(Control control, Desktop.Controls.StepsItem item)
    {
        return control.TranslatePoint(default, item)!.Value.X;
    }

    private static Rect GetBoundsInItem(Control control, Desktop.Controls.StepsItem item)
    {
        return new Rect(control.TranslatePoint(default, item)!.Value, control.Bounds.Size);
    }

    private static void AssertFinite(Control control)
    {
        double.IsFinite(control.Bounds.X).ShouldBeTrue();
        double.IsFinite(control.Bounds.Y).ShouldBeTrue();
        double.IsFinite(control.Bounds.Width).ShouldBeTrue();
        double.IsFinite(control.Bounds.Height).ShouldBeTrue();
        control.Bounds.Width.ShouldBeGreaterThanOrEqualTo(0);
        control.Bounds.Height.ShouldBeGreaterThanOrEqualTo(0);
    }

    private static void AssertNoInteriorOverlap(
        Control first,
        Control second,
        Desktop.Controls.StepsItem item)
    {
        if (!first.IsVisible || !second.IsVisible)
        {
            return;
        }

        // The body parts live inside the item's section panel, so their Bounds are
        // section-relative; compare in the item's coordinate space.
        var intersection = GetBoundsInItem(first, item).Intersect(GetBoundsInItem(second, item));
        (intersection.Width > 0 && intersection.Height > 0).ShouldBeFalse(
            $"{first.Name ?? first.GetType().Name} overlaps {second.Name ?? second.GetType().Name}.");
    }

    private static void Relayout()
    {
        Dispatcher.UIThread.RunJobs();
        AvaloniaHeadlessPlatform.ForceRenderTimerTick(1);
        Dispatcher.UIThread.RunJobs();
    }

    private static void LayoutAtNaturalWidth(Desktop.Controls.Steps steps)
    {
        // Heading-row geometry assertions assume uncompressed items with the header
        // and subheader on one row; size the container to the measured natural width
        // so the test font can never compress the items and stack the subheader.
        steps.Width = double.NaN;
        steps.Measure(Size.Infinity);
        steps.Width = steps.DesiredSize.Width;
        Relayout();
    }

    private static void ShowInWindow(Control content, Action assertion)
    {
        var window = new AvaloniaWindow
        {
            Width   = 900,
            Height  = 420,
            Content = content
        };

        try
        {
            window.Show();
            Relayout();
            assertion();
        }
        finally
        {
            window.Close();
        }
    }
}
