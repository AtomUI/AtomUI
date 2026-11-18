using AtomUI.Desktop.Controls;
using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;

namespace AtomUI.Themes.PrimeNG.Algorithms.Controls;

/// <summary>
/// FloatButton under the PrimeNG presets: <c>@primeuix/themes</c> 3.1.1 gives buttons no resting shadow (every severity
/// of <c>button</c> has none; only the raised variant adds <c>raisedShadow</c>) and <c>speeddial</c>, the floating action
/// button, has no shadow tokens of its own — so the floating button family (FloatButton, BackTopFloatButton and the
/// group containers, which all read this token) rests without a shadow. A neutral floating button takes the official
/// <c>button.secondary</c> surface instead (background <c>surface.100</c> / <c>surface.800</c>, hover <c>200</c> /
/// <c>700</c>, active <c>300</c> / <c>600</c>, colour <c>600</c> / <c>300</c>; the border equals the fill): PrimeNG has
/// no white shadowless button, so the Ant Design white default would be invisible.
/// </summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGFloatButtonTokens : IControlTokenAlgorithm<FloatButtonToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, FloatButtonToken token)
    {
        var preset  = PrimeNGPresets.Resolve(context);
        var surface = preset.For(context.Appearance).Surface.AsSpan();
        var dark    = context.Appearance == ThemeAppearance.Dark;
        token.FloatButtonShadows = default;
        // button.secondary: light-dark(background 100/800, hover 200/700, active 300/600, colour 600/300).
        token.DefaultBg        = new PrimeNGShade(dark ? 800 : 100).Resolve(surface);
        token.DefaultHoverBg   = new PrimeNGShade(dark ? 700 : 200).Resolve(surface);
        token.DefaultActiveBg  = new PrimeNGShade(dark ? 600 : 300).Resolve(surface);
        token.DefaultIconColor = new PrimeNGShade(dark ? 300 : 600).Resolve(surface);
    }
}
