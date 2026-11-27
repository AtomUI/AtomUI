using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// The PrimeNG view of <see cref="RangeTimePickerToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class RangeTimePickerTokenRule : IControlTokenRule<PrimeNGToken, RangeTimePickerToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<RangeTimePickerToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // PrimeNG: the control-global overrides of PrimeNGInputStage.Apply, written for the hosted input frame;
        // the formulas below read none of them.
        PrimeNGRangeTimePickerTokens.EvaluateGlobal(in context, global);
        TimePickerTokenCalculator.Calculate(in context, global, token);
        // Ant Design formula kept as is: the preset's stage for this token sets only PopupArrowVisible, so the item
        // sizes and paddings are the Ant formula over the PrimeNG global tokens.
        token.ItemWidth = global.ControlHeightLG * 1.4;
        token.ItemHeight = global.ControlHeight - 4;
        token.PeriodHostWidth = token.ItemWidth;
        token.ItemPadding = TimeViewTokenCalculator.CalculateTimeItemPadding(token.ItemWidth, token.ItemHeight);
        var range = RangePickerInputTokenCalculator.Calculate(global);
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
