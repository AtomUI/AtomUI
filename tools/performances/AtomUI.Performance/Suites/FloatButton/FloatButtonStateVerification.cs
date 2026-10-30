using AtomUI.Controls;
using AtomUI.Controls.Commons;
using AtomUI.Desktop.Controls;
using Avalonia.Controls;
using Avalonia.VisualTree;

namespace AtomUI.Performance;

internal static partial class Program
{
    private static bool RunFloatButtonStateVerification()
    {
        var failures = new List<string>();
        VerifyFloatButtonHostOverlayCleanup(failures);
        VerifyFloatButtonGroupHostOverlayCleanup(failures);
        VerifyFloatButtonBadgeLifecycle(failures);
        VerifyBackTopFloatButtonBadgeTemplatePart(failures);
        VerifyFloatButtonTriggerMenuLifecycle(failures);
        VerifyDefaultFloatButtonGroupStillCreatesItems(failures);
        VerifyFloatButtonGroupChildStateSync(failures);

        if (failures.Count == 0)
        {
            Console.WriteLine("FloatButton state verification passed.");
            return true;
        }

        Console.Error.WriteLine("FloatButton state verification failed:");
        foreach (var failure in failures)
        {
            Console.Error.WriteLine($"- {failure}");
        }
        return false;
    }

    private static void VerifyFloatButtonHostOverlayCleanup(ICollection<string> failures)
    {
        var host = new FloatButtonHost();
        var root = CreateFloatButtonOverlay(host);
        object? overlayButton;

        using (RealizeControl(root))
        {
            overlayButton = GetPrivateField(host, "AtomUI.Controls.Commons.AbstractFloatButtonHost", "FloatButton");
            Expect(overlayButton is FloatButton,
                "FloatButtonHost should create an overlay FloatButton while attached.",
                failures);
        }

        Expect(GetPrivateField(host, "AtomUI.Controls.Commons.AbstractFloatButtonHost", "FloatButton") == null,
            "Detached FloatButtonHost should clear the overlay FloatButton field.",
            failures);
        Expect(GetPrivateField(host, "AtomUI.Controls.Commons.AbstractFloatButtonHost", "Disposables") == null,
            "Detached FloatButtonHost should dispose and clear binding disposables.",
            failures);
        Expect(overlayButton is not Control control || control.GetVisualParent() == null,
            "Detached FloatButtonHost should remove the overlay FloatButton visual parent.",
            failures);
    }

    private static void VerifyFloatButtonGroupHostOverlayCleanup(ICollection<string> failures)
    {
        var host = CreateTriggerGroupHost(FloatButtonGroupTrigger.Click, false);
        var root = CreateFloatButtonOverlay(host);
        object? overlayGroup;

        using (RealizeControl(root))
        {
            overlayGroup = GetPrivateField(host, "AtomUI.Desktop.Controls.FloatButtonGroupHost", "FloatButtonGroup");
            Expect(overlayGroup is FloatButtonGroup,
                "FloatButtonGroupHost should create an overlay FloatButtonGroup while attached.",
                failures);
        }

        Expect(GetPrivateField(host, "AtomUI.Desktop.Controls.FloatButtonGroupHost", "FloatButtonGroup") == null,
            "Detached FloatButtonGroupHost should clear the overlay FloatButtonGroup field.",
            failures);
        Expect(GetPrivateField(host, "AtomUI.Desktop.Controls.FloatButtonGroupHost", "Disposables") == null,
            "Detached FloatButtonGroupHost should dispose and clear binding disposables.",
            failures);
        Expect(overlayGroup is not Control control || control.GetVisualParent() == null,
            "Detached FloatButtonGroupHost should remove the overlay FloatButtonGroup visual parent.",
            failures);
    }

    private static void VerifyFloatButtonBadgeLifecycle(ICollection<string> failures)
    {
        var button = new FloatButton
        {
            IsBadgeEnabled = true,
            IsDotBadge     = false,
            BadgeCount     = 5
        };
        using var realized = RealizeControl(button);

        Expect(CountVisualByTypeName(button, "CountBadgeAdorner") == 1,
            "FloatButton count badge should create one CountBadgeAdorner.",
            failures);
        var countBadge = GetPrivateField(button, "AtomUI.Controls.Commons.AbstractFloatButton", "_badge") as Control;

        button.IsDotBadge = true;
        RefreshLayout(realized.Window);
        Expect(CountVisualByTypeName(button, "CountBadgeAdorner") == 0 &&
               CountVisualByTypeName(button, "DotBadgeAdorner") == 1,
            "FloatButton should replace count badge with one dot badge, not accumulate both.",
            failures);
        Expect(countBadge?.GetVisualParent() == null,
            "Replaced FloatButton count badge should not keep a visual parent.",
            failures);
        var dotBadge = GetPrivateField(button, "AtomUI.Controls.Commons.AbstractFloatButton", "_badge") as Control;

        button.IsBadgeEnabled = false;
        RefreshLayout(realized.Window);
        Expect(CountVisualByTypeName(button, "CountBadgeAdorner") == 0 &&
               CountVisualByTypeName(button, "DotBadgeAdorner") == 0,
            "Disabled FloatButton badge should remove all badge adorners.",
            failures);
        Expect(dotBadge?.GetVisualParent() == null,
            "Disabled FloatButton dot badge should not keep a visual parent.",
            failures);
        Expect(GetPrivateField(button, "AtomUI.Controls.Commons.AbstractFloatButton", "_badge") == null,
            "Disabled FloatButton badge should clear the badge field.",
            failures);
    }

    private static void VerifyBackTopFloatButtonBadgeTemplatePart(ICollection<string> failures)
    {
        var button = new BackTopFloatButton
        {
            IsBadgeEnabled = true,
            IsDotBadge     = false,
            BadgeCount     = 3
        };
        using var _ = RealizeControl(button);

        Expect(FindVisualByName<Canvas>(button, "PART_BadgeLayout") != null,
            "BackTopFloatButton template should expose PART_BadgeLayout.",
            failures);
        Expect(CountVisualByTypeName(button, "CountBadgeAdorner") == 1,
            "BackTopFloatButton badge should be configured through PART_BadgeLayout.",
            failures);
    }

    private static void VerifyFloatButtonTriggerMenuLifecycle(ICollection<string> failures)
    {
        var host = CreateTriggerGroupHost(FloatButtonGroupTrigger.Click, false);
        var root = CreateFloatButtonOverlay(host);
        using var realized = RealizeControl(root);

        var surface = FindVisualByName<Border>(root, "PART_MenuSurface");
        Expect(surface is { IsVisible: false },
            "Closed trigger FloatButtonGroup should hide its static menu surface.",
            failures);
        Expect(CountVisualByTypeName(root, "FloatButtonItemsControl") == 1,
            "Trigger FloatButtonGroup should materialize one menu inside the hidden surface.",
            failures);

        host.IsOpen = true;
        Expect(surface == null || !surface.IsVisible || surface.Opacity < 0.001,
            "First-open menu should remain transparent until its motion starts.",
            failures);
        RefreshLayout(realized.Window);
        Expect(ReferenceEquals(surface, FindVisualByName<Border>(root, "PART_MenuSurface")),
            "Opening trigger FloatButtonGroup should reuse its static menu surface.",
            failures);

        host.IsOpen = false;
        RefreshLayout(realized.Window);
        host.IsOpen = true;
        RefreshLayout(realized.Window);
        Expect(CountVisualByTypeName(root, "FloatButtonItemsControl") == 1,
            "Reopening trigger FloatButtonGroup should reuse menu content, not duplicate it.",
            failures);
        Expect(ReferenceEquals(surface, FindVisualByName<Border>(root, "PART_MenuSurface")),
            "Reopening trigger FloatButtonGroup should reuse the same menu surface.",
            failures);
    }

    private static void VerifyDefaultFloatButtonGroupStillCreatesItems(ICollection<string> failures)
    {
        var host = CreateDefaultGroupHost(FloatButtonShape.Circle);
        var root = CreateFloatButtonOverlay(host);
        using var _ = RealizeControl(root);

        Expect(CountVisualByTypeName(root, "FloatButtonItemsControl") == 1,
            "Default FloatButtonGroup should still create FloatButtonItemsControl immediately.",
            failures);
        Expect(CountVisualByTypeName(root, "FloatButton") >= 3,
            "Default FloatButtonGroup should still materialize child FloatButton visuals.",
            failures);
    }

    private static void VerifyFloatButtonGroupChildStateSync(ICollection<string> failures)
    {
        var host     = CreateDefaultGroupHost(FloatButtonShape.Square);
        var children = host.Children.OfType<AbstractFloatButton>().ToList();
        var root     = CreateFloatButtonOverlay(host);

        using (var realized = RealizeControl(root))
        {
            Expect(children.All(child => child.Shape == FloatButtonShape.Square),
                "FloatButtonGroup should sync Shape to embedded child buttons while attached.",
                failures);

            host.Shape = FloatButtonShape.Circle;
            RefreshLayout(realized.Window);
            Expect(children.All(child => child.Shape == FloatButtonShape.Circle),
                "FloatButtonGroup should resync embedded child button Shape after group Shape changes.",
                failures);
        }

        Expect(children.All(child => child.Shape == FloatButtonShape.Circle),
            "Detached FloatButtonGroup should clear embedded child Shape overrides back to defaults.",
            failures);
    }
}
