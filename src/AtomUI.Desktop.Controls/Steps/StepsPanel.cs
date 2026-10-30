using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

namespace AtomUI.Desktop.Controls;

internal class StepsPanel : Panel
{
    public static readonly StyledProperty<StepsType> TypeProperty =
        AvaloniaProperty.Register<StepsPanel, StepsType>(nameof(Type), StepsType.Default);

    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<StepsPanel, Orientation>(nameof(Orientation), Orientation.Horizontal);

    public static readonly StyledProperty<Orientation> TitlePlacementProperty =
        AvaloniaProperty.Register<StepsPanel, Orientation>(nameof(TitlePlacement), Orientation.Horizontal);

    public static readonly StyledProperty<int> OffsetProperty =
        AvaloniaProperty.Register<StepsPanel, int>(nameof(Offset));

    public static readonly StyledProperty<HorizontalAlignment> HorizontalContentAlignmentProperty =
        AvaloniaProperty.Register<StepsPanel, HorizontalAlignment>(
            nameof(HorizontalContentAlignment),
            HorizontalAlignment.Center);

    public static readonly StyledProperty<double> MinItemWidthProperty =
        AvaloniaProperty.Register<StepsPanel, double>(nameof(MinItemWidth));

    public StepsType Type
    {
        get => GetValue(TypeProperty);
        set => SetValue(TypeProperty, value);
    }

    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public Orientation TitlePlacement
    {
        get => GetValue(TitlePlacementProperty);
        set => SetValue(TitlePlacementProperty, value);
    }

    public int Offset
    {
        get => GetValue(OffsetProperty);
        set => SetValue(OffsetProperty, value);
    }

    public HorizontalAlignment HorizontalContentAlignment
    {
        get => GetValue(HorizontalContentAlignmentProperty);
        set => SetValue(HorizontalContentAlignmentProperty, value);
    }

    public double MinItemWidth
    {
        get => GetValue(MinItemWidthProperty);
        set => SetValue(MinItemWidthProperty, value);
    }

    private double[] _itemBases      = [];
    private double[] _computedWidths = [];
    private double   _measureWidth   = double.NaN;

    static StepsPanel()
    {
        AffectsMeasure<StepsPanel>(
            TypeProperty,
            OrientationProperty,
            TitlePlacementProperty,
            OffsetProperty,
            MinItemWidthProperty);
        AffectsArrange<StepsPanel>(
            TypeProperty,
            OrientationProperty,
            TitlePlacementProperty,
            OffsetProperty,
            HorizontalContentAlignmentProperty,
            MinItemWidthProperty);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        return EffectiveOrientation == Orientation.Vertical
            ? MeasureVertically(availableSize)
            : MeasureHorizontally(availableSize);
    }

    private Size MeasureVertically(Size availableSize)
    {
        var width  = 0d;
        var height = 0d;
        foreach (var child in Children)
        {
            child.Measure(availableSize);
            if (!child.IsVisible)
            {
                continue;
            }

            width = Math.Max(width, child.DesiredSize.Width);
            height += child.DesiredSize.Height;
        }

        return new Size(width, height);
    }

    private Size MeasureHorizontally(Size availableSize)
    {
        var visible = Children.Where(static child => child.IsVisible).ToArray();

        // Pass 1: measure the natural (unconstrained) width of every visible item so
        // the flex share algorithm can distribute the available width over them.
        var bases = new double[visible.Length];
        for (var index = 0; index < visible.Length; ++index)
        {
            visible[index].Measure(new Size(double.PositiveInfinity, availableSize.Height));
            bases[index] = visible[index].DesiredSize.Width;
        }

        foreach (var child in Children)
        {
            if (!child.IsVisible)
            {
                child.Measure(availableSize);
            }
        }

        // DesiredSize describes the space needed by the content, not the spare space
        // offered by a parent (for example an auto-sized Dialog). Stretch belongs to
        // Arrange. Keep constrained re-measurement so narrow rows report wrapped height.
        var measureWidth = Math.Min(availableSize.Width, CalculateNaturalHorizontalWidth(bases));
        var widths = ComputeHorizontalItemWidths(bases, measureWidth);
        var width = Type == StepsType.Inline && widths.Length > 0
            ? widths[0] * Math.Max(0, Offset)
            : 0d;
        var height = 0d;
        for (var index = 0; index < visible.Length; ++index)
        {
            visible[index].Measure(new Size(widths[index], availableSize.Height));
            width  += widths[index];
            height =  Math.Max(height, visible[index].DesiredSize.Height);
        }

        _itemBases      = bases;
        _computedWidths = widths;
        _measureWidth   = measureWidth;

        return new Size(width, height);
    }

    private double CalculateNaturalHorizontalWidth(double[] bases)
    {
        if (bases.Length == 0)
        {
            return 0;
        }

        var minWidth = Type == StepsType.Panel ? 0 : Math.Max(0, MinItemWidth);
        if (Type is StepsType.Navigation or StepsType.Panel || ShouldArrangeTitleVerticalItemsEqually())
        {
            var offset = Type == StepsType.Inline ? Math.Max(0, Offset) : 0;
            return Math.Max(minWidth, bases.Max()) * (bases.Length + (double)offset);
        }

        // Non-last items share equal cells; the last item keeps its content width.
        // Reserve the widest non-last basis so an intrinsic arrangement cannot wrap
        // an item that was measured unconstrained or overflow the requested row width.
        var cellWidth = minWidth;
        for (var index = 0; index < bases.Length - 1; index++)
        {
            cellWidth = Math.Max(cellWidth, bases[index]);
        }

        return cellWidth * (bases.Length - 1) + Math.Max(minWidth, bases[^1]);
    }

    private double[] ComputeHorizontalItemWidths(double[] bases, double availableWidth)
    {
        var count  = bases.Length;
        var widths = new double[count];
        if (count == 0)
        {
            return widths;
        }

        if (!double.IsFinite(availableWidth))
        {
            Array.Copy(bases, widths, count);
            return widths;
        }

        var minWidth = Math.Max(0, MinItemWidth);

        if (Type == StepsType.Navigation)
        {
            var share = Math.Max(availableWidth / count, minWidth);
            Array.Fill(widths, share);
            return widths;
        }

        if (Type == StepsType.Panel)
        {
            var share = availableWidth / count;
            Array.Fill(widths, share);
            return widths;
        }

        if (StepsItemLayoutPanel.ResolveTitlePlacement(Type, Orientation, TitlePlacement) == Orientation.Vertical)
        {
            var offset = Type == StepsType.Inline ? Math.Max(0, Offset) : 0;
            var share = Math.Max(availableWidth / (count + (double)offset), minWidth);
            Array.Fill(widths, share);
            return widths;
        }

        if (count == 1)
        {
            widths[0] = Math.Max(minWidth, Math.Min(bases[0], availableWidth));
            return widths;
        }

        var totalBasis = 0d;
        foreach (var basis in bases)
        {
            totalBasis += basis;
        }

        if (totalBasis <= availableWidth)
        {
            // Wide: preserve the existing stretch semantics — the non-last items
            // split the space left after the last item keeps its content width.
            var lastWidth = Math.Max(minWidth, bases[^1]);
            var share     = Math.Max(0, (availableWidth - lastWidth) / (count - 1));
            for (var index = 0; index < count - 1; ++index)
            {
                widths[index] = Math.Max(share, Math.Max(minWidth, bases[index]));
            }

            widths[^1] = lastWidth;
            return widths;
        }

        return ShrinkItemsToFit(bases, availableWidth, minWidth);
    }

    private static double[] ShrinkItemsToFit(double[] bases, double availableWidth, double minWidth)
    {
        var count  = bases.Length;
        var widths = new double[count];
        Array.Copy(bases, widths, count);

        // Flex-shrink: the negative space is distributed proportionally to each
        // item's basis; items frozen at the MinItemWidth floor stop participating.
        var frozen = new bool[count];
        while (true)
        {
            var frozenWidth = 0d;
            var activeBasis = 0d;
            for (var index = 0; index < count; ++index)
            {
                if (frozen[index])
                {
                    frozenWidth += widths[index];
                    continue;
                }

                if (widths[index] <= minWidth)
                {
                    widths[index] = minWidth;
                    frozen[index] = true;
                    frozenWidth += minWidth;
                    continue;
                }

                activeBasis += bases[index];
            }

            var remaining = availableWidth - frozenWidth;
            if (activeBasis <= 0 || remaining <= 0)
            {
                break;
            }

            var ratio   = remaining / activeBasis;
            var changed = false;
            for (var index = 0; index < count; ++index)
            {
                if (frozen[index])
                {
                    continue;
                }

                var width = bases[index] * ratio;
                if (width <= minWidth)
                {
                    widths[index] = minWidth;
                    frozen[index] = true;
                    changed      = true;
                }
                else
                {
                    widths[index] = width;
                }
            }

            if (!changed)
            {
                break;
            }
        }

        return widths;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var children = Children.Where(static child => child.IsVisible).ToArray();
        if (children.Length == 0)
        {
            return finalSize;
        }

        var orientation = EffectiveOrientation;

        if (orientation == Orientation.Vertical)
        {
            if (Type == StepsType.Navigation)
            {
                ArrangeVerticalNavigation(children, finalSize);
            }
            else
            {
                ArrangeVertically(children, finalSize);
            }
            return finalSize;
        }

        var widths = GetHorizontalArrangeWidths(children, finalSize.Width);

        if (ShouldArrangeTitleVerticalItemsEqually())
        {
            ArrangeTitleVertical(children, finalSize, widths);
            return finalSize;
        }

        switch (Type)
        {
            case StepsType.Navigation:
                ArrangeNavigation(children, finalSize, widths);
                break;

            case StepsType.Panel:
                ArrangeNavigation(children, finalSize, panel: true);
                break;

            default:
                ArrangeDefault(children, finalSize, widths);
                break;
        }

        return finalSize;
    }

    private double[] GetHorizontalArrangeWidths(IReadOnlyList<Control> children, double finalWidth)
    {
        if (_computedWidths.Length == children.Count && Math.Abs(finalWidth - _measureWidth) < 0.01)
        {
            return _computedWidths;
        }

        var bases = _itemBases.Length == children.Count
            ? _itemBases
            : children.Select(static child => child.DesiredSize.Width).ToArray();
        return ComputeHorizontalItemWidths(bases, finalWidth);
    }

    private static void ArrangeVertically(IReadOnlyList<Control> children, Size finalSize)
    {
        var y = 0d;
        foreach (var child in children)
        {
            var height = child.DesiredSize.Height;
            child.Arrange(new Rect(0, y, finalSize.Width, height));
            y += height;
        }
    }

    private void ArrangeVerticalNavigation(IReadOnlyList<Control> children, Size finalSize)
    {
        var width = HorizontalContentAlignment == HorizontalAlignment.Stretch
            ? finalSize.Width
            : Math.Min(finalSize.Width, children.Max(static child => child.DesiredSize.Width));
        var x = GetHorizontalAlignedX(finalSize.Width, width);
        var y     = 0d;
        foreach (var child in children)
        {
            var height = child.DesiredSize.Height;
            child.Arrange(new Rect(x, y, width, height));
            y += height;
        }
    }

    private double GetHorizontalAlignedX(double finalWidth, double contentWidth)
    {
        return HorizontalContentAlignment switch
        {
            HorizontalAlignment.Right  => Math.Max(0, finalWidth - contentWidth),
            HorizontalAlignment.Center => Math.Max(0, (finalWidth - contentWidth) / 2),
            _                          => 0
        };
    }

    private bool ShouldArrangeTitleVerticalItemsEqually()
    {
        return StepsItemLayoutPanel.ResolveTitlePlacement(Type, EffectiveOrientation, TitlePlacement) == Orientation.Vertical;
    }

    private Orientation EffectiveOrientation => Type == StepsType.Panel ? Orientation.Horizontal : Orientation;

    private static void ArrangeNavigation(
        IReadOnlyList<Control> children,
        Size finalSize,
        double[]? widths = null,
        bool panel = false)
    {
        var equalWidth = finalSize.Width / children.Count;
        var x = 0d;
        for (var index = 0; index < children.Count; index++)
        {
            var child = children[index];
            child.ZIndex = panel ? children.Count - index : 0;
            var width = widths is not null ? widths[index] : equalWidth;
            child.Arrange(new Rect(x, 0, width, finalSize.Height));
            x += width;
        }
    }

    private void ArrangeTitleVertical(IReadOnlyList<Control> children, Size finalSize, double[] widths)
    {
        var offset    = Type == StepsType.Inline ? Math.Max(0, Offset) : 0;
        var share     = finalSize.Width / (children.Count + offset);
        var rowHeight = Math.Min(finalSize.Height, children.Max(static child => child.DesiredSize.Height));
        var rowY      = Math.Max(0, (finalSize.Height - rowHeight) / 2);
        var x         = share * offset;

        for (var index = 0; index < children.Count; ++index)
        {
            children[index].Arrange(new Rect(x, rowY, widths[index], rowHeight));
            x += widths[index];
        }
    }

    private static void ArrangeDefault(IReadOnlyList<Control> children, Size finalSize, double[] widths)
    {
        var rowHeight = Math.Min(finalSize.Height, children.Max(static child => child.DesiredSize.Height));
        var rowY      = Math.Max(0, (finalSize.Height - rowHeight) / 2);

        var x = 0d;
        for (var index = 0; index < children.Count; ++index)
        {
            children[index].Arrange(new Rect(x, rowY, widths[index], rowHeight));
            x += widths[index];
        }
    }
}
