using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;

namespace AtomUI.Toolkits.GalleryBase.Controls;

/// <summary>
/// A vertical stack of children with the panel's full width, used as the sticky panel in the template of a
/// <see cref="GalleryStickyTabsHost" />. The child at <see cref="StickyIndex" /> is held at
/// <see cref="StickyOffsetY" /> from the top of the panel once its natural position lies above that line,
/// while the children after it keep their natural positions.
/// </summary>
public class GalleryStickyTabsPanel : Panel
{
    /// <summary>
    /// Identifies the <see cref="StickyIndex" /> property.
    /// </summary>
    public static readonly StyledProperty<int> StickyIndexProperty =
        AvaloniaProperty.Register<GalleryStickyTabsPanel, int>(nameof(StickyIndex), 1);

    /// <summary>
    /// Identifies the <see cref="StickyOffsetY" /> property.
    /// </summary>
    public static readonly StyledProperty<double> StickyOffsetYProperty =
        AvaloniaProperty.Register<GalleryStickyTabsPanel, double>(nameof(StickyOffsetY));

    /// <summary>
    /// Identifies the <see cref="IsStickyPinned" /> property.
    /// </summary>
    public static readonly DirectProperty<GalleryStickyTabsPanel, bool> IsStickyPinnedProperty =
        AvaloniaProperty.RegisterDirect<GalleryStickyTabsPanel, bool>(
            nameof(IsStickyPinned),
            o => o.IsStickyPinned);

    private ScrollViewer? _scrollViewer;
    private bool _isStickyPinned;

    /// <summary>
    /// The zero-based index of the child that sticks. A value that matches no child, such as a negative one,
    /// leaves the panel as a plain vertical stack with <see cref="IsStickyPinned" /> always
    /// <see langword="false" />.
    /// </summary>
    public int StickyIndex
    {
        get => GetValue(StickyIndexProperty);
        set => SetValue(StickyIndexProperty, value);
    }

    /// <summary>
    /// The minimum distance in DIPs from the top of the panel at which the child at <see cref="StickyIndex" />
    /// is arranged. The panel keeps this value equal to the vertical scroll offset of the nearest ancestor
    /// <see cref="ScrollViewer" />, so the sticky child stays at a fixed place in the viewport while held.
    /// With no ancestor <see cref="ScrollViewer" /> the panel never changes the value.
    /// </summary>
    public double StickyOffsetY
    {
        get => GetValue(StickyOffsetYProperty);
        set => SetValue(StickyOffsetYProperty, value);
    }

    /// <summary>
    /// <see langword="true" /> when the last arrange pass placed the child at <see cref="StickyIndex" /> below
    /// its natural position because that position lay above <see cref="StickyOffsetY" />. Only arrangement
    /// updates the value: it is <see langword="false" /> before the first arrange pass and when no child sticks.
    /// </summary>
    public bool IsStickyPinned
    {
        get => _isStickyPinned;
        private set => SetAndRaise(IsStickyPinnedProperty, ref _isStickyPinned, value);
    }

    static GalleryStickyTabsPanel()
    {
        AffectsMeasure<GalleryStickyTabsPanel>(StickyIndexProperty);
        AffectsArrange<GalleryStickyTabsPanel>(StickyOffsetYProperty);
    }

    /// <summary>
    /// Called when this panel is attached. After the base call it starts following the offset of the nearest
    /// ancestor <see cref="ScrollViewer" />, so that <see cref="StickyOffsetY" /> tracks the scroll position.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        AttachScrollViewer();
    }

    /// <summary>
    /// Called when this panel is detached. Stops following the <see cref="ScrollViewer" /> before the base call.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        DetachScrollViewer();
        base.OnDetachedFromVisualTree(e);
    }

    /// <summary>
    /// Measures the visible children with the available width and an unbounded height, and skips the invisible
    /// ones. Returns the widest child, widened to the available width when that width is finite, and the sum of
    /// the visible children's heights.
    /// </summary>
    protected override Size MeasureOverride(Size availableSize)
    {
        var measureWidth = double.IsInfinity(availableSize.Width)
            ? double.PositiveInfinity
            : Math.Max(0, availableSize.Width);
        var desiredWidth  = 0d;
        var desiredHeight = 0d;

        foreach (var child in Children)
        {
            if (!child.IsVisible)
            {
                continue;
            }

            child.Measure(new Size(measureWidth, double.PositiveInfinity));
            desiredWidth  = Math.Max(desiredWidth, child.DesiredSize.Width);
            desiredHeight += child.DesiredSize.Height;
        }

        if (!double.IsInfinity(availableSize.Width))
        {
            desiredWidth = Math.Max(desiredWidth, availableSize.Width);
        }

        return new Size(desiredWidth, desiredHeight);
    }

    /// <summary>
    /// Stacks the visible children from the top with their desired heights and the full width of
    /// <paramref name="finalSize" />, and gives invisible children an empty rect. The child at
    /// <see cref="StickyIndex" /> is placed no higher than <see cref="StickyOffsetY" />; the children after it
    /// keep their natural positions. Updates <see cref="IsStickyPinned" /> and returns
    /// <paramref name="finalSize" />.
    /// </summary>
    protected override Size ArrangeOverride(Size finalSize)
    {
        var y              = 0d;
        var isStickyPinned = false;
        for (var i = 0; i < Children.Count; i++)
        {
            var child = Children[i];
            if (!child.IsVisible)
            {
                child.Arrange(default);
                continue;
            }

            var height   = child.DesiredSize.Height;
            var naturalY = y;
            var arrangeY = i == StickyIndex
                ? Math.Max(naturalY, StickyOffsetY)
                : naturalY;

            child.Arrange(new Rect(0, arrangeY, finalSize.Width, height));
            if (i == StickyIndex)
            {
                isStickyPinned = arrangeY > naturalY;
            }

            y += height;
        }

        IsStickyPinned = isStickyPinned;
        return finalSize;
    }

    private void AttachScrollViewer()
    {
        var scrollViewer = this.GetVisualAncestors().OfType<ScrollViewer>().FirstOrDefault();
        if (ReferenceEquals(scrollViewer, _scrollViewer))
        {
            return;
        }

        DetachScrollViewer();
        _scrollViewer = scrollViewer;
        if (_scrollViewer is not null)
        {
            SetCurrentValue(StickyOffsetYProperty, _scrollViewer.Offset.Y);
            _scrollViewer.ScrollChanged += HandleScrollChanged;
        }
    }

    private void DetachScrollViewer()
    {
        if (_scrollViewer is null)
        {
            return;
        }

        _scrollViewer.ScrollChanged -= HandleScrollChanged;
        _scrollViewer = null;
    }

    private void HandleScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        if (sender is ScrollViewer scrollViewer)
        {
            SetCurrentValue(StickyOffsetYProperty, scrollViewer.Offset.Y);
        }
    }
}
