using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using CP = Avalonia.Controls.Presenters.ContentPresenter;
using Shouldly;
using Xunit;
using AtomUILineEdit = AtomUI.Desktop.Controls.LineEdit;
using AtomUISearchEdit = AtomUI.Desktop.Controls.SearchEdit;
using AvaloniaButton = Avalonia.Controls.Button;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Input;

/// <summary>
/// Guards the suffix button alignment contract shared by the LineEdit input
/// family: clear and reveal buttons must be vertically centered inside their
/// suffix layout instead of top-aligned next to taller siblings.
/// </summary>
public class InputSuffixVerticalCenteringTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void Clear_And_Reveal_Buttons_Stay_Vertically_Centered(int controlIndex)
    {
        var window = new AvaloniaWindow { Width = 480, Height = 160 };
        Control owner = controlIndex switch
        {
            0 => new AtomUILineEdit
            {
                Text        = "atomui",
                IsAllowClear = true,
                InnerRightContent = ".com"
            },
            _ => new AtomUISearchEdit
            {
                Text         = "atomui",
                IsAllowClear = true,
                InnerRightContent = ".com"
            }
        };
        window.Content = owner;

        try
        {
            window.Show();
            owner.ApplyTemplate();
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            var clearButton = owner.GetVisualDescendants()
                                   .OfType<AvaloniaButton>()
                                   .Single(item => item.Name == "PART_ClearButton");
            var suffixPanel = (Panel)clearButton.Parent!;

            clearButton.IsVisible.ShouldBeTrue();
            // Guards the suffix midline contract: the clear button and every
            // sibling part share the same vertical band inside the suffix panel.
            suffixPanel.Bounds.Height.ShouldBeGreaterThanOrEqualTo(clearButton.Bounds.Height);
            Math.Abs(clearButton.Bounds.Center.Y - suffixPanel.Bounds.Height / 2)
                .ShouldBeLessThanOrEqualTo(0.5);

            var icon = clearButton.GetVisualDescendants().First();
            icon.ShouldNotBeNull();
            Math.Abs(AbsCenter(icon) - AbsCenter(suffixPanel))
                .ShouldBeLessThanOrEqualTo(0.5);

            var textPart = owner.GetVisualDescendants()
                                .OfType<CP>()
                                .Single(item => item.Name == "InnerRightContentPresenter");
            Math.Abs(AbsCenter(textPart) - AbsCenter(clearButton))
                .ShouldBeLessThanOrEqualTo(0.5);
        }
        finally
        {
            window.Close();
        }
    }

    private static double AbsCenter(Visual visual)
    {
        // Layout-time position in the window: this headless app does not render, so render-time transformed
        // bounds are not window coordinates (they only agreed while every suffix part had the same height).
        var topLevel = TopLevel.GetTopLevel(visual);
        var center   = topLevel is null ? null : visual.TranslatePoint(new Point(0, visual.Bounds.Height / 2), topLevel);
        return center?.Y ?? -1;
    }

    private static string DescribeKids(Visual owner)
    {
        return string.Join("; ", owner.GetVisualChildren().Select(child =>
            $"{child.GetType().Name}:{child.Bounds} absY={AbsCenter(child):0.##}"));
    }
}
