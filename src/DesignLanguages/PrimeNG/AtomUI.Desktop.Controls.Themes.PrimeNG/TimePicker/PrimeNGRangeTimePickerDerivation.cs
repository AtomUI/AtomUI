using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="RangeTimePickerToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGRangeTimePickerDerivation : IControlDerivation<PrimeNGToken, RangeTimePickerToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<RangeTimePickerToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGRangeTimePickerTokens.EvaluateGlobal(in context, global);
        PrimeNGAbstractTimePickerDerivation.DeriveShared(in context, global, token);
        token.ItemWidth = global.ControlHeightLG * 1.4;
        token.ItemHeight = global.ControlHeight - 4;
        token.PeriodHostWidth = token.ItemWidth;
        token.ItemPadding = PrimeNGAbstractTimeViewDerivation.CalculateTimeItemPadding(token.ItemWidth, token.ItemHeight);
        var range = RangePickerInputTokenValues.Calculate(global);
        token.RangePickerArrowMargin = range.ArrowMargin;
        token.RangePickerIndicatorThickness = range.IndicatorThickness;
        token.RangeMarginToAnchor = global.UniformlyMarginXS;
        // Ant Design aligns the items to the start and fills the whole row.
        token.CenterItemContent   = false;
        token.ItemBackgroundInset = new Thickness(0);
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGRangeTimePickerTokens.EvaluateOwn(in context, global, token);
    }
}
