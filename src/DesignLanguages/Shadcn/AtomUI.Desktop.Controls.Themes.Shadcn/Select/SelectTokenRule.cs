using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn view of <see cref="SelectToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class SelectTokenRule : IControlTokenRule<ShadcnToken, SelectToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<SelectToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        var item   = context.Language.GetListItemRecipe(global);
        var frame = InputTokenCalculator.CalculateFieldGlobal(in context, global);
        SelectInputTokenCalculator.Calculate(in context, global, token);
        // Ant Design's defaults: an outline on every side and a full-strength selected indicator, both replaced below.
        token.TransparentOutlineBorder = false;
        token.BottomOutlineBorder      = false;
        token.MutedSelectIndicator     = false;
        token.OptionSelectedColor      = item.SelectedForeground;
        // The indicator follows the focus color of the input family (--ring) rather than the formula's ColorPrimary.
        token.OptionSelectedIndicatorColor = global.InputActiveBorderColor;
        token.OptionSelectedFontWeight = global.FontWeightStrong;
        token.OptionSelectedBg         = item.SelectedBackground;
        token.OptionActiveBg           = item.FocusBackground;
        token.OptionPadding            = new Thickness(global.ControlPaddingHorizontal, (global.ControlHeight - global.FontHeight) / 2);
        token.OptionHeight = global.ControlHeight;
        token.SelectAffixPadding = global.PaddingXXS;
        token.FixedItemMargin = new Thickness(Math.Floor(global.UniformlyPaddingXXS / 2));
        token.PopupContentPadding = new Thickness(global.UniformlyPaddingXXS / 2);
        // Ant Design's option highlight fills the row without an inset or rounding.
        token.OptionBackgroundInset = new Thickness(0);
        token.OptionBorderRadius    = new CornerRadius(0);
        // registry select: the item background is inset by a line, the frame is the style's field frame (a transparent
        // border in Luma and Rhea, a bottom rule in Sera) and Luma weakens the selected indicator.
        token.OptionBackgroundInset = new Thickness(0, global.LineWidth);
        (token.TransparentOutlineBorder, token.BottomOutlineBorder) = frame;
        token.MutedSelectIndicator = context.Variant.Style == ShadcnStyles.Luma;
    }
}
