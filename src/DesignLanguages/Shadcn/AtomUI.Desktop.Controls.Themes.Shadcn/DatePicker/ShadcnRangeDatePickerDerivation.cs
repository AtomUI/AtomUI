using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="RangeDatePickerToken"/>.</summary>
[ControlDerivation]
internal sealed class ShadcnRangeDatePickerDerivation : IControlDerivation<ShadcnToken, RangeDatePickerToken>
{
    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<RangeDatePickerToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        ShadcnAbstractDatePickerDerivation.DeriveShared(in context, global, token);
        token.ItemWidth = global.ControlHeightLG * 1.4;
        token.ItemHeight = global.ControlHeight - 4;
        token.PeriodHostWidth = token.ItemWidth;
        token.ItemPadding = ShadcnAbstractTimeViewDerivation.CalculateTimeItemPadding(token.ItemWidth, token.ItemHeight);
        var range = RangePickerInputTokenValues.Calculate(global);
        token.RangePickerArrowMargin = range.ArrowMargin;
        token.RangePickerIndicatorThickness = range.IndicatorThickness;
        token.RangePanelSpacing = 20;
    }
}
