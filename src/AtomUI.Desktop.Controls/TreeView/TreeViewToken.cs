using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class TreeViewToken : AbstractTreeViewToken
{
    [ReservedToken("antd Tree directory-node mode is not implemented.")]
    public partial Color DirectoryNodeSelectedColor { get; set; }

    [ReservedToken("antd Tree directory-node mode is not implemented.")]
    public partial Color DirectoryNodeSelectedBg { get; set; }

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        DirectoryNodeSelectedBg = EffectiveGlobalToken.ColorPrimary;
        DirectoryNodeSelectedColor = EffectiveGlobalToken.ColorTextLightSolid;
    }
}
