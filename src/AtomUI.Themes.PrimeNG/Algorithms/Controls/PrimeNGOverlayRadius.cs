using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Themes.PrimeNG.Algorithms.Controls;

/// <summary>
/// The PrimeUIX <c>border.radius.xl</c> of cards and dialogs, relative to the <c>BorderRadius</c> seed.
/// </summary>
internal static class PrimeNGOverlayRadius
{
    internal static CornerRadius Of(DesignToken global, PrimeNGSizeLadder sizes) =>
        new(Math.Max(0, global.BorderRadius.TopLeft + sizes.BorderRadiusXLOffset));
}
