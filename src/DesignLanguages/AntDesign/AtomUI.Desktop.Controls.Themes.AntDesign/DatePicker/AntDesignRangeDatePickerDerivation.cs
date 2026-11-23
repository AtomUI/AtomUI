using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="RangeDatePickerToken"/>.</summary>
[ControlDerivation]
internal sealed class AntDesignRangeDatePickerDerivation : IControlDerivation<AntDesignToken, RangeDatePickerToken>
{
    public void Derive(in DerivationContext<AntDesignToken> context, ControlView<RangeDatePickerToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        AntDesignAbstractDatePickerDerivation.DeriveShared(in context, global, token);
        token.ItemWidth = global.ControlHeightLG * 1.4;
        token.ItemHeight = global.ControlHeight - 4;
        token.PeriodHostWidth = token.ItemWidth;
        token.ItemPadding = AntDesignAbstractTimeViewDerivation.CalculateTimeItemPadding(token.ItemWidth, token.ItemHeight);
        var range = RangePickerInputTokenValues.Calculate(global);
        token.RangePickerArrowMargin = range.ArrowMargin;
        token.RangePickerIndicatorThickness = range.IndicatorThickness;
        token.RangePanelSpacing = 20;
    }
}
