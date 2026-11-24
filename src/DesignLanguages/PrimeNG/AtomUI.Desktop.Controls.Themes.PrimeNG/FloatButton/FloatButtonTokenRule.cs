using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="FloatButtonToken"/>.</summary>
[ControlTokenRule]
internal sealed class FloatButtonTokenRule : IControlTokenRule<PrimeNGToken, FloatButtonToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<FloatButtonToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGFloatButtonTokens.EvaluateGlobal(in context, global);
        token.HasOutline                   = false;
        token.OutlineOnlyShadow            = false;
        token.CapsuleDescription           = false;
        token.UppercaseDescription         = false;
        token.DescriptionWeight            = FontWeight.Normal;
        token.OutlineDescriptionLineHeight = 0;
        token.FloatButtonShadows    = global.BoxShadowsSecondary;
        token.DefaultBg             = global.ColorBgElevated;
        token.DefaultHoverBg        = global.ColorBgTextHover;
        // Ant Design has no pressed reaction for the default (white) type: the frame stays transparent so the
        // elevated layer below shows through, exactly as before this token existed.
        token.DefaultActiveBg       = Colors.Transparent;
        token.DefaultIconColor      = global.ColorText;
        token.PrimaryColor          = global.ColorTextLightSolid;
        token.FloatButtonIconSize   = global.FontSizeIcon * 1.5;
        token.FloatButtonSize       = global.ControlHeightLG;
        token.DescriptionLineHeight = global.FontSizeSM * 1.2;
        token.FloatOffsetX          = global.UniformlyMargin;
        token.FloatOffsetY          = global.UniformlyMargin;
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGFloatButtonTokens.EvaluateOwn(in context, global, token);
    }
}
