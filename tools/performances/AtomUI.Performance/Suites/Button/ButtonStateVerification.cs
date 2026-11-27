using System.Reflection;
using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using AtomUI.Icons.AntDesign;
using Avalonia.Controls;
using Avalonia.VisualTree;

namespace AtomUI.Performance;

internal static partial class Program
{
    private static bool RunButtonStateVerification()
    {
        var failures = new List<string>();
        VerifyButtonPseudoClassUpdates(failures);
        VerifyButtonTemplateFrameTypes(failures);
        VerifyButtonRuntimeSlots(failures);
        VerifyButtonWaveSpiritSlots(failures);
        VerifyWaveSpiritPropertyRegistration(failures);

        if (failures.Count == 0)
        {
            Console.WriteLine("Button state verification passed.");
            return true;
        }

        Console.Error.WriteLine("Button state verification failed:");
        foreach (var failure in failures)
        {
            Console.Error.WriteLine($"- {failure}");
        }
        return false;
    }

    private static void VerifyButtonPseudoClassUpdates(ICollection<string> failures)
    {
        var iconButton = new AtomUI.Desktop.Controls.Button();
        using var iconRealized = RealizeControl(iconButton);
        iconButton.SetCurrentValue(AtomUI.Desktop.Controls.Button.IconProperty, new SearchOutlined());
        RefreshLayout(iconRealized.Window);
        Expect(HasPseudoClass(iconButton, ButtonPseudoClass.IconOnly),
            "Button should set :icononly when Icon is assigned at runtime and Content is null.",
            failures);

        iconButton.SetCurrentValue(Avalonia.Controls.ContentControl.ContentProperty, "Search");
        RefreshLayout(iconRealized.Window);
        Expect(!HasPseudoClass(iconButton, ButtonPseudoClass.IconOnly),
            "Button should clear :icononly when Content is assigned at runtime.",
            failures);

        var typedButton = new AtomUI.Desktop.Controls.Button
        {
            Content = "Type"
        };
        using var typeRealized = RealizeControl(typedButton);
        Expect(HasPseudoClass(typedButton, ButtonPseudoClass.DefaultType),
            "Default Button should start with :default.",
            failures);

        typedButton.SetCurrentValue(AtomUI.Desktop.Controls.Button.ButtonTypeProperty, ButtonType.Primary);
        RefreshLayout(typeRealized.Window);
        Expect(HasPseudoClass(typedButton, ButtonPseudoClass.PrimaryType),
            "Button should set :primary when ButtonType changes to Primary at runtime.",
            failures);
        Expect(!HasPseudoClass(typedButton, ButtonPseudoClass.DefaultType),
            "Button should clear :default when ButtonType changes away from Default at runtime.",
            failures);

        typedButton.SetCurrentValue(AtomUI.Desktop.Controls.Button.IsDangerProperty, true);
        RefreshLayout(typeRealized.Window);
        Expect(HasPseudoClass(typedButton, ButtonPseudoClass.IsDanger),
            "Button should set :danger when IsDanger changes to true at runtime.",
            failures);

        typedButton.SetCurrentValue(AtomUI.Desktop.Controls.Button.IsDangerProperty, false);
        RefreshLayout(typeRealized.Window);
        Expect(!HasPseudoClass(typedButton, ButtonPseudoClass.IsDanger),
            "Button should clear :danger when IsDanger changes to false at runtime.",
            failures);

        typedButton.SetCurrentValue(AtomUI.Desktop.Controls.Button.IsLoadingProperty, true);
        RefreshLayout(typeRealized.Window);
        Expect(HasPseudoClass(typedButton, ButtonPseudoClass.Loading),
            "Button should set :loading when IsLoading changes to true.",
            failures);

        typedButton.SetCurrentValue(AtomUI.Desktop.Controls.Button.IsLoadingProperty, false);
        RefreshLayout(typeRealized.Window);
        Expect(!HasPseudoClass(typedButton, ButtonPseudoClass.Loading),
            "Button should clear :loading when IsLoading changes to false.",
            failures);
    }

    private static void VerifyButtonTemplateFrameTypes(ICollection<string> failures)
    {
        foreach (var variant in new[] { ButtonType.Default, ButtonType.Text, ButtonType.Dashed })
        {
            var button = new AtomUI.Desktop.Controls.Button { ButtonType = variant, Content = variant.ToString() };
            using var realized = RealizeControl(button);
            var frame = FindVisualByName<AtomUI.Controls.Primitives.DashedBorder>(button, "Frame");
            var hasDash = frame?.StrokeDashArray is { Count: > 0 };
            Expect(frame is not null && hasDash == (variant == ButtonType.Dashed),
                $"{variant} Button should use the frame with the matching stroke pattern.", failures);
        }
    }

    private static void VerifyButtonRuntimeSlots(ICollection<string> failures)
    {
        var button = new AtomUI.Desktop.Controls.Button { Content = "Button" };
        using var realized = RealizeControl(button);
        var host = FindVisualByName<Avalonia.Controls.Presenters.ContentPresenter>(button, "PART_LoadingIcon");
        Expect(host is { Child: null }, "Idle Button should expose the semantic host without creating a loading glyph.", failures);

        button.Icon = new SearchOutlined();
        RefreshLayout(realized.Window);
        var icon = FindVisualByName<IconPresenter>(button, "PART_ButtonIcon");
        Expect(icon is { IsVisible: true, Width: > 0 }, "Assigned Button icon should be visible at the template size.", failures);

        button.IsLoading = true;
        RefreshLayout(realized.Window);
        var loading = host?.Child as LoadingOutlined;
        Expect(loading is not null, "Loading Button should create the selected template's glyph.", failures);
        Expect(icon?.IsVisible == false, "Loading Button should hide the ordinary icon.", failures);

        button.IsLoading = false;
        RefreshLayout(realized.Window);
        Expect(host?.Child is null && loading?.GetVisualParent() is null && icon?.IsVisible == true,
            "Ending Button loading should detach the loading glyph and restore the ordinary icon.", failures);

        button.IsLoading = true;
        RefreshLayout(realized.Window);
        var second = host?.Child;
        Expect(second is LoadingOutlined && !ReferenceEquals(loading, second),
            "Re-entering loading should create a fresh glyph.", failures);

        button.ButtonType = ButtonType.Dashed;
        RefreshLayout(realized.Window);
        var replaced = FindVisualByName<Avalonia.Controls.Presenters.ContentPresenter>(button, "PART_LoadingIcon");
        Expect(replaced?.Child is LoadingOutlined && !ReferenceEquals(second, replaced.Child) && second?.GetVisualParent() is null,
            "Changing the Button template should create the current glyph and detach the old one.", failures);
    }

    private static void VerifyButtonWaveSpiritSlots(ICollection<string> failures)
    {
        var button = new AtomUI.Desktop.Controls.Button { ButtonType = ButtonType.Primary, Content = "Feedback" };
        using var realized = RealizeFeedbackControl(button);
        Expect(FindInteractionAdornment(button) is null, "Idle Button should have no feedback visual.", failures);
        ClickControl(realized.Window, button, failures, "Button feedback");
        var active = FindInteractionAdornment(button);
        Expect(active is not null, "Button activation should attach a decoration.", failures);
        button.IsInteractionEffectEnabled = false;
        RefreshLayout(realized.Window);
        Expect(FindInteractionAdornment(button) is null && active?.GetVisualParent() is null,
            "Disabling Button feedback should release the active decoration.", failures);

        foreach (var variant in new[] { ButtonType.Text, ButtonType.Link })
        {
            button.ButtonType = variant;
            button.IsInteractionEffectEnabled = true;
            RefreshLayout(realized.Window);
            ClickControl(realized.Window, button, failures, variant + " Button");
            Expect(FindInteractionAdornment(button) is null, variant + " Button should use its silent default.", failures);
        }
    }

    private static void VerifyWaveSpiritPropertyRegistration(ICollection<string> failures)
    {
        Expect(AtomUI.Desktop.Controls.Button.IsInteractionEffectEnabledProperty.Name == "IsInteractionEffectEnabled",
            "Button should register the neutral feedback property.", failures);
    }

    private static RealizedScenario RealizeFeedbackControl(Control control)
    {
        var window = new AtomUI.Desktop.Controls.Window { Content = control, Width = 480, Height = 290 };
        window.Show();
        RefreshLayout(window);
        return new RealizedScenario(window, [control]);
    }

    private static Control? FindInteractionAdornment(Control target) =>
        Avalonia.Controls.Primitives.AdornerLayer.GetAdornerLayer(target)?.Children.FirstOrDefault(child =>
            child.GetType().Name == "InteractionEffectAdorner" &&
            Avalonia.Controls.Primitives.AdornerLayer.GetAdornedElement(child) is { } surface &&
            (ReferenceEquals(surface, target) || target.IsVisualAncestorOf(surface)));

    private static bool HasPseudoClass(Control control, string pseudoClass)
    {
        return control.Classes.Contains(pseudoClass);
    }

    private static Control? FindVisualByTypeName(Control root, string typeName, string? name = null)
    {
        return root.GetSelfAndVisualDescendants()
                   .OfType<Control>()
                   .FirstOrDefault(control => control.GetType().Name == typeName &&
                                              (name == null || control.Name == name));
    }
}
