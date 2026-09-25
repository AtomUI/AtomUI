using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Toolkits.GalleryBase.Controls;

/// <summary>
/// 放大舞台的单子布局面板：内容以舞台宽度测量（高度无限）。
/// 期望宽度吃满舞台宽度的铺满型内容（24 栅格、表格等跟随约束的控件）按舞台宽度
/// 重新布局并铺满；期望宽度小于舞台的固定尺寸内容按期望尺寸水平垂直居中。
/// 不做任何拉伸或 RenderTransform 缩放——铺满是内容自适应重排，不是外部变换。
/// （历史：捕获卡片宽度保真机制与缩放机制均已按用户决策移除，见实施计划记录五至八。）
/// </summary>
public class ShowCaseZoomStagePanel : Panel
{
    protected override Size MeasureOverride(Size availableSize)
    {
        if (Children.Count == 0 || availableSize.Width <= 0 || availableSize.Height <= 0)
        {
            return new Size();
        }

        Children[0].Measure(new Size(availableSize.Width, double.PositiveInfinity));
        return availableSize;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        if (Children.Count > 0 && finalSize.Width > 0 && finalSize.Height > 0)
        {
            var child = Children[0];
            child.Measure(new Size(finalSize.Width, double.PositiveInfinity));
            var desired = child.DesiredSize;

            var width  = Math.Min(desired.Width, finalSize.Width);
            var height = Math.Min(desired.Height, finalSize.Height);
            var x      = (finalSize.Width - width) / 2;
            var y      = (finalSize.Height - height) / 2;

            child.Arrange(new Rect(x, y, width, height));
        }

        return finalSize;
    }
}
