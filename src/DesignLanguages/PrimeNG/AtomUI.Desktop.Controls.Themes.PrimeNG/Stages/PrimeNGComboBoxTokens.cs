using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// ComboBox under the PrimeNG presets: PrimeUIX 3.1.1 <c>list.option</c> — a row of the 0.875rem line (1.5 line height
/// = 21) plus 0.25rem padding top and bottom, so 29 instead of the Ant Design <c>ControlHeight</c>, and 0.625rem
/// horizontal padding. The row height also drives the popup's page size.
/// </summary>
internal static class PrimeNGComboBoxTokens
{
    internal static void EvaluateGlobal(in TokenEvaluationContext<PrimeNGToken> context, DesignToken controlGlobal)
    {
    }
    internal static void EvaluateOwn(in TokenEvaluationContext<PrimeNGToken> context, DesignToken effectiveGlobal, ComboBoxToken token)
    {
        // list.option: padding 4px 10px (Lara 8px 14px) over the 0.875rem line at the 16px root the preset renders
        // at, so the row is 14 * 1.5 + 2 * 4 = 29 (Lara 37); PrimeUIX does not scale the list row with the control
        // size. Option text is list.option.color (text.color), hover list.option.focus.background; list.padding.
        var recipes = context.Language.GetVisualRecipes(effectiveGlobal, context.Variant);
        var padding  = recipes.ListOptionPadding;
        token.ItemHeight          = effectiveGlobal.FontSize * 1.5 + padding.Top + padding.Bottom;
        token.ItemPadding         = new Thickness(padding.Left, 0);
        token.ItemColor           = effectiveGlobal.ColorText;
        token.ItemHoverColor      = effectiveGlobal.ColorText;
        token.ItemHoverBgColor    = effectiveGlobal.ControlItemBgHover;
        token.PopupContentPadding = context.Language.GetSelectOverlayRecipe(effectiveGlobal, context.Variant).Padding;
    }
}
