using AtomUI.Icons.AntDesign;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls.Presenters;
using Avalonia.Media;
using Avalonia.VisualTree;
using AtomSwitch = AtomUI.Desktop.Controls.ToggleSwitch;

namespace AtomUI.Performance;

internal static partial class Program
{
    private static bool RunSwitchStateVerification()
    {
        var failures = new List<string>();
        VerifySwitchTemplateVisualShape(failures);
        VerifySwitchContentPresenterBehavior(failures);
        VerifySwitchIconContentSync(failures);
        VerifySwitchWaveTemplateContract(failures);
        VerifySwitchCheckedChangeKeepsMeasureValid(failures);
        VerifySwitchDisabledWaveDoesNotPlay(failures);
        VerifySwitchLoadingLifecycle(failures);
        VerifySwitchDisabledStateConverges(failures);
        VerifySwitchKnobBoxShadowLifecycle(failures);

        if (failures.Count == 0)
        {
            Console.WriteLine("Switch state verification passed.");
            return true;
        }

        Console.Error.WriteLine("Switch state verification failed:");
        foreach (var failure in failures)
        {
            Console.Error.WriteLine($"- {failure}");
        }
        return false;
    }

    private static void VerifySwitchTemplateVisualShape(ICollection<string> failures)
    {
        var toggleSwitch = new AtomSwitch();
        using var _ = RealizeControl(toggleSwitch);

        Expect(FindVisualByTypeName(toggleSwitch, "SwitchTrack", "PART_Track") != null,
            "Switch template should realize its replaceable track.",
            failures);
        var onPresenter = FindVisualByName<ContentPresenter>(toggleSwitch, "PART_OnContentPresenter");
        Expect(onPresenter != null,
            "Switch template should realize PART_OnContentPresenter.",
            failures);
        Expect(onPresenter?.Content == null && onPresenter?.IsVisible == false,
            "Contentless Switch OnContent presenter should stay present but hidden.",
            failures);
        var offPresenter = FindVisualByName<ContentPresenter>(toggleSwitch, "PART_OffContentPresenter");
        Expect(offPresenter != null,
            "Switch template should realize PART_OffContentPresenter.",
            failures);
        Expect(offPresenter?.Content == null && offPresenter?.IsVisible == false,
            "Contentless Switch OffContent presenter should stay present but hidden.",
            failures);
        Expect(CountVisualByTypeName(toggleSwitch, "SwitchKnob") == 1,
            $"Switch should keep one SwitchKnob, actual {CountVisualByTypeName(toggleSwitch, "SwitchKnob")}.",
            failures);
    }

    private static void VerifySwitchContentPresenterBehavior(ICollection<string> failures)
    {
        var toggleSwitch = new AtomSwitch();
        using var realized = RealizeControl(toggleSwitch);
        var initialOnPresenter = FindVisualByName<ContentPresenter>(toggleSwitch, "PART_OnContentPresenter");
        var initialOffPresenter = FindVisualByName<ContentPresenter>(toggleSwitch, "PART_OffContentPresenter");

        toggleSwitch.OnContent = "On";
        RefreshLayout(realized.Window);
        var onPresenter = FindVisualByName<ContentPresenter>(toggleSwitch, "PART_OnContentPresenter");
        Expect(ReferenceEquals(onPresenter, initialOnPresenter),
            "Switch should keep the template OnContent presenter when OnContent is set.",
            failures);
        Expect(onPresenter?.TemplatedParent == toggleSwitch,
            "Switch OnContent presenter should use the switch as TemplatedParent.",
            failures);
        Expect(Equals(onPresenter?.Content, "On"),
            $"Switch OnContent presenter should mirror OnContent, actual {onPresenter?.Content ?? "<null>"}.",
            failures);
        Expect(onPresenter?.IsVisible == true,
            "Switch OnContent presenter should become visible when OnContent is set.",
            failures);

        toggleSwitch.OffContent = "Off";
        RefreshLayout(realized.Window);
        var offPresenter = FindVisualByName<ContentPresenter>(toggleSwitch, "PART_OffContentPresenter");
        Expect(ReferenceEquals(offPresenter, initialOffPresenter),
            "Switch should keep the template OffContent presenter when OffContent is set.",
            failures);
        Expect(offPresenter?.TemplatedParent == toggleSwitch,
            "Switch OffContent presenter should use the switch as TemplatedParent.",
            failures);
        Expect(Equals(offPresenter?.Content, "Off"),
            $"Switch OffContent presenter should mirror OffContent, actual {offPresenter?.Content ?? "<null>"}.",
            failures);
        Expect(offPresenter?.IsVisible == true,
            "Switch OffContent presenter should become visible when OffContent is set.",
            failures);

        toggleSwitch.OnContent = null;
        RefreshLayout(realized.Window);
        onPresenter = FindVisualByName<ContentPresenter>(toggleSwitch, "PART_OnContentPresenter");
        Expect(ReferenceEquals(onPresenter, initialOnPresenter),
            "Switch should keep PART_OnContentPresenter when OnContent is cleared.",
            failures);
        Expect(onPresenter?.Content == null && onPresenter?.IsVisible == false,
            "Cleared Switch OnContent presenter should clear content and become hidden.",
            failures);

        toggleSwitch.OffContent = null;
        RefreshLayout(realized.Window);
        offPresenter = FindVisualByName<ContentPresenter>(toggleSwitch, "PART_OffContentPresenter");
        Expect(ReferenceEquals(offPresenter, initialOffPresenter),
            "Switch should keep PART_OffContentPresenter when OffContent is cleared.",
            failures);
        Expect(offPresenter?.Content == null && offPresenter?.IsVisible == false,
            "Cleared Switch OffContent presenter should clear content and become hidden.",
            failures);
    }

    private static void VerifySwitchIconContentSync(ICollection<string> failures)
    {
        var onIcon = new CheckOutlined();
        var offIcon = new CloseOutlined();
        var toggleSwitch = new AtomSwitch
        {
            OnContent  = onIcon,
            OffContent = offIcon
        };

        using var realized = RealizeControl(toggleSwitch);
        var iconSize = GetSwitchIconSize(toggleSwitch);
        Expect(MathUtils.AreClose(onIcon.Width, iconSize) &&
               MathUtils.AreClose(onIcon.Height, iconSize),
            $"Switch OnContent icon should receive IconSize {iconSize}, actual {onIcon.Width}x{onIcon.Height}.",
            failures);
        Expect(MathUtils.AreClose(offIcon.Width, iconSize) &&
               MathUtils.AreClose(offIcon.Height, iconSize),
            $"Switch OffContent icon should receive IconSize {iconSize}, actual {offIcon.Width}x{offIcon.Height}.",
            failures);

        toggleSwitch.Foreground = Brushes.Coral;
        RefreshLayout(realized.Window);
        Expect(BrushEquals(onIcon.Foreground, Brushes.Coral) &&
               BrushEquals(offIcon.Foreground, Brushes.Coral),
            "Switch icon content should mirror Foreground.",
            failures);
        toggleSwitch.SizeType = CustomizableSizeType.Small;
        RefreshLayout(realized.Window);
        iconSize = GetSwitchIconSize(toggleSwitch);
        Expect(MathUtils.AreClose(onIcon.Width, iconSize) &&
               MathUtils.AreClose(onIcon.Height, iconSize),
            $"Switch OnContent icon should update after SizeType changes, actual {onIcon.Width}x{onIcon.Height}.",
            failures);

        toggleSwitch.OnContent = null;
        RefreshLayout(realized.Window);
        Expect(onIcon.GetVisualParent() == null,
            "Cleared Switch OnContent icon should not keep a visual parent.",
            failures);
        Expect(double.IsNaN(onIcon.Width) && double.IsNaN(onIcon.Height),
            $"Cleared Switch OnContent icon bindings should be disposed, actual {onIcon.Width}x{onIcon.Height}.",
            failures);
    }

    private static void VerifySwitchWaveTemplateContract(ICollection<string> failures)
    {
        var toggleSwitch = new AtomSwitch();
        using var realized = RealizeFeedbackControl(toggleSwitch);
        Expect(FindInteractionAdornment(toggleSwitch) is null, "Idle Switch should have no feedback visual.", failures);

        toggleSwitch.IsChecked = true;
        RefreshLayout(realized.Window);
        Expect(FindInteractionAdornment(toggleSwitch) is null, "Programmatic Switch state changes should not trigger feedback.", failures);

        ClickControl(realized.Window, toggleSwitch, failures, "Switch feedback");
        var active = FindInteractionAdornment(toggleSwitch);
        Expect(active is not null, "User activation should attach Switch feedback.", failures);

        toggleSwitch.IsInteractionEffectEnabled = false;
        RefreshLayout(realized.Window);
        Expect(FindInteractionAdornment(toggleSwitch) is null && active?.GetVisualParent() is null,
            "Disabling Switch feedback should detach the active decoration.", failures);
    }

    private static void VerifySwitchCheckedChangeKeepsMeasureValid(ICollection<string> failures)
    {
        var toggleSwitch = new AtomSwitch
        {
            OnContent  = "Enabled",
            OffContent = "Disabled"
        };
        using var realized = RealizeControl(toggleSwitch);
        RefreshLayout(realized.Window);

        var uncheckedSize = toggleSwitch.DesiredSize;
        Expect(toggleSwitch.IsMeasureValid,
            "Switch should be measure-valid after initial layout.",
            failures);

        toggleSwitch.IsMotionEnabled = false;
        toggleSwitch.IsChecked = true;
        Expect(toggleSwitch.IsMeasureValid,
            "Switch checked-state change should not invalidate measure because switch size is state-independent.",
            failures);

        RefreshLayout(realized.Window);
        Expect(MathUtils.AreClose(toggleSwitch.DesiredSize.Width, uncheckedSize.Width) &&
               MathUtils.AreClose(toggleSwitch.DesiredSize.Height, uncheckedSize.Height),
            $"Switch checked-state change should keep DesiredSize {uncheckedSize}, actual {toggleSwitch.DesiredSize}.",
            failures);

        var knob = FindVisualByTypeName(toggleSwitch, "SwitchKnob", "PART_SwitchKnob");
        var position = knob?.TranslatePoint(default, toggleSwitch);
        Expect(position is { } point && point.X > toggleSwitch.TrackPadding &&
               point.X + knob!.Bounds.Width <= toggleSwitch.Bounds.Width,
            "The checked knob should land on the far end of the arranged track.", failures);
    }

    private static void VerifySwitchDisabledWaveDoesNotPlay(ICollection<string> failures)
    {
        var toggleSwitch = new AtomSwitch { IsInteractionEffectEnabled = false };
        using var realized = RealizeFeedbackControl(toggleSwitch);
        ClickControl(realized.Window, toggleSwitch, failures, "Switch with disabled feedback");
        Expect(toggleSwitch.IsChecked == true && FindInteractionAdornment(toggleSwitch) is null,
            "Disabling feedback should keep the Switch operation and skip its decoration.", failures);
    }

    private static void VerifySwitchLoadingLifecycle(ICollection<string> failures)
    {
        var toggleSwitch = new AtomSwitch
        {
            IsLoading = true
        };

        var realized = RealizeControl(toggleSwitch);
        var knob = FindVisualByTypeName(toggleSwitch, "SwitchKnob", "PART_SwitchKnob");
        Expect(knob != null,
            "Loading Switch should realize PART_SwitchKnob.",
            failures);
        Expect(GetPrivateField(knob!, "AtomUI.Controls.SwitchKnob", "_loadingMotionGate") != null,
            "Loading SwitchKnob should start a loading motion gate.",
            failures);
        Expect(toggleSwitch.Cursor?.ToString() == "Arrow",
            $"Loading Switch should use Arrow cursor, actual {toggleSwitch.Cursor?.ToString() ?? "<null>"}.",
            failures);

        toggleSwitch.IsLoading = false;
        RefreshLayout(realized.Window);
        Expect(GetPrivateField(knob!, "AtomUI.Controls.SwitchKnob", "_loadingMotionGate") == null,
            "SwitchKnob should dispose its loading motion gate when loading stops.",
            failures);
        Expect(toggleSwitch.Cursor?.ToString() == "Hand",
            $"Non-loading Switch should fall back to Hand cursor, actual {toggleSwitch.Cursor?.ToString() ?? "<null>"}.",
            failures);

        toggleSwitch.IsLoading = true;
        RefreshLayout(realized.Window);
        Expect(GetPrivateField(knob!, "AtomUI.Controls.SwitchKnob", "_loadingMotionGate") != null,
            "SwitchKnob should recreate its loading motion gate when loading restarts.",
            failures);
        Expect(toggleSwitch.Cursor?.ToString() == "Arrow",
            $"Restarted loading Switch should use Arrow cursor, actual {toggleSwitch.Cursor?.ToString() ?? "<null>"}.",
            failures);

        realized.Dispose();
        Expect(GetPrivateField(knob!, "AtomUI.Controls.SwitchKnob", "_loadingMotionGate") == null,
            "Detached loading SwitchKnob should cancel and clear its loading motion gate.",
            failures);
    }

    private static void VerifySwitchDisabledStateConverges(ICollection<string> failures)
    {
        var toggleSwitch = new AtomSwitch();
        using var realized = RealizeControl(toggleSwitch);

        toggleSwitch.IsMotionEnabled = false;
        toggleSwitch.IsChecked = true;
        toggleSwitch.IsEnabled = false;
        RefreshLayout(realized.Window);

        var knob = FindVisualByTypeName(toggleSwitch, "SwitchKnob", "PART_SwitchKnob");
        var position = knob?.TranslatePoint(default, toggleSwitch);
        Expect(position is { } point && point.X > toggleSwitch.TrackPadding &&
               point.X + knob!.Bounds.Width <= toggleSwitch.Bounds.Width,
            "The checked knob should land on the far end of the arranged track.", failures);
    }

    private static void VerifySwitchKnobBoxShadowLifecycle(ICollection<string> failures)
    {
        var toggleSwitch = new AtomSwitch();
        using var realized = RealizeControl(toggleSwitch);
        var knob = FindVisualByTypeName(toggleSwitch, "SwitchKnob", "PART_SwitchKnob");
        Expect(knob != null,
            "Switch should realize PART_SwitchKnob for box-shadow verification.",
            failures);
        Expect(knob?.Effect != null,
            "SwitchKnob should apply initial KnobBoxShadow effect.",
            failures);

        knob?.GetType().GetProperty("KnobBoxShadow")?.SetValue(knob, null);
        RefreshLayout(realized.Window);
        Expect(knob?.Effect == null,
            "SwitchKnob should clear Effect when KnobBoxShadow is cleared.",
            failures);
    }

    private static double GetSwitchIconSize(AtomSwitch toggleSwitch)
    {
        return GetNonPublicProperty(toggleSwitch, "AtomUI.Controls.AbstractToggleSwitch", "IconSize") is double iconSize
            ? iconSize
            : double.NaN;
    }
}
