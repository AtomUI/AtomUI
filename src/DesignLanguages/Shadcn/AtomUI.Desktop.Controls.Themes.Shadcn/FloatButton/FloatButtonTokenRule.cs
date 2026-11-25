using Avalonia;
using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="FloatButtonToken"/>.</summary>
[ControlTokenRule]
internal sealed class FloatButtonTokenRule : IControlTokenRule<ShadcnToken, FloatButtonToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<FloatButtonToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // The outline button's border and the style's large radius.
        global.ColorBorder    = ButtonTokenCalculator.ResolveOutline(in context, global).Border;
        global.BorderRadiusMD = new CornerRadius(Math.Min(context.Language.ButtonRadiusLG, global.ControlHeightLG / 3));
        global.BorderRadiusLG = new CornerRadius(context.Language.ButtonRadiusLG);
        global.PaddingLG      = new Thickness(Math.Min(context.Language.PaddingXSM, 10), 2);
        global.SpacingXS      = context.Variant.Style == ShadcnStyles.Mira || context.Variant.Style == ShadcnStyles.Sera ? 4 : 6;
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
        var floatOutline = ButtonTokenCalculator.ResolveOutline(in context, global);
        var elevated = global.ColorBgElevated;
        // Floating actions cover scrolling content: composite the outline paints over an elevated surface.
        token.DefaultBg = floatOutline.Background is { } floatBackground
            ? ShadcnRecipes.CompositeOver(floatBackground, floatBackground.A / 255d, elevated)
            : elevated;
        token.DefaultHoverBg = ShadcnRecipes.CompositeOver(
            floatOutline.HoverBackground, floatOutline.HoverBackground.A / 255d, token.DefaultBg);
        token.DefaultActiveBg  = ShadcnRecipes.CompositeOver(global.ColorText, 0.05, token.DefaultHoverBg);
        token.DefaultIconColor = global.ColorText;

        var floatStyle = context.Variant.Style;
        var vega = floatStyle == ShadcnStyles.Vega;
        token.HasOutline                   = true;
        token.OutlineOnlyShadow            = vega;
        token.CapsuleDescription           = floatStyle == ShadcnStyles.Luma || floatStyle == ShadcnStyles.Maia;
        token.UppercaseDescription         = floatStyle == ShadcnStyles.Sera;
        token.DescriptionWeight            = context.Language.ButtonWeight;
        token.OutlineDescriptionLineHeight = Math.Ceiling(global.FontSize * 1.15);
        token.FloatButtonShadows = vega ? ShadcnRecipes.ShadowXS : default;
    }
}
