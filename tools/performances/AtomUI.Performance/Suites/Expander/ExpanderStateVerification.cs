using AtomUI.Desktop.Controls;
using AtomUI.Icons.AntDesign;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.VisualTree;
using AtomExpander = AtomUI.Desktop.Controls.Expander;

namespace AtomUI.Performance;

internal static partial class Program
{
    private static bool RunExpanderStateVerification()
    {
        var failures = new List<string>();
        VerifyExpanderClosedSlotsAreLazy(failures);
        VerifyExpanderNoArrowLifecycle(failures);
        VerifyExpanderAddOnLifecycle(failures);
        VerifyExpanderContentMotionLifecycle(failures);
        VerifyExpanderRuntimeStateSync(failures);
        VerifyExpanderLazyPartsKeepTemplateStyles(failures);
        VerifyExpanderIconPositionRuntimeSync(failures);

        if (failures.Count == 0)
        {
            Console.WriteLine("Expander state verification passed.");
            return true;
        }

        Console.Error.WriteLine("Expander state verification failed:");
        foreach (var failure in failures)
        {
            Console.Error.WriteLine($"- {failure}");
        }
        return false;
    }

    private static void VerifyExpanderClosedSlotsAreLazy(ICollection<string> failures)
    {
        var expander = CreateVerificationExpander();
        using var _ = RealizeControl(expander);

        Expect(FindVisualByName<Control>(expander, "PART_ContentViewport") is { IsVisible: false },
            "Closed Expander should keep its static content viewport hidden.",
            failures);
        Expect(FindVisualByName<ContentPresenter>(expander, "PART_ContentPresenter") == null,
            "Closed Expander should not create PART_ContentPresenter before first expand.",
            failures);
        Expect(FindVisualByName<ContentPresenter>(expander, "PART_AddOnContentPresenter") == null,
            "Expander without AddOnContent should not create PART_AddOnContentPresenter.",
            failures);
        Expect(FindVisualByName<IconButton>(expander, "PART_ExpandButton") != null,
            "Default Expander should create PART_ExpandButton.",
            failures);
    }

    private static void VerifyExpanderNoArrowLifecycle(ICollection<string> failures)
    {
        var expander = CreateVerificationExpander();
        expander.IsShowExpandIcon = false;
        using var realized = RealizeControl(expander);

        Expect(FindVisualByName<IconButton>(expander, "PART_ExpandButton") == null,
            "No-arrow Expander should not create PART_ExpandButton.",
            failures);
        Expect(expander.ExpandIcon == null,
            "No-arrow Expander should not create the default expand icon.",
            failures);

        expander.IsShowExpandIcon = true;
        RefreshLayout(realized.Window);
        var firstButton = FindVisualByName<IconButton>(expander, "PART_ExpandButton");
        Expect(firstButton != null,
            "Expander should create PART_ExpandButton when IsShowExpandIcon becomes true.",
            failures);
        Expect(expander.ExpandIcon is RightOutlined,
            "Expander should create the default RightOutlined icon when expand button is needed.",
            failures);

        expander.IsShowExpandIcon = false;
        RefreshLayout(realized.Window);
        Expect(FindVisualByName<IconButton>(expander, "PART_ExpandButton") == null,
            "Expander should remove PART_ExpandButton when IsShowExpandIcon becomes false.",
            failures);
        Expect(firstButton?.GetVisualParent() == null,
            "Removed PART_ExpandButton should not keep a visual parent.",
            failures);
        Expect(firstButton == null || firstButton.TemplatedParent == null,
            "Removed PART_ExpandButton should clear templated parent.",
            failures);
        Expect(expander.ExpandIcon == null,
            "Expander should release the generated default expand icon when no arrow is shown.",
            failures);

        expander.IsShowExpandIcon = true;
        RefreshLayout(realized.Window);
        var secondButton = FindVisualByName<IconButton>(expander, "PART_ExpandButton");
        Expect(secondButton != null && !ReferenceEquals(firstButton, secondButton),
            "Expander should recreate PART_ExpandButton cleanly after it was removed.",
            failures);
    }

    private static void VerifyExpanderAddOnLifecycle(ICollection<string> failures)
    {
        var expander = CreateVerificationExpander();
        using var realized = RealizeControl(expander);

        Expect(FindVisualByName<ContentPresenter>(expander, "PART_AddOnContentPresenter") == null,
            "Expander without addon should not create addon presenter.",
            failures);

        var addOn = new SettingOutlined();
        expander.AddOnContent = addOn;
        RefreshLayout(realized.Window);
        var firstPresenter = FindVisualByName<ContentPresenter>(expander, "PART_AddOnContentPresenter");
        Expect(firstPresenter != null,
            "Expander should create addon presenter when AddOnContent is assigned.",
            failures);
        Expect(ReferenceEquals(firstPresenter?.Content, addOn),
            "Addon presenter should bind to AddOnContent.",
            failures);

        expander.AddOnContent = null;
        RefreshLayout(realized.Window);
        Expect(FindVisualByName<ContentPresenter>(expander, "PART_AddOnContentPresenter") == null,
            "Expander should remove addon presenter when AddOnContent is cleared.",
            failures);
        Expect(firstPresenter?.GetVisualParent() == null,
            "Removed addon presenter should not keep a visual parent.",
            failures);
        Expect(firstPresenter == null || firstPresenter.TemplatedParent == null,
            "Removed addon presenter should clear templated parent.",
            failures);
        Expect(firstPresenter?.Content == null,
            "Removed addon presenter should clear Content.",
            failures);
        Expect(addOn.GetVisualParent() == null,
            "Removed addon content should not keep a visual parent.",
            failures);

        expander.AddOnContent = new SettingOutlined();
        RefreshLayout(realized.Window);
        var secondPresenter = FindVisualByName<ContentPresenter>(expander, "PART_AddOnContentPresenter");
        Expect(secondPresenter != null && !ReferenceEquals(firstPresenter, secondPresenter),
            "Expander should recreate addon presenter cleanly after removal.",
            failures);
    }

    private static void VerifyExpanderContentMotionLifecycle(ICollection<string> failures)
    {
        var expander = CreateVerificationExpander();
        expander.SetValue(AtomExpander.IsMotionEnabledProperty, false, Avalonia.Data.BindingPriority.Animation);
        Control? firstViewport;
        using (var realized = RealizeControl(expander))
        {
            Expect(FindVisualByName<Control>(expander, "PART_ContentViewport") is { IsVisible: false },
                "Closed Expander should keep its static content viewport hidden.",
                failures);

            expander.IsExpanded = true;
            RefreshLayout(realized.Window);
            firstViewport = FindVisualByName<Control>(expander, "PART_ContentViewport");
            var firstPresenter = GetMaterializedExpanderContentPresenter(expander);
            Expect(firstViewport != null,
                "Expander should create content content viewport on first expand.",
                failures);
            Expect(firstPresenter != null,
                "Expander should create content presenter on first expand.",
                failures);

            expander.IsExpanded = false;
            RefreshLayout(realized.Window);
            Expect(CountExpanderVisualsByName(expander, "PART_ContentViewport") == 1,
                "Expander should keep one materialized content viewport after collapse, not duplicate it.",
                failures);

            expander.IsExpanded = true;
            RefreshLayout(realized.Window);
            var secondViewport = FindVisualByName<Control>(expander, "PART_ContentViewport");
            Expect(ReferenceEquals(firstViewport, secondViewport),
                "Expander should reuse the materialized content viewport on second expand.",
                failures);
            Expect(ReferenceEquals(firstPresenter, GetMaterializedExpanderContentPresenter(expander)),
                "Expander should not duplicate content presenter across expand/collapse toggles.",
                failures);
            Expect(firstPresenter == null || firstPresenter.TemplatedParent == expander,
                "Materialized content presenter should use Expander as templated parent.",
                failures);
        }

        Expect(firstViewport == null || TopLevel.GetTopLevel(firstViewport) == null,
            "Detached Expander should detach its content viewport from the visual root.",
            failures);
        Expect(firstViewport == null || Math.Abs(firstViewport.Opacity - 1) < 0.001,
            "Detached Expander should release Motion's opacity cover.",
            failures);
    }

    private static void VerifyExpanderRuntimeStateSync(ICollection<string> failures)
    {
        var expander = CreateVerificationExpander();
        using var realized = RealizeControl(expander);

        var frame = FindVisualByName<Border>(expander, "PART_Frame");
        expander.BorderThickness = new Thickness(5);
        RefreshLayout(realized.Window);
        Expect(frame?.BorderThickness == new Thickness(5),
            $"Expander should sync normal border thickness, actual {DescribeThickness(frame?.BorderThickness)}.",
            failures);

        expander.IsBorderless = true;
        RefreshLayout(realized.Window);
        Expect(frame?.BorderThickness == new Thickness(0),
            $"Expander.IsBorderless should update effective frame border at runtime, actual {DescribeThickness(frame?.BorderThickness)}.",
            failures);

        expander.IsBorderless = false;
        expander.IsGhostStyle = true;
        RefreshLayout(realized.Window);
        Expect(frame?.BorderThickness == new Thickness(0),
            $"Expander.IsGhostStyle should update effective frame border at runtime, actual {DescribeThickness(frame?.BorderThickness)}.",
            failures);

        expander.IsGhostStyle = false;
        RefreshLayout(realized.Window);
        Expect(frame?.BorderThickness == new Thickness(5),
            $"Expander should restore effective frame border when ghost/borderless are cleared, actual {DescribeThickness(frame?.BorderThickness)}.",
            failures);
    }

    private static void VerifyExpanderLazyPartsKeepTemplateStyles(ICollection<string> failures)
    {
        var expander = CreateVerificationExpander();
        expander.SetValue(AtomExpander.IsMotionEnabledProperty, false, Avalonia.Data.BindingPriority.Animation);
        expander.IsExpanded = true;
        using var realized = RealizeControl(expander);

        var headerDecorator = FindVisualByName<Border>(expander, "PART_HeaderDecorator");
        var contentPresenter = GetMaterializedExpanderContentPresenter(expander);
        Expect(headerDecorator?.Padding != default,
            $"Default header padding should still come from the Expander theme, actual {DescribeThickness(headerDecorator?.Padding)}.",
            failures);
        Expect(contentPresenter?.Padding != default,
            $"Lazy content presenter padding should still come from the Expander theme, actual {DescribeThickness(contentPresenter?.Padding)}.",
            failures);

        expander.HeaderPadding  = new Thickness(5);
        expander.ContentPadding = new Thickness(6);
        RefreshLayout(realized.Window);
        contentPresenter = GetMaterializedExpanderContentPresenter(expander);
        var fieldPresenter = GetPrivateField(expander, "AtomUI.Desktop.Controls.Expander", "_contentPresenter") as ContentPresenter;
        Expect(ReferenceEquals(fieldPresenter, contentPresenter),
            $"Materialized content presenter should match Expander._contentPresenter, field={DescribeThickness(fieldPresenter?.Padding)}, actor={DescribeThickness(contentPresenter?.Padding)}.",
            failures);
        Expect(expander.ContentPadding == new Thickness(6),
            $"Expander.ContentPadding should keep the assigned custom value, actual {DescribeThickness(expander.ContentPadding)}.",
            failures);
        Expect(headerDecorator?.Padding == new Thickness(5),
            $"Custom HeaderPadding should override theme padding, actual {DescribeThickness(headerDecorator?.Padding)}.",
            failures);
        Expect(contentPresenter?.Padding == new Thickness(6),
            $"Custom ContentPadding should override theme padding, actual {DescribeThickness(contentPresenter?.Padding)}.",
            failures);
    }

    private static void VerifyExpanderIconPositionRuntimeSync(ICollection<string> failures)
    {
        var expander = CreateVerificationExpander();
        expander.AddOnContent = new SettingOutlined();
        using var realized = RealizeControl(expander);

        var expandButton = FindVisualByName<IconButton>(expander, "PART_ExpandButton");
        var addOnPresenter = FindVisualByName<ContentPresenter>(expander, "PART_AddOnContentPresenter");
        if (expandButton is null || addOnPresenter is null)
        {
            Expect(expandButton is not null,
                "Expander should create PART_ExpandButton for icon position verification.",
                failures);
            Expect(addOnPresenter is not null,
                "Expander should create PART_AddOnContentPresenter for icon position verification.",
                failures);
            return;
        }

        Expect(Grid.GetColumn(expandButton) == 0,
            $"Expand icon should start in column 0, actual {Grid.GetColumn(expandButton)}.",
            failures);
        Expect(Grid.GetColumn(addOnPresenter) == 2,
            $"Addon presenter should stay in column 2, actual {Grid.GetColumn(addOnPresenter)}.",
            failures);

        expander.ExpandIconPosition = ExpanderIconPosition.End;
        RefreshLayout(realized.Window);
        Expect(Grid.GetColumn(expandButton) == 3,
            $"Expand icon should move to column 3 when ExpandIconPosition=End, actual {Grid.GetColumn(expandButton)}.",
            failures);
        Expect(Grid.GetColumn(addOnPresenter) == 2,
            $"Addon presenter should remain in column 2 after icon moves to End, actual {Grid.GetColumn(addOnPresenter)}.",
            failures);

        expander.ExpandIconPosition = ExpanderIconPosition.Start;
        RefreshLayout(realized.Window);
        Expect(Grid.GetColumn(expandButton) == 0,
            $"Expand icon should move back to column 0 when ExpandIconPosition=Start, actual {Grid.GetColumn(expandButton)}.",
            failures);
    }

    private static ContentPresenter? GetMaterializedExpanderContentPresenter(AtomExpander expander)
    {
        return FindVisualByName<ContentPresenter>(expander, "PART_ContentPresenter");
    }

    private static AtomExpander CreateVerificationExpander()
    {
        return new AtomExpander
        {
            Header  = "Header",
            Content = "Content"
        };
    }

    private static int CountExpanderVisualsByName(Control root, string name)
    {
        return root.GetSelfAndVisualDescendants()
                   .OfType<Control>()
                   .Count(control => control.Name == name);
    }
}
