using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Headless;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;
using AtomUISizeType = AtomUI.SizeType;

namespace AtomUI.Desktop.Controls.Tests.Steps;

public class StepsPanelVisualTests
{
    static StepsPanelVisualTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Panel_Variant_API_Is_Explicitly_Panel_Scoped()
    {
        var steps = new Desktop.Controls.Steps();

        Desktop.Controls.Steps.PanelVariantProperty.Name.ShouldBe(nameof(Desktop.Controls.Steps.PanelVariant));
        steps.PanelVariant.ShouldBe(Desktop.Controls.StepsPanelVariant.Filled);
        typeof(Desktop.Controls.Steps).GetProperty("Variant").ShouldBeNull();
        typeof(Desktop.Controls.Steps).GetField("VariantProperty").ShouldBeNull();
    }

    [Fact]
    public void Panel_Uses_Equal_Width_Items_And_Only_Shows_Arrow_On_NonLast_Items()
    {
        var steps = CreateSteps(Desktop.Controls.StepsPanelVariant.Filled, Orientation.Vertical);

        ShowInWindow(steps, () =>
        {
            var items = steps.Items.Cast<Desktop.Controls.StepsItem>().ToArray();
            items.Select(item => item.Bounds.Width).ShouldAllBe(width => width == items[0].Bounds.Width);
            items.Select(item => item.Bounds.X).ShouldBe([0d, 240d, 480d]);

            for (var index = 0; index < items.Length; index++)
            {
                var item = items[index];
                FindControl(item, "PART_Indicator").IsVisible.ShouldBeFalse();
                FindControl(item, "Connector").IsVisible.ShouldBeFalse();
                FindControl(item, "PanelArrow").IsVisible.ShouldBe(index < items.Length - 1);
                FindControl(item, "NavigationArrow").IsVisible.ShouldBeFalse();

                var wrapper = FindControl(item, "ItemWrapper");
                wrapper.Bounds.Size.ShouldBe(item.Bounds.Size);
            }
        });
    }

    [Fact]
    public void Panel_Outlined_Uses_Container_Background_And_Current_Primary_Background()
    {
        var steps = CreateSteps(Desktop.Controls.StepsPanelVariant.Outlined, Orientation.Horizontal);

        ShowInWindow(steps, () =>
        {
            var items = steps.Items.Cast<Desktop.Controls.StepsItem>().ToArray();
            var current = items[1];
            var expectedBackground = GetThemeResource<IBrush>(SharedTokenKind.ColorBgContainer);
            var expectedActive = GetThemeResource<IBrush>(SharedTokenKind.ColorPrimaryBg);
            var expectedOutlinedText = GetThemeResource<IBrush>(StepsTokenKind.PanelProcessTextColor);

            GetColor(items[0].Background).ShouldBe(GetColor(expectedBackground));
            GetColor(current.Background).ShouldBe(GetColor(expectedActive));
            current.BorderBrush.ShouldNotBeNull();
            GetColor(current.Foreground).ShouldBe(GetColor(expectedOutlinedText));
            GetColor(((ContentPresenter)FindControl(current, "SubHeaderPresenter")).Foreground).ShouldBe(GetColor(expectedOutlinedText));
            GetColor(((ContentPresenter)FindControl(current, "ContentPresenter")).Foreground).ShouldBe(GetColor(expectedOutlinedText));

            var currentArrow = (Desktop.Controls.StepsPanelArrow)FindControl(current, "PanelArrow");
            var strokeInset = currentArrow.StrokeThickness / 2;
            currentArrow.Bounds.Top.ShouldBe(strokeInset);
            currentArrow.Bounds.Bottom.ShouldBe(current.Bounds.Height - strokeInset);

            steps.PanelVariant = Desktop.Controls.StepsPanelVariant.Filled;
            Dispatcher.UIThread.RunJobs();
            AvaloniaHeadlessPlatform.ForceRenderTimerTick(1);
            Dispatcher.UIThread.RunJobs();
            GetColor(current.Background).ShouldBe(GetColor(GetThemeResource<IBrush>(StepsTokenKind.ProcessIconBgColor)));
            var expectedFilledText = GetThemeResource<IBrush>(StepsTokenKind.PanelActiveTextColor);
            GetColor(current.Foreground).ShouldBe(GetColor(expectedFilledText));
            GetColor(((ContentPresenter)FindControl(current, "SubHeaderPresenter")).Foreground).ShouldBe(GetColor(expectedFilledText));
            GetColor(((ContentPresenter)FindControl(current, "ContentPresenter")).Foreground).ShouldBe(GetColor(expectedFilledText));
            currentArrow.Bounds.Top.ShouldBe(0);
            currentArrow.Bounds.Bottom.ShouldBe(current.Bounds.Height);
        });
    }

    [Fact]
    public void Panel_Outlined_NonCurrent_Error_Uses_Container_Background_And_Error_Border()
    {
        var steps = CreateSteps(Desktop.Controls.StepsPanelVariant.Outlined, Orientation.Horizontal);
        steps.Current = 0;
        var error = steps.Items.Cast<Desktop.Controls.StepsItem>().ElementAt(1);
        error.Status = Desktop.Controls.StepsStatus.Error;

        ShowInWindow(steps, () =>
        {
            GetColor(error.Background).ShouldBe(GetColor(GetThemeResource<IBrush>(SharedTokenKind.ColorBgContainer)));
            GetColor(error.BorderBrush).ShouldBe(GetColor(GetThemeResource<IBrush>(SharedTokenKind.ColorError)));
            GetColor(error.Foreground).ShouldBe(GetColor(GetThemeResource<IBrush>(StepsTokenKind.PanelErrorTextColor)));

            steps.Current = 1;
            Dispatcher.UIThread.RunJobs();
            GetColor(error.Background).ShouldBe(GetColor(GetThemeResource<IBrush>(SharedTokenKind.ColorErrorBg)));
            GetColor(error.Foreground).ShouldBe(GetColor(GetThemeResource<IBrush>(StepsTokenKind.PanelErrorTextColor)));
        });
    }

    [Fact]
    public void Panel_Arrow_Uses_Rtl_Overflow_Direction()
    {
        var steps = CreateSteps(Desktop.Controls.StepsPanelVariant.Filled, Orientation.Horizontal);
        steps.FlowDirection = FlowDirection.RightToLeft;

        ShowInWindow(steps, () =>
        {
            var first = steps.Items.Cast<Desktop.Controls.StepsItem>().First();
            var arrow = FindControl(first, "PanelArrow");
            arrow.Bounds.Right.ShouldBeLessThanOrEqualTo(2);
            arrow.Bounds.Left.ShouldBeLessThan(0);
        });
    }

    [Fact]
    public void Panel_Filled_Clips_NonFirst_ItemFrame_And_Reserves_Arrow_Inset_For_Content()
    {
        var steps = CreateSteps(Desktop.Controls.StepsPanelVariant.Filled, Orientation.Horizontal);

        ShowInWindow(steps, () =>
        {
            var items = steps.Items.Cast<Desktop.Controls.StepsItem>().ToArray();
            var firstArrow = (Desktop.Controls.StepsPanelArrow)FindControl(items[0], "PanelArrow");
            var secondWrapper = (Desktop.Controls.StepsPanelItemFrame)FindControl(items[1], "ItemWrapper");
            var secondHeader = FindControl(items[1], "HeaderPresenter");
            var secondContent = FindControl(items[1], "ContentPresenter");

            firstArrow.Width.ShouldBeGreaterThan(0);
            secondWrapper.BackgroundSizing.ShouldBe(BackgroundSizing.OuterBorderEdge);
            secondWrapper.Clip.ShouldNotBeNull();
            // The body presenters live inside the item's section panel, so their
            // Bounds are section-relative; compare the content inset in item
            // coordinates.
            GetBoundsInItem(secondHeader, items[1]).Left.ShouldBeGreaterThanOrEqualTo(firstArrow.Width);
            GetBoundsInItem(secondContent, items[1]).Left.ShouldBeGreaterThanOrEqualTo(firstArrow.Width);
        });
    }

    [Fact]
    public void Filled_Notch_Clip_Is_Exclusive_To_The_Panel_Type()
    {
        // Non-Panel items only paint the item frame as a hover/focus background, so
        // the Filled leading-notch clip must never apply to them: a notched gray
        // rectangle shows up as soon as such a background becomes visible.
        foreach (var type in new[]
                 {
                     Desktop.Controls.StepsType.Default,
                     Desktop.Controls.StepsType.Dot,
                     Desktop.Controls.StepsType.OutlineDot,
                     Desktop.Controls.StepsType.Navigation,
                     Desktop.Controls.StepsType.Inline
                 })
        {
            var steps = new Desktop.Controls.Steps
            {
                Width = 720,
                Height = 120,
                Type = type,
                Orientation = Orientation.Horizontal,
                Current = 1
            };
            steps.Items.Add(new Desktop.Controls.StepsItem { Header = "First" });
            steps.Items.Add(new Desktop.Controls.StepsItem { Header = "Second" });
            steps.Items.Add(new Desktop.Controls.StepsItem { Header = "Last" });

            ShowInWindow(steps, () =>
            {
                foreach (var item in steps.Items.Cast<Desktop.Controls.StepsItem>())
                {
                    var wrapper = (Desktop.Controls.StepsPanelItemFrame)FindControl(item, "ItemWrapper");
                    wrapper.PanelVariant.ShouldBe(Desktop.Controls.StepsPanelVariant.Filled);
                    wrapper.Clip.ShouldBeNull($"type {type} must not clip the item frame");
                }
            });
        }
    }

    [Fact]
    public void Inline_Hover_Background_Fills_The_Item_Cell_Width_Like_Antd_v6()
    {
        var steps = new Desktop.Controls.Steps
        {
            Width = 720,
            Height = 120,
            Type = Desktop.Controls.StepsType.Inline,
            Orientation = Orientation.Horizontal,
            Current = 1
        };
        steps.Items.Add(new Desktop.Controls.StepsItem { Header = "First", SubHeader = "Sub" });
        steps.Items.Add(new Desktop.Controls.StepsItem { Header = "Second", SubHeader = "Sub" });
        steps.Items.Add(new Desktop.Controls.StepsItem { Header = "Last", SubHeader = "Sub" });

        ShowInWindow(steps, () =>
        {
            foreach (var item in steps.Items.Cast<Desktop.Controls.StepsItem>())
            {
                var wrapper   = FindControl(item, "ItemWrapper");
                var indicator = FindControl(item, "PART_Indicator");
                var section   = FindControl(item, "Section");
                var panel     = (Desktop.Controls.StepsItemLayoutPanel)wrapper.Parent!;

                // Wrapper, indicator and section share the item layout panel as their
                // visual parent, so their bounds are directly comparable. antd v6
                // renders the inline hover background on `.ant-steps-item-wrapper`:
                // a block that fills the item cell width, with the block padding
                // contract supplied by the `InlineItemPadding` token bound to the
                // panel `Padding` (top = paddingXS + lineWidth, bottom = 0).
                wrapper.Bounds.Contains(indicator.Bounds).ShouldBeTrue();
                wrapper.Bounds.Contains(section.Bounds).ShouldBeTrue();
                wrapper.Bounds.Left.ShouldBe(panel.Bounds.Left, 0.01);
                wrapper.Bounds.Right.ShouldBe(panel.Bounds.Right, 0.01);
                wrapper.Bounds.Top.ShouldBe(Math.Min(indicator.Bounds.Top, section.Bounds.Top) - panel.Padding.Top, 0.01);
                wrapper.Bounds.Bottom.ShouldBe(Math.Max(indicator.Bounds.Bottom, section.Bounds.Bottom) + panel.Padding.Bottom, 0.01);
            }
        });
    }

    private static Desktop.Controls.Steps CreateSteps(
        Desktop.Controls.StepsPanelVariant panelVariant,
        Orientation orientation)
    {
        var steps = new Desktop.Controls.Steps
        {
            Width = 720,
            Height = 120,
            Type = Desktop.Controls.StepsType.Panel,
            PanelVariant = panelVariant,
            Orientation = orientation,
            SizeType = AtomUISizeType.Middle,
            Current = 1
        };
        steps.Items.Add(new Desktop.Controls.StepsItem { Header = "First", SubHeader = "00:01", Content = "Content" });
        steps.Items.Add(new Desktop.Controls.StepsItem { Header = "Second", SubHeader = "00:02", Content = "Content" });
        steps.Items.Add(new Desktop.Controls.StepsItem { Header = "Last", SubHeader = "00:03", Content = "Content" });
        return steps;
    }

    private static Control FindControl(Desktop.Controls.StepsItem item, string name)
    {
        return item.GetVisualDescendants().OfType<Control>().Single(control => control.Name == name);
    }

    private static Rect GetBoundsInItem(Control control, Desktop.Controls.StepsItem item)
    {
        return new Rect(control.TranslatePoint(default, item)!.Value, control.Bounds.Size);
    }

    private static T GetThemeResource<T>(object key)
    {
        var application = Application.Current.ShouldNotBeNull();
        application.TryGetResource(key, application.ActualThemeVariant, out var value).ShouldBeTrue();
        value.ShouldBeAssignableTo<T>();
        return (T)value!;
    }

    private static Color GetColor(IBrush? brush)
    {
        brush.ShouldNotBeNull();
        return ((ISolidColorBrush)brush!).Color;
    }

    private static void ShowInWindow(Control content, Action assertion)
    {
        var window = new AvaloniaWindow
        {
            Width = 900,
            Height = 360,
            Content = content
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            AvaloniaHeadlessPlatform.ForceRenderTimerTick(1);
            Dispatcher.UIThread.RunJobs();
            assertion();
        }
        finally
        {
            window.Close();
        }
    }
}
