using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="RangeTimePickerToken"/>.</summary>
[ControlDerivation]
internal sealed class ShadcnRangeTimePickerDerivation : IControlDerivation<ShadcnToken, RangeTimePickerToken>
{
    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<RangeTimePickerToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        ShadcnAbstractTimePickerDerivation.DeriveShared(in context, global, token);
        token.ItemWidth = global.ControlHeightLG * 1.4;
        token.ItemHeight = global.ControlHeight - 4;
        token.PeriodHostWidth = token.ItemWidth;
        // shadcn centres the items (no item padding) and insets the item background by a line.
        token.ItemPadding = default;
        var range = RangePickerInputTokenValues.Calculate(global);
        token.RangePickerArrowMargin = range.ArrowMargin;
        token.RangePickerIndicatorThickness = range.IndicatorThickness;
        token.RangeMarginToAnchor = global.UniformlyMarginXS;
        token.CenterItemContent   = true;
        token.ItemBackgroundInset = new Thickness(0, global.LineWidth);
    }
}
