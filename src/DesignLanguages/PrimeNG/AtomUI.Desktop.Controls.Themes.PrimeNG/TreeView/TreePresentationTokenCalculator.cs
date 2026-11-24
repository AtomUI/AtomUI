using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

internal static class TreePresentationTokenCalculator
{
    internal static (double HeaderHeight, Color NodeHoverBg, Color NodeSelectedBg, Color NodeSelectedColor, Thickness TreeItemMargin, Thickness TreeItemHeaderPadding, Thickness TreeItemHeaderMargin, Thickness TreeNodeSwitcherMargin, Thickness TreeNodeIconMargin, Color FilterHighlightColor) Calculate(DesignToken token) =>
        (token.ControlHeightSM,
         token.ControlItemBgHover,
         token.ControlItemBgActive,
         token.ControlItemColorActive,
         new Thickness(0, 0, 0, token.UniformlyPaddingXXS / 2),
         new Thickness(token.UniformlyPaddingXS, 0),
         new Thickness(token.UniformlyMarginXXS, 0, 0, 0),
         new Thickness(0, 0, token.UniformlyPaddingXS / 2, 0),
         new Thickness(token.UniformlyPaddingXS / 2, 0, 0, 0),
         token.ColorError);
}
