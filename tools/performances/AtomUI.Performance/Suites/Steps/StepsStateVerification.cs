using AtomUI.Desktop.Controls;
using AtomUI.Icons.AntDesign;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.VisualTree;

namespace AtomUI.Performance;

internal static partial class Program
{
    private static bool RunStepsStateVerification()
    {
        var failures = new List<string>();
        VerifyStepsPanelDefinitions(failures);
        VerifyStepsProgressState(failures);
        VerifyStepsStatusMatrix(failures);
        VerifyStepsIndicatorTemplateShape(failures);

        if (failures.Count == 0)
        {
            Console.WriteLine("Steps state verification passed.");
            return true;
        }

        Console.Error.WriteLine("Steps state verification failed:");
        foreach (var failure in failures)
        {
            Console.Error.WriteLine($"- {failure}");
        }
        return false;
    }

    private static void VerifyStepsPanelDefinitions(ICollection<string> failures)
    {
        var steps = CreateSteps(orientation: Orientation.Vertical);

        using var realized = RealizeControl(steps);
        var grid = GetStepsItemsGrid(steps);
        Expect(grid?.ColumnDefinitions.Count == 1,
            $"Vertical Steps should start with one column definition, actual {grid?.ColumnDefinitions.Count}.",
            failures);

        steps.Orientation = Orientation.Horizontal;
        RefreshLayout(realized.Window);
        Expect(grid?.ColumnDefinitions.Count == 3,
            $"Horizontal Steps should create one column per item, actual {grid?.ColumnDefinitions.Count}.",
            failures);

        steps.Orientation = Orientation.Vertical;
        RefreshLayout(realized.Window);
        Expect(grid?.ColumnDefinitions.Count == 1,
            $"Vertical Steps should clear old horizontal columns before adding its single column, actual {grid?.ColumnDefinitions.Count}.",
            failures);
    }

    private static void VerifyStepsProgressState(ICollection<string> failures)
    {
        var steps = CreateSteps(current: 1, percent: 50);

        using var realized = RealizeControl(steps);
        var items = GetRealizedStepsItems(steps);
        Expect(items.Count == 3,
            $"Steps progress verification should realize three items, actual {items.Count}.",
            failures);
        if (items.Count == 0)
        {
            return;
        }

        var first = items[0];
        Expect(GetNonPublicProperty<bool>(first, "IsProgressVisible"),
            "StepsItem should show progress on the current item when Percent is set and no custom icon is used.",
            failures);
        Expect(!GetNonPublicProperty<bool>(items[1], "IsProgressVisible") || items[1].Status == StepsStatus.Process,
            "StepsItem should not show progress on non-current items when Percent is set.",
            failures);

        first.Icon = new UserOutlined();
        RefreshLayout(realized.Window);
        Expect(!GetNonPublicProperty<bool>(first, "IsProgressVisible"),
            "StepsItem should hide progress when a custom icon is added.",
            failures);

        first.Icon = null;
        RefreshLayout(realized.Window);
        Expect(GetNonPublicProperty<bool>(first, "IsProgressVisible"),
            "StepsItem should re-enable progress when the custom icon is cleared.",
            failures);
    }

    private static void VerifyStepsStatusMatrix(ICollection<string> failures)
    {
        var steps = CreateSteps(current: 1);

        using var realized = RealizeControl(steps);
        var items = GetRealizedStepsItems(steps);
        Expect(items.Count == 3,
            $"Steps status verification should realize three items, actual {items.Count}.",
            failures);
        if (items.Count != 3)
        {
            return;
        }

        Expect(items[0].Status == StepsStatus.Finish,
            $"Step before current should be Finish, actual {items[0].Status}.",
            failures);
        Expect(items[1].Status == StepsStatus.Process,
            $"Current step should be Process, actual {items[1].Status}.",
            failures);
        Expect(items[2].Status == StepsStatus.Wait,
            $"Step after current should be Wait, actual {items[2].Status}.",
            failures);

        steps.Status = StepsStatus.Error;
        RefreshLayout(realized.Window);
        Expect(items[1].Status == StepsStatus.Error,
            $"Steps.Status change should update the current item, actual {items[1].Status}.",
            failures);

        steps.Current = 2;
        RefreshLayout(realized.Window);
        Expect(items[1].Status == StepsStatus.Finish,
            $"Previously current step should become Finish, actual {items[1].Status}.",
            failures);
        Expect(items[2].Status == StepsStatus.Error,
            $"New current step should inherit Steps.Status Error, actual {items[2].Status}.",
            failures);
    }

    private static void VerifyStepsIndicatorTemplateShape(ICollection<string> failures)
    {
        var steps = CreateSteps(current: 1);

        using var realized = RealizeControl(steps);
        Expect(CountNamedVisuals(steps, "FinishedMark") == 1,
            $"Steps default indicator should materialize one FinishedMark for the finished item, actual {CountNamedVisuals(steps, "FinishedMark")}.",
            failures);
        Expect(CountNamedVisuals(steps, "ErrorMark") == 0,
            $"Steps default indicator should not materialize ErrorMark when no item is in error, actual {CountNamedVisuals(steps, "ErrorMark")}.",
            failures);
        Expect(CountNamedVisuals(steps, "StepNumberText") >= 2,
            $"Steps default indicator should keep StepNumberText for wait/process items, actual {CountNamedVisuals(steps, "StepNumberText")}.",
            failures);
        Expect(CountNamedVisuals(steps, "CustomIconPresenter") == 0,
            $"Steps without custom icons should not materialize CustomIconPresenter, actual {CountNamedVisuals(steps, "CustomIconPresenter")}.",
            failures);

        steps.Status = StepsStatus.Error;
        RefreshLayout(realized.Window);
        Expect(CountNamedVisuals(steps, "ErrorMark") == 1,
            $"Steps error status should materialize one ErrorMark, actual {CountNamedVisuals(steps, "ErrorMark")}.",
            failures);

        var iconSteps = CreateIconSteps();
        using var _ = RealizeControl(iconSteps);
        Expect(CountNamedVisuals(iconSteps, "CustomIconPresenter") == 4,
            $"Icon Steps should materialize one CustomIconPresenter per icon item, actual {CountNamedVisuals(iconSteps, "CustomIconPresenter")}.",
            failures);
    }

    private static Grid? GetStepsItemsGrid(Steps steps)
    {
        return GetPrivateField(steps, "AtomUI.Desktop.Controls.Steps", "_grid") as Grid;
    }

    private static IReadOnlyList<StepsItem> GetRealizedStepsItems(Steps steps)
    {
        return steps.GetSelfAndVisualDescendants()
                    .OfType<StepsItem>()
                    .OrderBy(item => GetNonPublicProperty<int>(item, "StepNumber"))
                    .ToList();
    }

    private static int CountNamedVisuals(Control root, string name)
    {
        return root.GetSelfAndVisualDescendants()
                   .OfType<Control>()
                   .Count(control => control.Name == name);
    }
}
