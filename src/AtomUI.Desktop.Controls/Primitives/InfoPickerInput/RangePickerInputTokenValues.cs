using AtomUI.Theme.DesignTokens;
using AtomUI.Desktop.Controls.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls;

internal static class RangePickerInputTokenValues
{
    internal static (Thickness ArrowMargin, double IndicatorThickness) Calculate(DesignToken token) =>
        (new Thickness(token.UniformlyMarginXS, 0), token.LineWidthFocus);
}
