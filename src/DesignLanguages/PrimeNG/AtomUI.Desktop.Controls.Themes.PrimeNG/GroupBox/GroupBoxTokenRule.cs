using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// The PrimeNG view of <see cref="GroupBoxToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class GroupBoxTokenRule : IControlTokenRule<PrimeNGToken, GroupBoxToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<GroupBoxToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // PrimeNG: the control-global overrides (ColorBorder, ColorBgContainer), read by the GroupBox theme
        // (BorderBrush, Background) and not by the formulas below.
        PrimeNGGroupBoxTokens.EvaluateGlobal(in context, global);
        token.ContentPadding           = global.PaddingXS;
        token.HeaderContainerMargin = new Thickness(global.UniformlyMargin, global.UniformlyMarginXS,
            global.UniformlyMargin, 0);
        token.HeaderContentPadding = new Thickness(global.UniformlyPaddingXXS, 0);
        token.HeaderIconMargin     = new Thickness(0, 0, global.UniformlyMarginXXS, 0);
        token.HeaderFontWeight     = FontWeight.Normal;
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGGroupBoxTokens.EvaluateOwn(in context, global, token);
    }
}
