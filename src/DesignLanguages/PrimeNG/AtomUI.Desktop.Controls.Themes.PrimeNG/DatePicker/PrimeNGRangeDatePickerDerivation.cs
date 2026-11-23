using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="RangeDatePickerToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGRangeDatePickerDerivation : IControlDerivation<PrimeNGToken, RangeDatePickerToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<RangeDatePickerToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGRangeDatePickerTokens.EvaluateGlobal(in context, global);
        PrimeNGAbstractDatePickerDerivation.DeriveShared(in context, global, token);
        token.ItemWidth = global.ControlHeightLG * 1.4;
        token.ItemHeight = global.ControlHeight - 4;
        token.PeriodHostWidth = token.ItemWidth;
        token.ItemPadding = PrimeNGAbstractTimeViewDerivation.CalculateTimeItemPadding(token.ItemWidth, token.ItemHeight);
        var range = RangePickerInputTokenValues.Calculate(global);
        token.RangePickerArrowMargin = range.ArrowMargin;
        token.RangePickerIndicatorThickness = range.IndicatorThickness;
        token.RangePanelSpacing = 20;
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGRangeDatePickerTokens.EvaluateOwn(in context, global, token);
    }
}
