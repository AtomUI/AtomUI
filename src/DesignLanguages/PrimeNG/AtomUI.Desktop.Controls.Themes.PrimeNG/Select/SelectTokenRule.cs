using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// The PrimeNG view of <see cref="SelectToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class SelectTokenRule : IControlTokenRule<PrimeNGToken, SelectToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<SelectToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        var item   = context.Language.GetListItemRecipe(global);
        // PrimeNG: the control-global overrides of PrimeNGInputStage.Apply; the formulas below and the Select theme
        // read ColorTextDisabled; none of the others is read by the formulas below.
        PrimeNGSelectTokens.EvaluateGlobal(in context, global);
        SelectInputTokenCalculator.Calculate(in context, global, token);
        // PrimeUIX form.field has a full border and a regular icon color (form.field.icon.color), so the three frame
        // switches stay off.
        token.TransparentOutlineBorder = false;
        token.BottomOutlineBorder      = false;
        token.MutedSelectIndicator     = false;
        // PrimeUIX list.option: highlight.background / highlight.color for the selected option, focus.background for
        // the hovered one.
        token.OptionSelectedColor      = item.SelectedForeground;
        token.OptionSelectedIndicatorColor = global.ColorPrimary;
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
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGSelectTokens.EvaluateOwn(in context, global, token);
        if (context.Variant.Style == PrimeNGLanguage.Lara && context.Variant.Density == DesignDensity.Compact)
        {
            // Lara's list.padding (0.5rem 0) has no compact counterpart on the size ladder; the compact Lara density
            // keeps the 4 of the previous compact profiles.
            token.PopupContentPadding = new Thickness(4);
        }
    }
}
