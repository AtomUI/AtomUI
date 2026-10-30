using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Controls;

// 内容画布汇报内容的期望尺寸，使内容尺寸变化沿模板树触发 owner 重新测量。
// 偏移由主题声明式传入；本面板是内容的唯一排列 owner，包含偏移动画的每一帧。
internal sealed class ToggleSwitchContentPanel : Canvas
{
    public static readonly StyledProperty<Point> OnContentOffsetProperty =
        AbstractToggleSwitch.OnContentOffsetProperty.AddOwner<ToggleSwitchContentPanel>();

    public static readonly StyledProperty<Point> OffContentOffsetProperty =
        AbstractToggleSwitch.OffContentOffsetProperty.AddOwner<ToggleSwitchContentPanel>();

    public Point OnContentOffset
    {
        get => GetValue(OnContentOffsetProperty);
        set => SetValue(OnContentOffsetProperty, value);
    }

    public Point OffContentOffset
    {
        get => GetValue(OffContentOffsetProperty);
        set => SetValue(OffContentOffsetProperty, value);
    }

    static ToggleSwitchContentPanel()
    {
        AffectsArrange<ToggleSwitchContentPanel>(OnContentOffsetProperty, OffContentOffsetProperty);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var desired = default(Size);
        foreach (var child in Children)
        {
            child.Measure(availableSize);
            desired = new Size(Math.Max(desired.Width, child.DesiredSize.Width),
                Math.Max(desired.Height, child.DesiredSize.Height));
        }

        // owner 的居中偏移依赖每个 presenter 的尺寸，而不仅是模板根的最大尺寸。
        // 把手尺寸可能掩盖内容的尺寸变化，因此必须显式声明这条排列依赖。
        if (TemplatedParent is AbstractToggleSwitch owner)
        {
            owner.InvalidateArrange();
        }

        return desired;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        foreach (var child in Children)
        {
            var offset = child.Name == "PART_OnContentPresenter" ? OnContentOffset : OffContentOffset;
            child.Arrange(new Rect(offset, child.DesiredSize));
        }

        return finalSize;
    }
}
