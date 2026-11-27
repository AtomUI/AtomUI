using Avalonia;
using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>
/// The AntDesign view of <see cref="TimePickerToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class TimePickerTokenRule : IControlTokenRule<AntDesignToken, TimePickerToken>
{
    public void Apply(in TokenEvaluationContext<AntDesignToken> context, ControlTokenTarget<TimePickerToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        TimePickerTokenCalculator.Calculate(in context, global, token);
        token.ItemWidth = global.ControlHeightLG * 1.4;
        token.ItemHeight = global.ControlHeight - 4;
        token.PeriodHostWidth = token.ItemWidth;
        token.ItemPadding = TimeViewTokenCalculator.CalculateTimeItemPadding(token.ItemWidth, token.ItemHeight);
        // Ant Design aligns the items to the start and fills the whole row.
        token.CenterItemContent   = false;
        token.ItemBackgroundInset = new Thickness(0);
    }
}
