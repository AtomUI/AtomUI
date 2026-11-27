using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// The PrimeNG view of <see cref="ExpanderToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class ExpanderTokenRule : IControlTokenRule<PrimeNGToken, ExpanderToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<ExpanderToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // PrimeNG: the control-global overrides (ColorBorder, MotionDurationSlow), read by the Expander theme
        // (BorderBrush, MotionDuration) and not by the formulas below.
        PrimeNGExpanderTokens.EvaluateGlobal(in context, global);
        token.HeaderPadding            = new Thickness(global.UniformlyPadding, global.UniformlyPaddingSM);
        token.HeaderBg                 = global.ColorFillAlter;
        token.HeaderColor              = global.ColorTextHeading;
        token.HeaderFontWeight         = FontWeight.Normal;
        token.ContentPadding           = new Thickness(16, global.UniformlyPadding);
        token.ContentPaddingSM         = global.PaddingSM;
        token.ContentPaddingLG         = global.PaddingLG;
        token.ContentBg                = global.ColorBgContainer;
        token.HeaderPaddingSM          = new Thickness(global.UniformlyPaddingSM, global.UniformlyPaddingXS);
        token.HeaderPaddingLG          = new Thickness(global.UniformlyPaddingLG, global.UniformlyPadding);
        token.ExpanderBorderRadius     = global.BorderRadiusLG;
        token.LeftExpandButtonHMargin  = new Thickness(0, 0, global.UniformlyMarginSM, 0);
        token.RightExpandButtonHMargin = new Thickness(global.UniformlyMarginSM, 0, 0, 0);
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGExpanderTokens.EvaluateOwn(in context, global, token);
    }
}
