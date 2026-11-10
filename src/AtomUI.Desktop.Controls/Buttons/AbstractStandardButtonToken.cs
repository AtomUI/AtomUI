using AtomUI.Controls;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Algorithms;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal abstract class AbstractStandardButtonToken : AbstractButtonGeometryToken
{
    public FontWeight FontWeight { get; set; }
    public Color DefaultBorderColorDisabled { get; set; }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        FontWeight = FontWeight.Normal;
        DefaultBorderColorDisabled = EffectiveGlobalToken.ColorBorder;

    }
}
