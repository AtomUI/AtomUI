using AtomUI.Controls;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.VisualTree;
using AtomRadioButton = AtomUI.Desktop.Controls.RadioButton;
using AtomRadioButtonGroup = AtomUI.Desktop.Controls.RadioButtonGroup;

namespace AtomUI.Performance;

internal static partial class Program
{
    private static bool RunRadioButtonStateVerification()
    {
        var failures = new List<string>();
        VerifyRadioIndicatorWaveSlots(failures);
        VerifyRadioButtonGroupSelectionSync(failures);

        if (failures.Count == 0)
        {
            Console.WriteLine("RadioButton state verification passed.");
            return true;
        }

        Console.Error.WriteLine("RadioButton state verification failed:");
        foreach (var failure in failures)
        {
            Console.Error.WriteLine($"- {failure}");
        }
        return false;
    }

    private static void VerifyRadioIndicatorWaveSlots(ICollection<string> failures)
    {
        var uncheckedRadio = new AtomRadioButton
        {
            Content = "Unchecked"
        };
        using var uncheckedRealized = RealizeFeedbackControl(uncheckedRadio);
        Expect(FindVisualByTypeName(uncheckedRadio, "RadioIndicator", "Indicator") != null,
            "RadioButton should create RadioIndicator.",
            failures);
        Expect(FindInteractionAdornment(uncheckedRadio) == null,
            "Unchecked RadioButton should not create wave decorator before interaction.",
            failures);

        ClickControl(uncheckedRealized.Window, uncheckedRadio, failures, "RadioButton");
        RefreshLayout(uncheckedRealized.Window);
        var runtimeWave = FindInteractionAdornment(uncheckedRadio);
        Expect(runtimeWave != null,
            "RadioButton should create wave decorator when a checked interaction needs it.",
            failures);

        uncheckedRadio.SetCurrentValue(AtomRadioButton.IsInteractionEffectEnabledProperty, false);
        RefreshLayout(uncheckedRealized.Window);
        Expect(FindInteractionAdornment(uncheckedRadio) == null,
            "RadioButton should remove wave decorator when wave is disabled.",
            failures);
        Expect(runtimeWave?.GetVisualParent() == null,
            "Removed RadioButton wave decorator should not keep a visual parent.",
            failures);

        var motionRadio = new AtomRadioButton
        {
            Content = "Motion"
        };
        using var motionRealized = RealizeFeedbackControl(motionRadio);
        ClickControl(motionRealized.Window, motionRadio, failures, "RadioButton motion");
        RefreshLayout(motionRealized.Window);
        var motionWave = FindInteractionAdornment(motionRadio);
        Expect(motionWave != null,
            "RadioButton should create wave decorator before motion is disabled.",
            failures);

        motionRadio.SetCurrentValue(AtomRadioButton.IsMotionEnabledProperty, false);
        RefreshLayout(motionRealized.Window);
        Expect(FindInteractionAdornment(motionRadio) == null,
            "RadioButton should remove wave decorator when motion is disabled.",
            failures);
        Expect(motionWave?.GetVisualParent() == null,
            "Removed RadioButton motion wave decorator should not keep a visual parent.",
            failures);

        var checkedRadio = new AtomRadioButton
        {
            Content   = "Checked",
            IsChecked = true
        };
        using var checkedRealized = RealizeFeedbackControl(checkedRadio);
        Expect(FindInteractionAdornment(checkedRadio) == null,
            "Initially checked RadioButton should not create wave decorator without interaction.",
            failures);

        var detachedPanel = new StackPanel();
        var detachedRadio = new AtomRadioButton
        {
            Content = "Detached"
        };
        detachedPanel.Children.Add(detachedRadio);
        using var detachedRealized = RealizeFeedbackControl(detachedPanel);
        ClickControl(detachedRealized.Window, detachedRadio, failures, "detached RadioButton");
        RefreshLayout(detachedRealized.Window);
        var detachedWave = FindInteractionAdornment(detachedRadio);
        Expect(detachedWave != null,
            "RadioButton should create a wave decorator before detach verification.",
            failures);
        detachedPanel.Children.Remove(detachedRadio);
        RefreshLayout(detachedRealized.Window);
        Expect(detachedWave?.GetVisualParent() == null,
            "RadioButton should detach wave decorator when it leaves the visual tree.",
            failures);
        detachedRadio.SetCurrentValue(AtomRadioButton.IsCheckedProperty, false);
        detachedRadio.SetCurrentValue(AtomRadioButton.IsCheckedProperty, true);
        RefreshLayout(detachedRealized.Window);
        Expect(FindInteractionAdornment(detachedRadio) == null,
            "Detached RadioButton should not recreate wave decorator when checked state changes.",
            failures);
    }

    private static void VerifyRadioButtonGroupSelectionSync(ICollection<string> failures)
    {
        var optionA = new RadioButtonOption { Content = "Option A" };
        var optionB = new RadioButtonOption { Content = "Option B" };
        var optionC = new RadioButtonOption { Content = "Option C" };
        var group = new AtomRadioButtonGroup
        {
            ItemsSource = new[] { optionA, optionB, optionC },
            CheckedItem = optionB
        };

        using var realized = RealizeControl(group);
        var radioA = GetOptionRadioButton(group, optionA);
        var radioB = GetOptionRadioButton(group, optionB);
        var radioC = GetOptionRadioButton(group, optionC);
        Expect(radioA?.IsChecked == false,
            "RadioButtonGroup should initialize unchecked option as false.",
            failures);
        Expect(radioB?.IsChecked == true,
            "RadioButtonGroup should initialize CheckedItem option as true.",
            failures);
        Expect(radioC?.IsChecked == false,
            "RadioButtonGroup should keep non-selected options unchecked.",
            failures);

        group.CheckedItem = optionA;
        RefreshLayout(realized.Window);
        Expect(radioA?.IsChecked == true,
            "RadioButtonGroup should check newly selected item when CheckedItem changes.",
            failures);
        Expect(radioB?.IsChecked == false,
            "RadioButtonGroup should clear stale checked item when CheckedItem changes.",
            failures);

        group.CheckedItem = null;
        RefreshLayout(realized.Window);
        Expect(radioA?.IsChecked == false &&
               radioB?.IsChecked == false &&
               radioC?.IsChecked == false,
            "RadioButtonGroup should clear all item states when CheckedItem becomes null.",
            failures);
    }

    private static AtomRadioButton? GetOptionRadioButton(Control root, RadioButtonOption option)
    {
        return root.GetSelfAndVisualDescendants()
                   .OfType<AtomRadioButton>()
                   .FirstOrDefault(radioButton => ReferenceEquals(radioButton.DataContext, option));
    }

    private static void ClickControl(Avalonia.Controls.Window window,
                                     Control control,
                                     ICollection<string> failures,
                                     string label)
    {
        var point = GetControlCenterPoint(window, control);
        if (!point.HasValue)
        {
            failures.Add($"{label} center point should be available for pointer verification.");
            return;
        }

        window.MouseMove(point.Value);
        window.MouseDown(point.Value, MouseButton.Left);
        window.MouseUp(point.Value, MouseButton.Left);
    }
}
