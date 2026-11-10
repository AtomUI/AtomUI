using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal sealed class TreeViewToken : AbstractTreeViewToken
{
    [ReservedToken("antd Tree directory-node mode is not implemented.")]
    public Color DirectoryNodeSelectedColor { get; set; }

    [ReservedToken("antd Tree directory-node mode is not implemented.")]
    public Color DirectoryNodeSelectedBg { get; set; }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        DirectoryNodeSelectedBg = EffectiveGlobalToken.ColorPrimary;
        DirectoryNodeSelectedColor = EffectiveGlobalToken.ColorTextLightSolid;
    }
}
