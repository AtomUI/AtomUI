using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using AtomUI.Theme.DesignTokens;

namespace AtomUI.Controls.Themes.PrimeNG;

/// <summary>
/// Avatar under the PrimeNG presets, PrimeUIX 3.1.1 <c>avatar</c>: <c>background</c> surface.200 / 700 with
/// text.color, one <c>border.radius</c> at every size, <c>lg.font.size</c> 20 and the group border in the content
/// background.
/// </summary>
internal static class PrimeNGAvatarTokens
{
    internal static void EvaluateGlobal(in DerivationContext<PrimeNGToken> context, DesignToken controlGlobal)
    {
        controlGlobal.BorderRadiusSM = controlGlobal.BorderRadius;
        controlGlobal.BorderRadiusLG = controlGlobal.BorderRadius;
    }
    internal static void EvaluateOwn(in DerivationContext<PrimeNGToken> context, DesignToken effectiveGlobal, AvatarToken token)
    {
        token.AvatarBg         = context.Variant.IsDark ? context.Language.Surface700 : context.Language.Surface200;
        token.AvatarColor      = effectiveGlobal.ColorText;
        token.TextFontSizeLG   = 20;
        token.GroupBorderColor = effectiveGlobal.ColorBgElevated;
    }
}
