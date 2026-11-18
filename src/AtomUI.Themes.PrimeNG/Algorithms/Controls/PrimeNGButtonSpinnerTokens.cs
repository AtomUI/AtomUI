using AtomUI.Desktop.Controls;
using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;
using Avalonia.Media;

namespace AtomUI.Themes.PrimeNG.Algorithms.Controls;

/// <summary>
/// The increment/decrement handle of ButtonSpinner and NumericUpDown under the PrimeNG presets: PrimeUIX 3.1.1
/// <c>inputnumber.button</c> — a transparent resting fill, <c>surface.100</c> / <c>800</c> on hover,
/// <c>surface.200</c> / <c>700</c> while pressed, icon <c>surface.400</c> / <c>500</c> / <c>600</c> (dark
/// <c>300</c> / <c>200</c>), no divider (the stacked buttons carry <c>border: 0 none</c>) and a <c>2.25rem</c> wide
/// button at the 16px root. Both hosts share the handle theme, so both token identities are written.
/// </summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGButtonSpinnerTokens : IControlTokenAlgorithm<ButtonSpinnerToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, ButtonSpinnerToken token)
    {
        PrimeNGSpinnerHandle.Apply(context, token);
    }
}

/// <summary>
/// NumericUpDown's spinner owns <see cref="NumericUpDownToken"/> (a sibling of <see cref="ButtonSpinnerToken"/>), so the
/// same PrimeUIX <c>inputnumber.button</c> values are written through its own identity.
/// </summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGNumericUpDownTokens : IControlTokenAlgorithm<NumericUpDownToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, NumericUpDownToken token)
    {
        PrimeNGSpinnerHandle.Apply(context, token);
    }
}

/// <summary>The shared <c>inputnumber.button</c> mapping, written for both spinner token identities.</summary>
internal static class PrimeNGSpinnerHandle
{
    internal static void Apply(in ThemeAlgorithmContext context, AbstractButtonSpinnerToken token)
    {
        var preset  = PrimeNGPresets.Resolve(context);
        var surface = preset.For(context.Appearance).Surface.AsSpan();
        var dark    = context.Appearance == ThemeAppearance.Dark;
        token.HandleBg          = Colors.Transparent;
        token.FilledHandleBg    = Colors.Transparent;
        token.HandleHoverBg     = new PrimeNGShade(dark ? 800 : 100).Resolve(surface);
        token.HandleActiveBg    = new PrimeNGShade(dark ? 700 : 200).Resolve(surface);
        token.HandleIconColor   = new PrimeNGShade(400).Resolve(surface);
        token.HandleHoverColor  = new PrimeNGShade(dark ? 300 : 500).Resolve(surface);
        token.HandlePressedColor = new PrimeNGShade(dark ? 200 : 600).Resolve(surface);
        // The stacked buttons have no border of their own in PrimeUIX (border: 0 none): the input frame is the only
        // outline, so the two dividers AtomUI draws turn invisible.
        token.HandleBorderColor = Colors.Transparent;
        // inputnumber.button.width: 2.25rem at the 16px root the official preset is rendered at.
        token.HandleWidth = 36;
        // .p-inputnumber-button .p-icon { font-size/width/height: form.field.font.size } = 0.875rem (14); the product
        // asks for two quarter steps smaller than that (14 -> 10.5 -> 7.875), which also stays above the Ant Design
        // handleFontSize (fontSize / 2).
        token.HandleIconSize = 7.875;
    }
}
