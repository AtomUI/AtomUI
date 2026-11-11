using AtomUI.Desktop.Controls;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Badge;

public class CountBadgeContentTests
{
    static CountBadgeContentTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void CountBadge_Template_Presents_Count_Then_Content_Override()
    {
        var badge = new CountBadge
        {
            Count           = 120,
            IsMotionEnabled = false,
            DecoratedTarget = new Border { Width = 48, Height = 48 }
        };
        var window = Show(badge);
        try
        {
            var presenter = FindPresenter(badge);
            PresentedText(presenter).ShouldBe("99+");

            // Token-driven font size and foreground must reach the generated text element.
            var text = presenter.Child.ShouldBeAssignableTo<Avalonia.Controls.TextBlock>()!;
            presenter.FontSize.ShouldBeGreaterThan(0);
            text.FontSize.ShouldBe(presenter.FontSize);
            text.Foreground.ShouldBe(FindAdorner(badge) is TemplatedControl adorner ? adorner.Foreground : null);

            badge.Content = "NEW";
            Dispatcher.UIThread.RunJobs();
            PresentedText(presenter).ShouldBe("NEW");

            badge.Content = null;
            Dispatcher.UIThread.RunJobs();
            PresentedText(presenter).ShouldBe("99+");
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void CountBadge_IsBordered_Defaults_True_And_Removes_Ring_When_False()
    {
        var badge = new CountBadge
        {
            Count           = 3,
            IsMotionEnabled = false,
            DecoratedTarget = new Border { Width = 48, Height = 48 }
        };
        badge.IsBordered.ShouldBeTrue();
        var window = Show(badge);
        try
        {
            var indicator = FindIndicatorBorder(badge);
            indicator.BoxShadow.Count.ShouldBe(1);

            badge.IsBordered = false;
            Dispatcher.UIThread.RunJobs();
            indicator.BoxShadow.Count.ShouldBe(0);
        }
        finally
        {
            window.Close();
        }
    }

    private static string? PresentedText(ContentPresenter presenter)
    {
        return presenter.Child.ShouldBeAssignableTo<Avalonia.Controls.TextBlock>()!.Text;
    }

    private static ContentPresenter FindPresenter(CountBadge badge)
    {
        return FindAdorner(badge).GetVisualDescendants().OfType<ContentPresenter>()
                                 .Single(p => p.Name == "PART_ContentPresenter");
    }

    private static Border FindIndicatorBorder(CountBadge badge)
    {
        return FindAdorner(badge).GetVisualDescendants().OfType<Border>()
                                 .Single(b => b.Name == "BadgeIndicator");
    }

    private static Control FindAdorner(CountBadge badge)
    {
        var layer = AdornerLayer.GetAdornerLayer(badge).ShouldNotBeNull();
        return layer.Children.Single(child => ReferenceEquals(AdornerLayer.GetAdornedElement(child), badge));
    }

    private static AvaloniaWindow Show(Control content)
    {
        var window = new AvaloniaWindow
        {
            Width   = 240,
            Height  = 160,
            Content = new VisualLayerManager { EnableAdornerLayer = true, Child = content }
        };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return window;
    }
}
