using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.VisualTree;

namespace AtomUI.Performance;

internal static partial class Program
{
    private static bool RunCheckBoxStateVerification()
    {
        var failures = new List<string>();
        VerifyCheckBoxIndicatorSlots(failures);
        VerifyCheckBoxContentPresenterLifecycle(failures);
        VerifyCheckBoxGroupSelectionSync(failures);

        if (failures.Count == 0)
        {
            Console.WriteLine("CheckBox state verification passed.");
            return true;
        }

        Console.Error.WriteLine("CheckBox state verification failed:");
        foreach (var failure in failures)
        {
            Console.Error.WriteLine($"- {failure}");
        }
        return false;
    }

    private static void VerifyCheckBoxIndicatorSlots(ICollection<string> failures)
    {
        var checkBox = new AtomUI.Desktop.Controls.CheckBox { Content = "Checkbox", IsChecked = false };
        using var realized = RealizeFeedbackControl(checkBox);
        var indicator = FindVisualByTypeName(checkBox, "CheckBoxIndicator", "Indicator");
        Expect(indicator is not null, "CheckBox should provide its indicator.", failures);
        var checkedMark = FindVisualByName<Control>(checkBox, "CheckedMark");
        var tristateMark = FindVisualByName<Control>(checkBox, "TristateMark");
        Expect(checkedMark?.IsVisible == false && tristateMark?.IsVisible == false,
            "An unchecked CheckBox should hide both marks.", failures);
        Expect(FindInteractionAdornment(checkBox) is null,
            "An idle CheckBox should have no feedback visual.", failures);

        checkBox.IsChecked = true;
        RefreshLayout(realized.Window);
        Expect(checkedMark?.IsVisible == true && tristateMark?.IsVisible == false,
            "A checked CheckBox should show only the check mark.", failures);
        Expect(FindInteractionAdornment(checkBox) is null, "Assigning IsChecked should not start feedback.", failures);
        VerifyCheckedMarkLayout(checkBox, checkedMark, failures);

        checkBox.IsChecked = false;
        RefreshLayout(realized.Window);
        if (indicator is not null)
        {
            ClickControl(realized.Window, indicator, failures, "CheckBox indicator");
            var wave = FindInteractionAdornment(checkBox);
            Expect(checkBox.IsChecked == true && wave is not null,
                "A user checking the box should change the value and attach feedback.", failures);
            checkBox.IsInteractionEffectEnabled = false;
            RefreshLayout(realized.Window);
            Expect(FindInteractionAdornment(checkBox) is null && wave?.GetVisualParent() is null,
                "Disabling CheckBox feedback should release its decoration.", failures);
        }

        checkBox.IsChecked = null;
        RefreshLayout(realized.Window);
        Expect(checkedMark?.IsVisible == false && tristateMark?.IsVisible == true,
            "An indeterminate CheckBox should show only the tristate mark.", failures);
    }

    private static void VerifyCheckBoxContentPresenterLifecycle(ICollection<string> failures)
    {
        var box = new AtomUI.Desktop.Controls.CheckBox();
        using var realized = RealizeControl(box);
        var presenter = FindVisualByName<ContentPresenter>(box, "ContentPresenter");
        Expect(presenter?.Child is null, "A contentless CheckBox should have no content visual.", failures);

        box.Content = "Runtime";
        RefreshLayout(realized.Window);
        presenter = FindVisualByName<ContentPresenter>(box, "ContentPresenter");
        var child = presenter?.Child;
        Expect(child is not null, "Assigning CheckBox content should create its visual.", failures);

        box.Content = null;
        RefreshLayout(realized.Window);
        Expect((FindVisualByName<ContentPresenter>(box, "ContentPresenter")?.Child is null) && child?.GetVisualParent() is null,
            "Clearing CheckBox content should release the old visual.", failures);
    }

    private static void VerifyCheckBoxGroupSelectionSync(ICollection<string> failures)
    {
        var apple = new CheckBoxOption { Content = "Apple" };
        var pear  = new CheckBoxOption { Content = "Pear", IsChecked = true };
        var group = new AtomUI.Desktop.Controls.CheckBoxGroup
        {
            ItemsSource  = new[] { apple, pear },
            CheckedItems = new List<CheckBoxOption> { pear }
        };

        using var realized = RealizeControl(group);
        Expect(GetOptionCheckBox(group, apple)?.IsChecked == false,
            "CheckBoxGroup should initialize unchecked option as false.",
            failures);
        Expect(GetOptionCheckBox(group, pear)?.IsChecked == true,
            "CheckBoxGroup should initialize checked option as true.",
            failures);

        group.CheckedItems = new List<CheckBoxOption> { apple };
        RefreshLayout(realized.Window);
        Expect(GetOptionCheckBox(group, apple)?.IsChecked == true,
            "CheckBoxGroup should check newly selected item when CheckedItems is replaced.",
            failures);
        Expect(GetOptionCheckBox(group, pear)?.IsChecked == false,
            "CheckBoxGroup should clear stale checked item when CheckedItems is replaced.",
            failures);

        group.CheckedItems = null;
        RefreshLayout(realized.Window);
        Expect(GetOptionCheckBox(group, apple)?.IsChecked == false &&
               GetOptionCheckBox(group, pear)?.IsChecked == false,
            "CheckBoxGroup should clear all item states when CheckedItems becomes null.",
            failures);
    }

    private static AtomUI.Desktop.Controls.CheckBox? GetOptionCheckBox(Control root, CheckBoxOption option)
    {
        return root.GetSelfAndVisualDescendants()
                   .OfType<AtomUI.Desktop.Controls.CheckBox>()
                   .FirstOrDefault(checkBox => ReferenceEquals(checkBox.Content, option) ||
                                               ReferenceEquals(checkBox.Content, option.Content) ||
                                               Equals(checkBox.Content, option.Content));
    }

    private static void VerifyCheckedMarkLayout(
        Control checkBox,
        Control? checkedMark,
        ICollection<string> failures)
    {
        var indicator = FindVisualByTypeName(checkBox, "CheckBoxIndicator", "Indicator");
        if (indicator == null || checkedMark == null)
        {
            return;
        }

        var markCenter = checkedMark.TranslatePoint(
            new Avalonia.Point(checkedMark.Bounds.Width / 2, checkedMark.Bounds.Height / 2),
            indicator);
        if (!markCenter.HasValue)
        {
            failures.Add("CheckBox checked mark should be transformable to its indicator.");
            return;
        }

        var indicatorCenter = new Avalonia.Point(
            indicator.Bounds.Width / 2,
            indicator.Bounds.Height / 2);
        Expect(Math.Abs(markCenter.Value.X - indicatorCenter.X) < 0.75 &&
               Math.Abs(markCenter.Value.Y - indicatorCenter.Y) < 0.75,
            $"CheckBox checked mark should be centered in indicator, actual ({markCenter.Value.X:0.###}, {markCenter.Value.Y:0.###}), expected ({indicatorCenter.X:0.###}, {indicatorCenter.Y:0.###}).",
            failures);
    }
}
