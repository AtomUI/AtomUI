using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using AtomUI.Theme;
using AtomUI.Theme.DesignTokens;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// FloatButton under the PrimeNG presets: <c>@primeuix/themes</c> 3.1.1 gives buttons no resting shadow (every severity
/// of <c>button</c> has none; only the raised variant adds <c>raisedShadow</c>) and <c>speeddial</c>, the floating action
/// button, has no shadow tokens of its own — so the floating button family (FloatButton, BackTopFloatButton and the
/// group containers, which all read this token) rests without a shadow. A neutral floating button takes the official
/// <c>button.secondary</c> surface instead (background <c>surface.100</c> / <c>surface.800</c>, hover <c>200</c> /
/// <c>700</c>, active <c>300</c> / <c>600</c>, colour <c>600</c> / <c>300</c>; the border equals the fill): PrimeNG has
/// no white shadowless button, so the Ant Design white default would be invisible.
/// </summary>
internal static class PrimeNGFloatButtonTokens
{
    internal static void EvaluateGlobal(in TokenEvaluationContext<PrimeNGToken> context, DesignToken controlGlobal)
    {
    }
    internal static void EvaluateOwn(in TokenEvaluationContext<PrimeNGToken> context, DesignToken effectiveGlobal, FloatButtonToken token)
    {
        var language = context.Language;
        var dark    = context.Variant.Appearance == ThemeAppearance.Dark;
        token.FloatButtonShadows = default;
        // button.secondary: light-dark(background 100/800, hover 200/700, active 300/600, colour 600/300).
        token.DefaultBg        = dark ? language.Surface800 : language.Surface100;
        token.DefaultHoverBg   = dark ? language.Surface700 : language.Surface200;
        token.DefaultActiveBg  = dark ? language.Surface600 : language.Surface300;
        token.DefaultIconColor = dark ? language.Surface300 : language.Surface600;
    }
}
