using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>
/// The AntDesign view of <see cref="WindowTitleBarButtonToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class WindowTitleBarButtonTokenRule : IControlTokenRule<AntDesignToken, WindowTitleBarButtonToken>
{
    public void Apply(in TokenEvaluationContext<AntDesignToken> context, ControlTokenTarget<WindowTitleBarButtonToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        TitleBarActionTokenCalculator.Calculate(in context, global, token);
        token.CaptionButtonIconSize = global.IconSize;
        token.CaptionButtonPadding = new Thickness(global.SizeUnit * 2);
        token.CaptionButtonCornerRadius = TitleBarActionTokenCalculator.CalculateCaptionCornerRadius(token.CaptionButtonIconSize, token.CaptionButtonPadding);
    }
}
