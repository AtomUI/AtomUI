using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

internal static class RangePickerInputTokenValues
{
    internal static (Thickness ArrowMargin, double IndicatorThickness) Calculate(DesignToken token) =>
        (new Thickness(token.UniformlyMarginXS, 0), token.LineWidthBold);
}
