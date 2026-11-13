using AtomUI.Controls;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Algorithms;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public abstract partial class AbstractStandardButtonToken : AbstractButtonGeometryToken
{
    public partial FontWeight FontWeight { get; set; }
    public partial Color DefaultBorderColorDisabled { get; set; }

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        FontWeight = FontWeight.Normal;
        DefaultBorderColorDisabled = EffectiveGlobalToken.ColorBorder;

    }
}
