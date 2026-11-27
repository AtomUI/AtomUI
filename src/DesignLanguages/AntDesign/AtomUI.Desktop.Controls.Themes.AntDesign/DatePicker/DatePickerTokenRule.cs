using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>
/// The AntDesign view of <see cref="DatePickerToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class DatePickerTokenRule : IControlTokenRule<AntDesignToken, DatePickerToken>
{
    public void Apply(in TokenEvaluationContext<AntDesignToken> context, ControlTokenTarget<DatePickerToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        DatePickerTokenCalculator.Calculate(in context, global, token);
        token.HeaderBorderThickness = new Thickness(0, 0, 0, global.LineWidth);
        token.ItemWidth = global.ControlHeightLG * 1.4;
        token.ItemHeight = global.ControlHeight - 4;
        token.PeriodHostWidth = token.ItemWidth;
        token.ItemPadding = TimeViewTokenCalculator.CalculateTimeItemPadding(token.ItemWidth, token.ItemHeight);
    }
}
