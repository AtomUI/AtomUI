using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>
/// The AntDesign view of <see cref="RangeDatePickerToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class RangeDatePickerTokenRule : IControlTokenRule<AntDesignToken, RangeDatePickerToken>
{
    public void Apply(in TokenEvaluationContext<AntDesignToken> context, ControlTokenTarget<RangeDatePickerToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        DatePickerTokenCalculator.Calculate(in context, global, token);
        token.ItemWidth = global.ControlHeightLG * 1.4;
        token.ItemHeight = global.ControlHeight - 4;
        token.PeriodHostWidth = token.ItemWidth;
        token.ItemPadding = TimeViewTokenCalculator.CalculateTimeItemPadding(token.ItemWidth, token.ItemHeight);
        var range = RangePickerInputTokenCalculator.Calculate(global);
        token.RangePickerArrowMargin = range.ArrowMargin;
        token.RangePickerIndicatorThickness = range.IndicatorThickness;
        token.RangePanelSpacing = 20;
    }
}
