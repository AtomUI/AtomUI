using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class SimplePaginationToken : AbstractPaginationToken
{
    public partial Color ItemInputBg { get; set; }

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        ItemInputBg = EffectiveGlobalToken.ColorBgContainer;
    }
}
