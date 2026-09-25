using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Toolkits.GalleryBase.Controls;

/// <summary>
/// 放大舞台的单子布局面板：内容以捕获的布局宽度（未捕获时以舞台宽度）测量（高度无限），
/// 按期望尺寸水平垂直居中，**不做任何拉伸或 RenderTransform 缩放**——
/// 保证放大态内容与原 ShowCaseItem 卡片内的布局（换行、间距、控件尺寸）完全一致。
/// （缩放机制曾于两段式版本引入并被移除：对单个控件等比放大会被读作渲染 bug，
/// 见实施计划缺陷修复记录五、六。）
/// </summary>
public class ShowCaseZoomStagePanel : Panel
{
    public static readonly StyledProperty<double> StageMeasureWidthProperty =
        AvaloniaProperty.Register<ShowCaseZoomStagePanel, double>(nameof(StageMeasureWidth), double.NaN);

    public double StageMeasureWidth
    {
        get => GetValue(StageMeasureWidthProperty);
        set => SetValue(StageMeasureWidthProperty, value);
    }

    private double ResolveMeasureWidth(double stageWidth)
    {
        var captured = StageMeasureWidth;
        if (double.IsNaN(captured) || captured <= 0)
        {
            return stageWidth;
        }

        return Math.Min(captured, stageWidth);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        if (Children.Count == 0 || availableSize.Width <= 0 || availableSize.Height <= 0)
        {
            return new Size();
        }

        Children[0].Measure(new Size(ResolveMeasureWidth(availableSize.Width), double.PositiveInfinity));
        return availableSize;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        if (Children.Count > 0 && finalSize.Width > 0 && finalSize.Height > 0)
        {
            var child = Children[0];
            child.Measure(new Size(ResolveMeasureWidth(finalSize.Width), double.PositiveInfinity));
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
