using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="DatePickerToken"/>.</summary>
[ControlTokenRule]
internal sealed class DatePickerTokenRule : IControlTokenRule<ShadcnToken, DatePickerToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<DatePickerToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        InputTokenCalculator.CalculateGlobal(in context, global);
        DatePickerTokenCalculator.Calculate(in context, global, token);
        token.HeaderBorderThickness = new Thickness(0, 0, 0, global.LineWidth);
        token.ItemWidth = global.ControlHeightLG * 1.4;
        token.ItemHeight = global.ControlHeight - 4;
        token.PeriodHostWidth = token.ItemWidth;
        token.ItemPadding = TimeViewTokenCalculator.CalculateTimeItemPadding(token.ItemWidth, token.ItemHeight);
    }
}
