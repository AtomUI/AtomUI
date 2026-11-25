using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace AtomUI.Controls;

// 内容画布汇报内容的期望尺寸，使内容尺寸变化沿模板树触发 owner 重新测量。
// 偏移由模板提供。内容排列在固定槽，过渡只更新渲染平移。
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

    internal static void SetContentOffset(Control? child, Point offset)
    {
        if (child is null)
        {
            return;
        }

        if (child.RenderTransform is TranslateTransform transform)
        {
            transform.X = offset.X;
            transform.Y = offset.Y;
        }
        else
        {
            child.SetCurrentValue(RenderTransformProperty, new TranslateTransform(offset.X, offset.Y));
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == OnContentOffsetProperty || change.Property == OffContentOffsetProperty)
        {
            foreach (var child in Children)
            {
                ApplyOffset(child);
            }
        }
    }

    private void ApplyOffset(Control child) =>
        SetContentOffset(child, child.Name == "PART_OnContentPresenter" ? OnContentOffset : OffContentOffset);

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
            child.Arrange(new Rect(child.DesiredSize));
            ApplyOffset(child);
        }

        return finalSize;
    }
}
