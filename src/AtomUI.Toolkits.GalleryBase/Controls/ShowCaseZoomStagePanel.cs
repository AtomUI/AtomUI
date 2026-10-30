using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Toolkits.GalleryBase.Controls;

/// <summary>
/// 放大舞台的单子布局面板：Natural 按舞台宽度测量并以期望尺寸居中；
/// Stretch 使用舞台的有限测量与排列槽。
/// Width、Height、Min/Max 和 Margin 继续由子控件自身的 Arrange 处理。
/// 不做任何拉伸或 RenderTransform 缩放——铺满是内容自适应重排，不是外部变换。
/// （历史：捕获卡片宽度保真机制与缩放机制均已按用户决策移除，见实施计划记录五至八。）
/// </summary>
public class ShowCaseZoomStagePanel : Panel
{
    public static readonly StyledProperty<ShowCaseZoomContentLayout> ZoomContentLayoutProperty =
        ShowCaseItem.ZoomContentLayoutProperty.AddOwner<ShowCaseZoomStagePanel>();

    public ShowCaseZoomContentLayout ZoomContentLayout
    {
        get => GetValue(ZoomContentLayoutProperty);
        set => SetValue(ZoomContentLayoutProperty, value);
    }

    static ShowCaseZoomStagePanel()
    {
        AffectsMeasure<ShowCaseZoomStagePanel>(ZoomContentLayoutProperty);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        if (Children.Count == 0 || availableSize.Width <= 0 || availableSize.Height <= 0)
        {
            return new Size();
        }

        MeasureContent(availableSize);
        var desired = Children[0].DesiredSize;
        return new Size(
            double.IsFinite(availableSize.Width) ? availableSize.Width : desired.Width,
            double.IsFinite(availableSize.Height) ? availableSize.Height : desired.Height);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        if (Children.Count > 0 && finalSize.Width > 0 && finalSize.Height > 0)
        {
            var child = Children[0];
            MeasureContent(finalSize);
            var desired = child.DesiredSize;

            // 布局策略是显式、可观察的契约，不从对齐属性的赋值来源或控件类型推断。
            var width = ZoomContentLayout == ShowCaseZoomContentLayout.Stretch
                ? finalSize.Width : Math.Min(desired.Width, finalSize.Width);
            var height = ZoomContentLayout == ShowCaseZoomContentLayout.Stretch
                ? finalSize.Height : Math.Min(desired.Height, finalSize.Height);
            var x      = (finalSize.Width - width) / 2;
            var y      = (finalSize.Height - height) / 2;

            child.Arrange(new Rect(x, y, width, height));
        }

        return finalSize;
    }

    private void MeasureContent(Size availableSize)
    {
        Children[0].Measure(ZoomContentLayout == ShowCaseZoomContentLayout.Stretch
            ? availableSize
            : new Size(availableSize.Width, double.PositiveInfinity));
    }
}
