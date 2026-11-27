using Avalonia;
using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn view of <see cref="TimePickerToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class TimePickerTokenRule : IControlTokenRule<ShadcnToken, TimePickerToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<TimePickerToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        InputTokenCalculator.CalculateGlobal(in context, global);
        TimePickerTokenCalculator.Calculate(in context, global, token);
        token.ItemWidth = global.ControlHeightLG * 1.4;
        token.ItemHeight = global.ControlHeight - 4;
        token.PeriodHostWidth = token.ItemWidth;
        // shadcn centres the items and insets the item background by a line.
        token.ItemPadding = default;
        token.CenterItemContent   = true;
        token.ItemBackgroundInset = new Thickness(0, global.LineWidth);
    }
}
