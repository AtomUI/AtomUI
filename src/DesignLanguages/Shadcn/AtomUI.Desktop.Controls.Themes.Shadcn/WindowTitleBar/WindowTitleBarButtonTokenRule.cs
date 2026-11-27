using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn view of <see cref="WindowTitleBarButtonToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class WindowTitleBarButtonTokenRule : IControlTokenRule<ShadcnToken, WindowTitleBarButtonToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<WindowTitleBarButtonToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        TitleBarActionTokenCalculator.Calculate(in context, global, token);
        token.CaptionButtonIconSize = global.IconSize;
        token.CaptionButtonPadding = new Thickness(global.SizeUnit * 2);
        token.CaptionButtonCornerRadius = TitleBarActionTokenCalculator.CalculateCaptionCornerRadius(token.CaptionButtonIconSize, token.CaptionButtonPadding);
    }
}
