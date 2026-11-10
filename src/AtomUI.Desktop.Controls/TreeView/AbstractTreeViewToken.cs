using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal abstract class AbstractTreeViewToken : AbstractTreePresentationToken
{






    /// <summary>
    /// 目录树节点选中文字颜色
    /// </summary>

    /// <summary>
    /// 目录树节点选中背景色
    /// </summary>






    /// <summary>
    /// 拖动指示器
    /// </summary>
    public double DragIndicatorLineWidth { get; set; }


    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);


        DragIndicatorLineWidth = EffectiveGlobalToken.LineWidthBold;
    }

}
