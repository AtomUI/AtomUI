using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

namespace AtomUI.Controls.Commons;

internal class SegmentedStackPanel : StackPanel
{
    #region 公共属性定义

    public static readonly StyledProperty<bool> IsExpandingProperty =
        AvaloniaProperty.Register<SegmentedStackPanel, bool>(nameof(IsExpanding));

    public bool IsExpanding
    {
        get => GetValue(IsExpandingProperty);
        set => SetValue(IsExpandingProperty, value);
    }
    
    #endregion

    static SegmentedStackPanel()
    {
        AffectsMeasure<SegmentedStackPanel>(IsExpandingProperty);
        OrientationProperty.OverrideDefaultValue<SegmentedStackPanel>(Orientation.Horizontal);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        if (Orientation == Orientation.Vertical)
        {
            return MeasureVertical(availableSize);
        }

        return IsExpanding && !double.IsInfinity(availableSize.Width)
            ? MeasureHorizontalExpanding(availableSize)
            : MeasureHorizontalNatural(availableSize);
    }

    private Size MeasureHorizontalNatural(Size availableSize)
    {
        var layoutSlotSize = availableSize.WithWidth(double.PositiveInfinity);
        var targetWidth  = 0d;
        var targetHeight = 0d;
        foreach (var child in Children)
        {
            if (child is AbstractSegmentedItem { IsVisible: true } item)
            {
                item.Measure(layoutSlotSize);
                var childDesiredSize = item.DesiredSize;
                targetWidth  += childDesiredSize.Width;
                targetHeight = Math.Max(targetHeight, childDesiredSize.Height);
            }
        }

        return new Size(targetWidth, targetHeight);
    }

    private Size MeasureHorizontalExpanding(Size availableSize)
    {
        var visibleCount = CountVisibleSegmentedItems();
        if (visibleCount == 0)
        {
            return default;
        }

        var maxHeight = 0d;
        var childAvailableSize = new Size(availableSize.Width / visibleCount, availableSize.Height);
        foreach (var child in Children)
        {
            if (child is AbstractSegmentedItem { IsVisible: true } item)
            {
                item.Measure(childAvailableSize);
                maxHeight = Math.Max(maxHeight, item.DesiredSize.Height);
            }
        }

        return new Size(availableSize.Width, maxHeight);
    }

    private Size MeasureVertical(Size availableSize)
    {
        var expandsWidth = IsExpanding && !double.IsInfinity(availableSize.Width);
        var availableWidth = expandsWidth ? availableSize.Width : double.PositiveInfinity;
        var childAvailableSize = new Size(availableWidth, double.PositiveInfinity);
        var targetWidth = 0d;
        var targetHeight = 0d;

        foreach (var child in Children)
        {
            if (child is AbstractSegmentedItem { IsVisible: true } item)
            {
                item.Measure(childAvailableSize);
                targetWidth   = Math.Max(targetWidth, item.DesiredSize.Width);
                targetHeight += item.DesiredSize.Height;
            }
        }

        return new Size(expandsWidth ? availableSize.Width : targetWidth, targetHeight);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        if (Orientation == Orientation.Vertical)
        {
            ArrangeVertical(finalSize);
        }
        else if (IsExpanding)
        {
            ArrangeHorizontalExpanding(finalSize);
        }
        else
        {
            ArrangeHorizontalNatural(finalSize);
        }

        return finalSize;
    }

    private void ArrangeHorizontalNatural(Size finalSize)
    {
        var offsetX = 0d;
        foreach (var child in Children)
        {
            if (child is AbstractSegmentedItem { IsVisible: true } item)
            {
                item.Arrange(new Rect(offsetX, 0, item.DesiredSize.Width,
                    Math.Max(finalSize.Height, item.DesiredSize.Height)));
                offsetX += item.DesiredSize.Width;
            }
        }
    }

    private void ArrangeHorizontalExpanding(Size finalSize)
    {
        var visibleCount = CountVisibleSegmentedItems();
        if (visibleCount == 0)
        {
            return;
        }

        var width   = finalSize.Width / visibleCount;
        var offsetX = 0d;
        foreach (var child in Children)
        {
            if (child is AbstractSegmentedItem { IsVisible: true } item)
            {
                item.Arrange(new Rect(offsetX, 0, width, Math.Max(finalSize.Height, item.DesiredSize.Height)));
                offsetX += width;
            }
        }
    }

    private void ArrangeVertical(Size finalSize)
    {
        var offsetY = 0d;
        foreach (var child in Children)
        {
            if (child is AbstractSegmentedItem { IsVisible: true } item)
            {
                item.Arrange(new Rect(0, offsetY, finalSize.Width, item.DesiredSize.Height));
                offsetY += item.DesiredSize.Height;
            }
        }
    }

    private int CountVisibleSegmentedItems()
    {
        var count = 0;
        foreach (var child in Children)
        {
            if (child is AbstractSegmentedItem { IsVisible: true })
            {
                count++;
            }
        }

        return count;
    }
}
