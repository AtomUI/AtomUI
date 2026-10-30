using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using Avalonia.Controls;
using Avalonia.VisualTree;

namespace AtomUI.Performance;

internal static partial class Program
{
    private static bool RunWindowTitleBarStateVerification()
    {
        var failures = new List<string>();
        VerifyCaptionButtonGroupVisibilityStates(failures);
        VerifyCaptionButtonSingleIconPresenter(failures);

        if (failures.Count == 0)
        {
            Console.WriteLine("WindowTitleBar state verification passed.");
            return true;
        }

        Console.Error.WriteLine("WindowTitleBar state verification failed:");
        foreach (var failure in failures)
        {
            Console.Error.WriteLine($"- {failure}");
        }
        return false;
    }

    private static void VerifyCaptionButtonGroupVisibilityStates(ICollection<string> failures)
    {
        var group = CreateCaptionButtonGroup(OsType.Windows);

        using var realized = RealizeControl(group);
        ExpectCaptionButtonVisibility(group, true, true, true, true, true, "default Windows caption group", failures);

        group.HostWindowState = WindowState.Maximized;
        RefreshLayout(realized.Window);
        ExpectCaptionButtonVisibility(group, false, true, true, true, true, "maximized Windows caption group", failures);

        group.HostWindowState = WindowState.FullScreen;
        RefreshLayout(realized.Window);
        ExpectCaptionButtonVisibility(group, true, true, false, false, true, "fullscreen Windows caption group", failures);

        group.IsFullScreenCaptionButtonVisible = false;
        RefreshLayout(realized.Window);
        ExpectCaptionButtonVisibility(group, false, true, false, false, true, "fullscreen button disabled", failures);

        group.HostWindowState                 = WindowState.Normal;
        group.IsMaximizeCaptionButtonVisible  = false;
        group.IsMinimizeCaptionButtonVisible  = false;
        group.IsPinCaptionButtonVisible       = false;
        group.IsCloseCaptionButtonVisible     = false;
        RefreshLayout(realized.Window);
        ExpectCaptionButtonVisibility(group, false, false, false, false, false, "all optional caption buttons disabled", failures);
    }

    private static void VerifyCaptionButtonSingleIconPresenter(ICollection<string> failures)
    {
        VerifyCaptionButtonSingleIconPresenter(OsType.Windows, "Windows caption group", failures);
        VerifyCaptionButtonSingleIconPresenter(OsType.Linux, "Linux caption group", failures);
        VerifyCaptionButtonSingleIconPresenter(OsType.macOS, "macOS caption group", failures);
    }

    private static void VerifyCaptionButtonSingleIconPresenter(
        OsType osType,
        string label,
        ICollection<string> failures)
    {
        var group = CreateCaptionButtonGroup(osType);

        using var realized = RealizeControl(group);
        var buttons = group.GetVisualDescendants()
                           .OfType<CaptionButton>()
                           .ToArray();
        Expect(buttons.Length > 0,
            $"{label}: caption buttons should be realized.",
            failures);

        foreach (var button in buttons)
        {
            var iconPresenters = button.GetVisualDescendants()
                                       .OfType<IconPresenter>()
                                       .ToArray();
            Expect(iconPresenters.Length == 1,
                $"{label}: {button.Name ?? button.GetType().Name} should use exactly one IconPresenter.",
                failures);
            if (iconPresenters.Length != 1)
            {
                continue;
            }

            var presenter = iconPresenters[0];
            Expect(ReferenceEquals(presenter.Icon, button.NormalIcon),
                $"{label}: {button.Name ?? button.GetType().Name} should start with its normal icon.",
                failures);

            if (button.CheckedIcon is null)
            {
                continue;
            }

            button.IsChecked = true;
            RefreshLayout(realized.Window);
            Expect(ReferenceEquals(presenter.Icon, button.CheckedIcon),
                $"{label}: {button.Name ?? button.GetType().Name} should switch to checked icon.",
                failures);

            button.IsChecked = false;
            RefreshLayout(realized.Window);
            Expect(ReferenceEquals(presenter.Icon, button.NormalIcon),
                $"{label}: {button.Name ?? button.GetType().Name} should switch back to normal icon.",
                failures);
        }
    }

    private static void ExpectCaptionButtonVisibility(
        CaptionButtonGroup group,
        bool fullScreen,
        bool pin,
        bool minimize,
        bool maximize,
        bool close,
        string label,
        ICollection<string> failures)
    {
        Expect(group.IsFullScreenButtonEffectivelyVisible == fullScreen,
            $"{label}: FullScreen effective visibility should be {fullScreen}.",
            failures);
        Expect(group.IsMinimizeButtonEffectivelyVisible == minimize,
            $"{label}: Minimize effective visibility should be {minimize}.",
            failures);
        Expect(group.IsMaximizeButtonEffectivelyVisible == maximize,
            $"{label}: Maximize effective visibility should be {maximize}.",
            failures);

        ExpectButtonVisible(group, "PART_FullScreenButton", fullScreen, label, failures);
        ExpectButtonVisible(group, "PART_PinButton", pin, label, failures);
        ExpectButtonVisible(group, "PART_MinimizeButton", minimize, label, failures);
        ExpectButtonVisible(group, "PART_MaximizeButton", maximize, label, failures);
        ExpectButtonVisible(group, "PART_CloseButton", close, label, failures);
    }

    private static void ExpectButtonVisible(
        CaptionButtonGroup group,
        string partName,
        bool expected,
        string label,
        ICollection<string> failures)
    {
        var button = FindVisualByName<CaptionButton>(group, partName);
        Expect(button != null,
            $"{label}: {partName} should remain in the template.",
            failures);
        Expect(button?.IsVisible == expected,
            $"{label}: {partName} visibility should be {expected}, actual {button?.IsVisible.ToString() ?? "<missing>"}.",
            failures);
        if (button is WindowsCaptionButton windowsButton)
        {
            Expect(IsZeroCornerRadius(windowsButton.EffectiveCornerRadius),
                $"{label}: {partName} should keep zero Windows caption corner radius.",
                failures);
        }
    }
}
