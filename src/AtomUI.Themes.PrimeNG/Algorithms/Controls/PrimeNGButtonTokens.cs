using AtomUI.Desktop.Controls;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Themes.PrimeNG.Algorithms.Controls;

/// <summary>
/// Button under the PrimeNG presets: the PrimeUIX <c>button.label.font.weight</c> (Aura 500, Lara 600, Nora 700)
/// and the horizontal label padding of each size: PrimeUIX <c>button.padding.x</c> (the field padding: 10 / sm 8 /
/// lg 12; Lara's normal size 0.875rem) plus the border, so the label sits where primeng.dev puts it. The radius is
/// the global <c>BorderRadius</c> already (PrimeUIX <c>button.border.radius</c> is <c>border.radius.md</c>; Nora <c>border.radius.xs</c>, which is its seed). The Filled
/// variant draws the primary on the global <c>ColorPrimaryBg</c>, which every preset keeps pale (the solid highlight is
/// <c>ControlItemBgActive</c> only), so it needs no compensation. The Link variant takes PrimeUIX
/// <c>button.link.color</c>, <c>{primary.color}</c>: for the Button only, <c>ColorLink</c> / <c>Hover</c> /
/// <c>Active</c> are the primary and its hover and active states (PrimeUIX keeps the colour and underlines the label on
/// hover, which AtomUI does not draw, so the primary states keep the feedback). The global link colours stay the
/// deeper, page-readable stops for hyperlinks and other link text.
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
        controlGlobal.ColorLink       = controlGlobal.ColorPrimary;
        controlGlobal.ColorLinkHover  = controlGlobal.ColorPrimaryHover;
        controlGlobal.ColorLinkActive = controlGlobal.ColorPrimaryActive;
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
