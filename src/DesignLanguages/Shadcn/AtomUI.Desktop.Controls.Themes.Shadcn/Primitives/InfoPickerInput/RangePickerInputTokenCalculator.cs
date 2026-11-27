using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

internal static class RangePickerInputTokenCalculator
{
    // Ant Design formula kept as is: no registry value is merged into the arrow margin and the indicator thickness.
    internal static (Thickness ArrowMargin, double IndicatorThickness) Calculate(DesignToken token) =>
        (new Thickness(token.UniformlyMarginXS, 0), token.LineWidthBold);
}
