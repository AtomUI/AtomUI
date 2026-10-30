using System.Reflection;
using AtomUI.Desktop.Controls;
using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Configuration;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;

namespace AtomUI.Performance;

using AvaloniaButton = Avalonia.Controls.Button;

internal static partial class Program
{
    private static readonly FieldInfo FlyoutOpenedEventField =
        typeof(FlyoutBase).GetField("Opened", BindingFlags.Instance | BindingFlags.NonPublic)
        ?? throw new InvalidOperationException("FlyoutBase.Opened event field was not found.");

    private static readonly FieldInfo FlyoutClosedEventField =
        typeof(FlyoutBase).GetField("Closed", BindingFlags.Instance | BindingFlags.NonPublic)
        ?? throw new InvalidOperationException("FlyoutBase.Closed event field was not found.");

    private static readonly FieldInfo FlyoutTargetField =
        typeof(FlyoutBase).GetField("_target", BindingFlags.Instance | BindingFlags.NonPublic)
        ?? throw new InvalidOperationException("FlyoutBase._target field was not found.");

    private static readonly MethodInfo FlyoutOnOpenedMethod =
        typeof(FlyoutBase).GetMethod("OnOpened", BindingFlags.Instance | BindingFlags.NonPublic)
        ?? throw new InvalidOperationException("FlyoutBase.OnOpened method was not found.");

    private static readonly MethodInfo FlyoutOnClosedMethod =
        typeof(FlyoutBase).GetMethod("OnClosed", BindingFlags.Instance | BindingFlags.NonPublic)
        ?? throw new InvalidOperationException("FlyoutBase.OnClosed method was not found.");

    private static bool RunSplitButtonStateVerification()
    {
        var failures = new List<string>();
        VerifySplitButtonFlyoutEventRegistrationLifecycle(failures);
        VerifySplitButtonSecondaryFlyoutState(failures);
        VerifySplitButtonPrimaryClick(failures);
        VerifySplitButtonSecondaryButtonRearrangesAfterCompactThemeToggle(failures);

        if (failures.Count == 0)
        {
            Console.WriteLine("SplitButton state verification passed.");
            return true;
        }

        Console.Error.WriteLine("SplitButton state verification failed:");
        foreach (var failure in failures)
        {
            Console.Error.WriteLine($"- {failure}");
        }
        return false;
    }

    private static void VerifySplitButtonFlyoutEventRegistrationLifecycle(ICollection<string> failures)
    {
        var firstFlyout = CreateSplitButtonMenuFlyout();
        var splitButton = new SplitButton
        {
            Content         = "Split",
            IsMotionEnabled = false,
            Flyout          = firstFlyout
        };

        Expect(GetFlyoutOpenedHandlerCount(firstFlyout) == 1,
            $"Detached SplitButton should only keep FlyoutStateHelper.Opened handler. Actual: {GetFlyoutOpenedHandlerCount(firstFlyout)}.",
            failures);
        Expect(GetFlyoutClosedHandlerCount(firstFlyout) == 1,
            $"Detached SplitButton should only keep FlyoutStateHelper.Closed handler. Actual: {GetFlyoutClosedHandlerCount(firstFlyout)}.",
            failures);

        var realized = RealizeControl(splitButton);
        var secondFlyout = CreateSplitButtonMenuFlyout();
        try
        {
            Expect(GetFlyoutOpenedHandlerCount(firstFlyout) == 2,
                $"Attached SplitButton should register exactly one own Opened handler plus helper. Actual: {GetFlyoutOpenedHandlerCount(firstFlyout)}.",
                failures);
            Expect(GetFlyoutClosedHandlerCount(firstFlyout) == 2,
                $"Attached SplitButton should register exactly one own Closed handler plus helper. Actual: {GetFlyoutClosedHandlerCount(firstFlyout)}.",
                failures);

            splitButton.Flyout = secondFlyout;
            RefreshLayout(realized.Window);

            Expect(GetFlyoutOpenedHandlerCount(firstFlyout) == 0,
                $"Replacing SplitButton.Flyout should remove all Opened handlers from old flyout. Actual: {GetFlyoutOpenedHandlerCount(firstFlyout)}.",
                failures);
            Expect(GetFlyoutClosedHandlerCount(firstFlyout) == 0,
                $"Replacing SplitButton.Flyout should remove all Closed handlers from old flyout. Actual: {GetFlyoutClosedHandlerCount(firstFlyout)}.",
                failures);
            Expect(GetFlyoutOpenedHandlerCount(secondFlyout) == 2,
                $"Replacement flyout should have exactly helper + SplitButton Opened handlers. Actual: {GetFlyoutOpenedHandlerCount(secondFlyout)}.",
                failures);
            Expect(GetFlyoutClosedHandlerCount(secondFlyout) == 2,
                $"Replacement flyout should have exactly helper + SplitButton Closed handlers. Actual: {GetFlyoutClosedHandlerCount(secondFlyout)}.",
                failures);
        }
        finally
        {
            realized.Dispose();
        }

        Expect(GetFlyoutOpenedHandlerCount(secondFlyout) == 1,
            $"Detached SplitButton should release its own Opened handler. Actual: {GetFlyoutOpenedHandlerCount(secondFlyout)}.",
            failures);
        Expect(GetFlyoutClosedHandlerCount(secondFlyout) == 1,
            $"Detached SplitButton should release its own Closed handler. Actual: {GetFlyoutClosedHandlerCount(secondFlyout)}.",
            failures);
    }

    private static void VerifySplitButtonSecondaryFlyoutState(ICollection<string> failures)
    {
        var flyout = CreateSplitButtonMenuFlyout();
        var splitButton = new SplitButton
        {
            Content               = "Split",
            IsMotionEnabled       = false,
            ShouldUseOverlayPopup = false,
            Flyout                = flyout
        };

        using var realized = RealizeControl(splitButton);
        var secondaryButton = FindVisualByName<AvaloniaButton>(splitButton, "PART_SecondaryButton");
        Expect(secondaryButton is not null,
            "SplitButton should materialize PART_SecondaryButton.",
            failures);
        if (secondaryButton is null)
        {
            return;
        }

        SetFlyoutTarget(flyout, secondaryButton);
        FlyoutOnOpenedMethod.Invoke(flyout, null);
        RefreshLayout(realized.Window);
        Expect(HasPseudoClass(splitButton, StdPseudoClass.FlyoutOpen),
            "SplitButton should set :flyout-open when its secondary button opens the flyout.",
            failures);

        FlyoutOnClosedMethod.Invoke(flyout, null);
        SetFlyoutTarget(flyout, null);
        RefreshLayout(realized.Window);
        Expect(!HasPseudoClass(splitButton, StdPseudoClass.FlyoutOpen),
            "SplitButton should clear :flyout-open when its secondary flyout closes.",
            failures);
    }

    private static void VerifySplitButtonPrimaryClick(ICollection<string> failures)
    {
        var clickCount = 0;
        var splitButton = new SplitButton
        {
            Content = "Split",
            Flyout  = CreateSplitButtonMenuFlyout()
        };
        splitButton.Click += (_, _) => clickCount++;

        using var _ = RealizeControl(splitButton);
        var primaryButton = FindVisualByName<AvaloniaButton>(splitButton, "PART_PrimaryButton");
        Expect(primaryButton is not null,
            "SplitButton should materialize PART_PrimaryButton.",
            failures);
        if (primaryButton is null)
        {
            return;
        }

        primaryButton.RaiseEvent(new RoutedEventArgs(AvaloniaButton.ClickEvent));
        Expect(clickCount == 1,
            $"Primary part click should raise SplitButton.Click once. Actual: {clickCount}.",
            failures);
    }

    private static void VerifySplitButtonSecondaryButtonRearrangesAfterCompactThemeToggle(ICollection<string> failures)
    {
        var application = Application.Current;
        var themeManager = application?.GetThemeManager();
        Expect(themeManager is not null,
            "SplitButton compact theme layout verification requires the AtomUI theme manager.",
            failures);
        if (themeManager is null)
        {
            return;
        }

        // SetCompactThemeMode 扩展已随旧 ThemeManager 移除：
        // 紧凑模式现在通过 ThemeAlgorithm.Compact 算法以 ApplyThemeAsync 应用。
        ApplyCompactThemeMode(themeManager, isCompact: false);
        var splitButton = new SplitButton
        {
            Content             = "Split",
            IsPrimaryButtonType = true,
            IsMotionEnabled     = false,
            SizeType            = CustomizableSizeType.Large,
            Flyout              = CreateSplitButtonMenuFlyout()
        };

        using var realized = RealizeControl(splitButton);
        var secondaryButton = FindVisualByName<AvaloniaButton>(splitButton, "PART_SecondaryButton");
        Expect(secondaryButton is not null,
            "SplitButton should materialize PART_SecondaryButton before compact theme layout verification.",
            failures);
        if (secondaryButton is null)
        {
            ApplyCompactThemeMode(themeManager, isCompact: false);
            return;
        }

        ApplyCompactThemeMode(themeManager, isCompact: true);
        RefreshLayout(realized.Window);

        Expect(Math.Abs(secondaryButton.Bounds.Right - splitButton.Bounds.Width) <= 0.001,
            $"Secondary button should be rearranged after compact theme is enabled. SplitButton width: {splitButton.Bounds.Width:0.###}, secondary right: {secondaryButton.Bounds.Right:0.###}.",
            failures);

        ApplyCompactThemeMode(themeManager, isCompact: false);
        RefreshLayout(realized.Window);

        Expect(Math.Abs(secondaryButton.Bounds.Right - splitButton.Bounds.Width) <= 0.001,
            $"Secondary button should be rearranged after compact theme is disabled. SplitButton width: {splitButton.Bounds.Width:0.###}, secondary right: {secondaryButton.Bounds.Right:0.###}.",
            failures);

        ApplyCompactThemeMode(themeManager, isCompact: false);
    }

    private static void ApplyCompactThemeMode(IThemeManager themeManager, bool isCompact)
    {
        var algorithms = isCompact
            ? new[] { ThemeAlgorithm.Default, ThemeAlgorithm.Compact }
            : new[] { ThemeAlgorithm.Default };
        var config = new ThemeConfigBuilder()
                     .WithAlgorithms(algorithms)
                     .Build();
        themeManager.ApplyThemeAsync(new ThemeRequest(
            themeManager.CurrentTheme?.ThemeId ?? IThemeManager.DEFAULT_THEME_ID,
            config,
            ThemeTransitionReason.UserRequest)).GetAwaiter().GetResult();
    }

    private static int GetFlyoutOpenedHandlerCount(FlyoutBase flyout)
    {
        return GetFlyoutEventHandlerCount(FlyoutOpenedEventField, flyout);
    }

    private static int GetFlyoutClosedHandlerCount(FlyoutBase flyout)
    {
        return GetFlyoutEventHandlerCount(FlyoutClosedEventField, flyout);
    }

    private static int GetFlyoutEventHandlerCount(FieldInfo eventField, FlyoutBase flyout)
    {
        return eventField.GetValue(flyout) is Delegate handler
            ? handler.GetInvocationList().Length
            : 0;
    }

    private static void SetFlyoutTarget(FlyoutBase flyout, Avalonia.Controls.Control? target)
    {
        FlyoutTargetField.SetValue(flyout, target);
    }
}
