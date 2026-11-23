using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="RangeTimePickerToken"/>.</summary>
[ControlDerivation]
internal sealed class AntDesignRangeTimePickerDerivation : IControlDerivation<AntDesignToken, RangeTimePickerToken>
{
    public void Derive(in DerivationContext<AntDesignToken> context, ControlView<RangeTimePickerToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        AntDesignAbstractTimePickerDerivation.DeriveShared(in context, global, token);
        token.ItemWidth = global.ControlHeightLG * 1.4;
        token.ItemHeight = global.ControlHeight - 4;
        token.PeriodHostWidth = token.ItemWidth;
        token.ItemPadding = AntDesignAbstractTimeViewDerivation.CalculateTimeItemPadding(token.ItemWidth, token.ItemHeight);
        var range = RangePickerInputTokenValues.Calculate(global);
        token.RangePickerArrowMargin = range.ArrowMargin;
        token.RangePickerIndicatorThickness = range.IndicatorThickness;
        token.RangeMarginToAnchor = global.UniformlyMarginXS;
        // Ant Design aligns the items to the start and fills the whole row.
        token.CenterItemContent   = false;
        token.ItemBackgroundInset = new Thickness(0);
    }
}
