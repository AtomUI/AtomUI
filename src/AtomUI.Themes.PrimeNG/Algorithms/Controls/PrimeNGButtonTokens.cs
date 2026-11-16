using AtomUI.Desktop.Controls;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Themes.PrimeNG.Algorithms.Controls;

/// <summary>
/// Button under the PrimeNG presets: the PrimeUIX <c>button.label.font.weight</c> (Aura 500, Lara 600, Nora 700)
/// and the horizontal label padding of each size: PrimeUIX <c>button.padding.x</c> (the field padding: 10 / sm 8 /
/// lg 12; Lara's normal size 0.875rem) plus the border, so the label sits where primeng.dev puts it. The radius is
/// the global <c>BorderRadius</c> already (PrimeUIX <c>button.border.radius</c> is <c>border.radius.md</c>; Nora <c>border.radius.xs</c>, which is its seed). Where the
/// preset's selected background is the solid primary (Nora), the button's own <c>ColorPrimaryBg</c> /
/// <c>ColorPrimaryBgHover</c> come from the rule table so the Filled variant's primary text stays readable; a noir
/// primary keeps the global ones, which are a neutral tint (<see cref="PrimeNGNoir"/>).
/// </summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGButtonTokens : IControlTokenAlgorithm<ButtonToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
        var preset = PrimeNGPresets.Resolve(context);
        if (PrimeNGNoir.Resolve(preset, context.Appearance, controlGlobal).PrimaryTint is { } primaryBg)
        {
            (controlGlobal.ColorPrimaryBg, controlGlobal.ColorPrimaryBgHover) = primaryBg(controlGlobal);
        }
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, ButtonToken token)
    {
        var controls = PrimeNGPresets.Resolve(context).Controls;
        token.FontWeight = controls.ButtonFontWeight;

        // The Button's label inset is its ContentPadding (the border is inside it); a CSS button adds the border to
        // its padding. The vertical paddings keep the Button's own centring.
        var border     = effectiveGlobal.LineWidth;
        var horizontal = controls.ButtonPaddingHorizontal?.Invoke(effectiveGlobal) ?? effectiveGlobal.InputPadding.Left;
        token.ContentPadding   = WithHorizontal(token.ContentPadding, horizontal + border);
        token.ContentPaddingSM = WithHorizontal(token.ContentPaddingSM, effectiveGlobal.InputPaddingSM.Left + border);
        token.ContentPaddingLG = WithHorizontal(token.ContentPaddingLG, effectiveGlobal.InputPaddingLG.Left + border);
    }

    private static Thickness WithHorizontal(Thickness padding, double horizontal)
    {
        return new Thickness(horizontal, padding.Top, horizontal, padding.Bottom);
    }
}
