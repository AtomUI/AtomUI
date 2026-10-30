using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using Avalonia.Input;
using Avalonia.Media;

namespace AtomUI.Performance;

internal static partial class Program
{
    private static bool RunEffectiveBrushVerification()
    {
        var failures = new List<string>();
        VerifyAddOnDecoratedBoxEffectiveBrushes(failures);
        VerifyFocusedFilledBackground(failures);
        VerifyDropdownEffectiveBrushes(new SelectAddOnDecoratedBox(), box => box.IsDropDownOpen = true, "SelectAddOnDecoratedBox", failures);
        VerifyDropdownEffectiveBrushes(new TreeSelectAddOnDecoratedBox(), box => box.IsDropDownOpen = true, "TreeSelectAddOnDecoratedBox", failures);
        VerifyDropdownEffectiveBrushes(new CascaderAddOnDecoratedBox(), box => box.IsDropDownOpen = true, "CascaderAddOnDecoratedBox", failures);

        if (failures.Count == 0)
        {
            Console.WriteLine("Effective brush verification passed.");
            return true;
        }

        Console.Error.WriteLine("Effective brush verification failed:");
        foreach (var failure in failures)
        {
            Console.Error.WriteLine($"- {failure}");
        }
        return false;
    }

    private static void VerifyAddOnDecoratedBoxEffectiveBrushes(ICollection<string> failures)
    {
        var box     = new AddOnDecoratedBox();
        var brushes = ApplyTestBrushes(box);

        ResetEffectiveBrushState(box, InputControlStyleVariant.Outlined);
        ExpectBrush(box.EffectiveInnerBoxBorderBrush, brushes.DefaultBorder, "Outlined default border", failures);
        ExpectBrush(box.EffectiveInnerBoxBackground, Brushes.Transparent, "Outlined default background", failures);

        box.IsInnerBoxHover = true;
        ExpectBrush(box.EffectiveInnerBoxBorderBrush, brushes.HoverBorder, "Outlined hover border", failures);

        box.IsInnerBoxPressed = true;
        ExpectBrush(box.EffectiveInnerBoxBorderBrush, brushes.ActiveBorder, "Outlined pressed border", failures);

        box.IsInnerBoxPressed = false;
        box.SetCurrentValue(AddOnDecoratedBox.StatusProperty, InputControlStatus.Error);
        ExpectBrush(box.EffectiveInnerBoxBorderBrush, brushes.ErrorHoverBorder, "Outlined error hover border", failures);

        box.IsInnerBoxHover   = false;
        box.IsInnerBoxPressed = true;
        ExpectBrush(box.EffectiveInnerBoxBorderBrush, brushes.ErrorBorder, "Outlined error pressed border", failures);

        ResetEffectiveBrushState(box, InputControlStyleVariant.Underlined);
        box.SetCurrentValue(AddOnDecoratedBox.StatusProperty, InputControlStatus.Warning);
        box.IsInnerBoxHover = true;
        ExpectBrush(box.EffectiveInnerBoxBorderBrush, brushes.WarningHoverBorder, "Underlined warning hover border", failures);
        ExpectBrush(box.EffectiveInnerBoxBackground, Brushes.Transparent, "Underlined warning hover background", failures);

        ResetEffectiveBrushState(box, InputControlStyleVariant.Borderless);
        box.IsInnerBoxHover = true;
        ExpectBrush(box.EffectiveInnerBoxBorderBrush, Brushes.Transparent, "Borderless hover border", failures);
        ExpectBrush(box.EffectiveInnerBoxBackground, Brushes.Transparent, "Borderless hover background", failures);

        ResetEffectiveBrushState(box, InputControlStyleVariant.Filled);
        ExpectBrush(box.EffectiveInnerBoxBorderBrush, brushes.FilledBorder, "Filled default border", failures);
        ExpectBrush(box.EffectiveInnerBoxBackground, brushes.FilledBackground, "Filled default background", failures);

        box.IsInnerBoxHover = true;
        ExpectBrush(box.EffectiveInnerBoxBorderBrush, brushes.FilledBorder, "Filled hover border", failures);
        ExpectBrush(box.EffectiveInnerBoxBackground, brushes.FilledHoverBackground, "Filled hover background", failures);

        box.IsInnerBoxPressed = true;
        ExpectBrush(box.EffectiveInnerBoxBorderBrush, brushes.ActiveBorder, "Filled pressed border", failures);
        ExpectBrush(box.EffectiveInnerBoxBackground, brushes.FilledHoverBackground, "Filled pressed keeps hover background", failures);

        box.IsInnerBoxHover   = false;
        box.IsInnerBoxPressed = false;
        box.SetCurrentValue(AddOnDecoratedBox.StatusProperty, InputControlStatus.Error);
        ExpectBrush(box.EffectiveInnerBoxBorderBrush, brushes.ErrorFilledBorder, "Filled error border", failures);
        ExpectBrush(box.EffectiveInnerBoxBackground, brushes.ErrorBackground, "Filled error background", failures);

        box.IsInnerBoxHover = true;
        ExpectBrush(box.EffectiveInnerBoxBorderBrush, brushes.ErrorFilledBorder, "Filled error hover border", failures);
        ExpectBrush(box.EffectiveInnerBoxBackground, brushes.ErrorHoverBackground, "Filled error hover background", failures);

        box.IsInnerBoxHover   = false;
        box.IsInnerBoxPressed = true;
        ExpectBrush(box.EffectiveInnerBoxBorderBrush, brushes.ErrorBorder, "Filled error pressed border", failures);
        ExpectBrush(box.EffectiveInnerBoxBackground, brushes.ErrorBackground, "Filled error pressed background", failures);

        ResetEffectiveBrushState(box, InputControlStyleVariant.Filled);
        box.SetCurrentValue(InputElement.IsEnabledProperty, false);
        ExpectBrush(box.EffectiveInnerBoxBorderBrush, brushes.DefaultBorder, "Filled disabled border", failures);
        ExpectBrush(box.EffectiveInnerBoxBackground, brushes.DisabledBackground, "Filled disabled background", failures);

        ResetEffectiveBrushState(box, InputControlStyleVariant.Outlined);
        box.IsInnerBoxHover = true;
        var replacementHoverBorder = Brush(Color.FromRgb(240, 20, 210));
        box.InnerBoxHoverBorderBrush = replacementHoverBorder;
        ExpectBrush(box.EffectiveInnerBoxBorderBrush, replacementHoverBorder, "Source brush replacement refreshes effective border", failures);
    }

    private static void VerifyFocusedFilledBackground(ICollection<string> failures)
    {
        var editor = new Avalonia.Controls.TextBox { Text = "focus" };
        var box = new AddOnDecoratedBox
        {
            Width        = 260,
            Content      = editor,
            StyleVariant = InputControlStyleVariant.Filled,
            IsMotionEnabled = false
        };
        var brushes = ApplyTestBrushes(box);

        using var realized = RealizeControl(box);
        editor.Focus();
        RefreshLayout(realized.Window);

        Expect(box.IsKeyboardFocusWithin, "Focused filled verification should set IsKeyboardFocusWithin.", failures);
        ExpectBrush(box.EffectiveInnerBoxBorderBrush, brushes.ActiveBorder, "Filled focused border", failures);
        ExpectBrush(box.EffectiveInnerBoxBackground, brushes.ActiveBackground, "Filled focused background", failures);
    }

    private static void VerifyDropdownEffectiveBrushes<T>(T box,
                                                          Action<T> openDropDown,
                                                          string label,
                                                          ICollection<string> failures)
        where T : AddOnDecoratedBox
    {
        var brushes = ApplyTestBrushes(box);
        box.InnerBoxFilledBorderBrush        = Brushes.Transparent;
        box.InnerBoxErrorFilledBorderBrush   = Brushes.Transparent;
        box.InnerBoxWarningFilledBorderBrush = Brushes.Transparent;
        box.SetCurrentValue(AddOnDecoratedBox.StyleVariantProperty, InputControlStyleVariant.Filled);

        ExpectBrush(box.EffectiveInnerBoxBorderBrush, Brushes.Transparent, $"{label} filled closed border", failures);
        ExpectBrush(box.EffectiveInnerBoxBackground, brushes.FilledBackground, $"{label} filled closed background", failures);

        openDropDown(box);
        ExpectBrush(box.EffectiveInnerBoxBorderBrush, brushes.ActiveBorder, $"{label} opened border", failures);
        ExpectBrush(box.EffectiveInnerBoxBackground, brushes.FilledBackground, $"{label} opened background", failures);

        box.SetCurrentValue(AddOnDecoratedBox.StatusProperty, InputControlStatus.Warning);
        ExpectBrush(box.EffectiveInnerBoxBorderBrush, brushes.WarningBorder, $"{label} opened warning border", failures);
        ExpectBrush(box.EffectiveInnerBoxBackground, brushes.WarningBackground, $"{label} opened warning background", failures);
    }
}
