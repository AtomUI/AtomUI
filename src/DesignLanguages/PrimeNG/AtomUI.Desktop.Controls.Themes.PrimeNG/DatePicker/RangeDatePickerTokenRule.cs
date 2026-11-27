using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// The PrimeNG view of <see cref="RangeDatePickerToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class RangeDatePickerTokenRule : IControlTokenRule<PrimeNGToken, RangeDatePickerToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<RangeDatePickerToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // PrimeNG: the control-global overrides of PrimeNGInputStage.Apply (form-field values); the formulas below
        // read none, the RangeDatePicker theme and its input frame (RangeArrowBrush, CornerRadius, Background).
        PrimeNGRangeDatePickerTokens.EvaluateGlobal(in context, global);
        DatePickerTokenCalculator.Calculate(in context, global, token);
        // Ant Design formula kept as is; the PrimeNG range date picker stage
        // (PrimeNGRangeDatePickerTokens.EvaluateOwn) replaces RangePanelSpacing below with PrimeUIX
        // group.gap. ItemWidth and ItemHeight stay.
        token.ItemWidth = global.ControlHeightLG * 1.4;
        token.ItemHeight = global.ControlHeight - 4;
        token.PeriodHostWidth = token.ItemWidth;
        token.ItemPadding = TimeViewTokenCalculator.CalculateTimeItemPadding(token.ItemWidth, token.ItemHeight);
        var range = RangePickerInputTokenCalculator.Calculate(global);
        token.RangePickerArrowMargin = range.ArrowMargin;
        token.RangePickerIndicatorThickness = range.IndicatorThickness;
        token.RangePanelSpacing = 20;
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGRangeDatePickerTokens.EvaluateOwn(in context, global, token);
    }
}
