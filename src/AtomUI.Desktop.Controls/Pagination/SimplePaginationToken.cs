using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal sealed class SimplePaginationToken : AbstractPaginationToken
{
    public Color ItemInputBg { get; set; }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        ItemInputBg = EffectiveGlobalToken.ColorBgContainer;
    }
}
