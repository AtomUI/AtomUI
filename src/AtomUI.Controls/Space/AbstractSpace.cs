using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Metadata;

namespace AtomUI.Controls.Commons;

/// <summary>
/// Shared layout host of the Space controls: lays its children out along one main axis, optionally wraps
/// them into lines, aligns them inside a line and distributes the remaining main-axis space of each line.
/// </summary>
public abstract class AbstractSpace : Control,
                                      IChildIndexProvider,
                                      INavigableContainer
{
    #region 公共属性定义

    public static readonly StyledProperty<double> ItemSpacingProperty =
        AvaloniaProperty.Register<AbstractSpace, double>(nameof(ItemSpacing));

    public static readonly StyledProperty<double> LineSpacingProperty =
        AvaloniaProperty.Register<AbstractSpace, double>(nameof(LineSpacing));

    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<AbstractSpace, Orientation>(nameof(Orientation), defaultValue: Orientation.Horizontal);

    public static readonly StyledProperty<SpaceItemsAlignment> ItemsAlignmentProperty =
        AvaloniaProperty.Register<AbstractSpace, SpaceItemsAlignment>(nameof(ItemsAlignment), defaultValue: SpaceItemsAlignment.Start);

    public static readonly StyledProperty<double> ItemWidthProperty =
        AvaloniaProperty.Register<AbstractSpace, double>(nameof(ItemWidth), double.NaN);

    public static readonly StyledProperty<double> ItemHeightProperty =
        AvaloniaProperty.Register<AbstractSpace, double>(nameof(ItemHeight), double.NaN);

    public static readonly StyledProperty<bool> IsWrapProperty =
        AvaloniaProperty.Register<AbstractSpace, bool>(nameof(IsWrap), true);

    public static readonly StyledProperty<SpaceJustify> JustifyProperty =
        AvaloniaProperty.Register<AbstractSpace, SpaceJustify>(nameof(Justify), SpaceJustify.Start);

    public double ItemSpacing
    {
        get => GetValue(ItemSpacingProperty);
        set => SetValue(ItemSpacingProperty, value);
    }

    public double LineSpacing
    {
        get => GetValue(LineSpacingProperty);
        set => SetValue(LineSpacingProperty, value);
    }

    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public SpaceItemsAlignment ItemsAlignment
    {
        get => GetValue(ItemsAlignmentProperty);
        set => SetValue(ItemsAlignmentProperty, value);
    }

    public double ItemWidth
    {
        get => GetValue(ItemWidthProperty);
        set => SetValue(ItemWidthProperty, value);
    }

    public double ItemHeight
    {
        get => GetValue(ItemHeightProperty);
        set => SetValue(ItemHeightProperty, value);
    }

    /// <summary>
    /// Whether items that do not fit into the main-axis length start a new line. When <c>false</c> all items stay
    /// on one line and the exceeding part overflows.
    /// </summary>
    public bool IsWrap
    {
        get => GetValue(IsWrapProperty);
        set => SetValue(IsWrapProperty, value);
    }

    /// <summary>
    /// How the remaining main-axis space of every line is distributed.
    /// </summary>
    public SpaceJustify Justify
    {
        get => GetValue(JustifyProperty);
        set => SetValue(JustifyProperty, value);
    }

    [Content]
    public AvaloniaList<Control> Children { get; } = new();

    public bool IsItemsHost { get; internal set; }

    #endregion

    private EventHandler<ChildIndexChangedEventArgs>? _childIndexChanged;

    static AbstractSpace()
    {
        AffectsMeasure<AbstractSpace>(ItemSpacingProperty,
            LineSpacingProperty,
            OrientationProperty,
            ItemWidthProperty,
            ItemHeightProperty,
            IsWrapProperty);
        AffectsArrange<AbstractSpace>(ItemsAlignmentProperty, JustifyProperty);
    }

    protected AbstractSpace()
    {
        Children.CollectionChanged += HandleChildrenChanged;
    }

    protected virtual void HandleChildrenChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                if (!IsItemsHost)
                {
                    ControlCollectionChangedUtils.InsertLogicalControls(LogicalChildren, e.NewStartingIndex, e.NewItems!);
                }
                ControlCollectionChangedUtils.InsertVisuals(VisualChildren, e.NewStartingIndex, e.NewItems!);
                break;

            case NotifyCollectionChangedAction.Move:
                if (!IsItemsHost)
                {
                    LogicalChildren.MoveRange(e.OldStartingIndex, e.OldItems!.Count, e.NewStartingIndex);
                }
                VisualChildren.MoveRange(e.OldStartingIndex, e.OldItems!.Count, e.NewStartingIndex);
                break;

            case NotifyCollectionChangedAction.Remove:
                if (!IsItemsHost)
                {
                    LogicalChildren.RemoveRange(e.OldStartingIndex, e.OldItems!.Count);
                }
                VisualChildren.RemoveRange(e.OldStartingIndex, e.OldItems!.Count);
                break;

            case NotifyCollectionChangedAction.Replace:
                for (var i = 0; i < e.OldItems!.Count; ++i)
                {
                    var index = i + e.OldStartingIndex;
                    var child = (Control)e.NewItems![i]!;
                    if (!IsItemsHost)
                    {
                        LogicalChildren[index] = child;
                    }
                    VisualChildren[index] = child;
                }
                break;

            case NotifyCollectionChangedAction.Reset:
                throw new NotSupportedException();
        }
        _childIndexChanged?.Invoke(this, ChildIndexChangedEventArgs.ChildIndexesReset);
        InvalidateMeasureOnChildrenChanged();
    }

    private protected virtual void InvalidateMeasureOnChildrenChanged()
    {
        InvalidateMeasure();
    }

    /// <summary>
    /// Space reserved around the lines (for example padding and border of the owner); measured size includes it and
    /// children are arranged inside it.
    /// </summary>
    private protected virtual Thickness GetLayoutInset()
    {
        return default;
    }

    protected override Size MeasureOverride(Size constraint)
    {
        double itemWidth     = ItemWidth;
        double itemHeight    = ItemHeight;
        double itemSpacing   = ItemSpacing;
        double lineSpacing   = LineSpacing;
        bool   isWrap        = IsWrap;
        var    orientation   = Orientation;
        var    children      = VisualChildren;
        var    frameInset    = GetLayoutInset();
        var    layoutConstraint = new Size(
            Math.Max(0, constraint.Width - frameInset.Left - frameInset.Right),
            Math.Max(0, constraint.Height - frameInset.Top - frameInset.Bottom));
        var    curLineSize   = new UVSize(orientation);
        var    panelSize     = new UVSize(orientation);
        var    uvConstraint  = new UVSize(orientation, layoutConstraint.Width, layoutConstraint.Height);
        bool   itemWidthSet  = !double.IsNaN(itemWidth);
        bool   itemHeightSet = !double.IsNaN(itemHeight);
        bool   itemExists    = false;
        bool   lineExists    = false;

        var childConstraint = new Size(
            itemWidthSet ? itemWidth : layoutConstraint.Width,
            itemHeightSet ? itemHeight : layoutConstraint.Height);

        for (int i = 0, count = children.Count; i < count; ++i)
        {
            var child = children[i] as Control;
            Debug.Assert(child != null);
            // Flow passes its own constraint to children
            child.Measure(childConstraint);

            if (!child.IsVisible)
            {
                continue;
            }

            // This is the size of the child in UV space
            UVSize childSize = new UVSize(orientation,
                itemWidthSet ? itemWidth : child.DesiredSize.Width,
                itemHeightSet ? itemHeight : child.DesiredSize.Height);

            var nextSpacing = itemExists ? itemSpacing : 0;
            // Only a line that already holds an item can be broken: an oversized first item starts the first line
            // instead of leaving an empty line (and its line spacing) above it.
            if (isWrap && itemExists && MathUtils.GreaterThan(curLineSize.U + childSize.U + nextSpacing, uvConstraint.U)) // Need to switch to another line
            {
                panelSize.U =  Math.Max(curLineSize.U, panelSize.U);
                panelSize.V += curLineSize.V + (lineExists ? lineSpacing : 0);
                curLineSize =  childSize;

                itemExists = true;
                lineExists = true;
            }
            else // Continue to accumulate a line
            {
                curLineSize.U += childSize.U + nextSpacing;
                curLineSize.V =  Math.Max(childSize.V, curLineSize.V);

                itemExists = true;
            }
        }

        // The last line size, if any should be added
        panelSize.U =  Math.Max(curLineSize.U, panelSize.U);
        panelSize.V += curLineSize.V + (lineExists ? lineSpacing : 0);

        // Go from UV space to W/H space
        return new Size(panelSize.Width, panelSize.Height).Inflate(frameInset);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        double itemWidth     = ItemWidth;
        double itemHeight    = ItemHeight;
        double itemSpacing   = ItemSpacing;
        double lineSpacing   = LineSpacing;
        bool   isWrap        = IsWrap;
        var    justify       = Justify;
        var    itemsAlign    = ItemsAlignment;
        var    orientation   = Orientation;
        bool   isHorizontal  = orientation == Orientation.Horizontal;
        var    children      = VisualChildren;
        var    frameInset    = GetLayoutInset();
        var    layoutSize    = finalSize.Deflate(frameInset);
        int    firstInLine   = 0;
        double accumulatedV  = 0;
        double itemU         = isHorizontal ? itemWidth : itemHeight;
        double itemV         = isHorizontal ? itemHeight : itemWidth;
        var    curLineSize   = new UVSize(orientation);
        var    uvFinalSize   = new UVSize(orientation, layoutSize.Width, layoutSize.Height);
        bool   itemWidthSet  = !double.IsNaN(itemWidth);
        bool   itemHeightSet = !double.IsNaN(itemHeight);
        bool   itemExists    = false;
        bool   lineExists    = false;

        for (int i = 0; i < children.Count; ++i)
        {
            var child = children[i] as Control;
            Debug.Assert(child != null);
            if (!child.IsVisible)
            {
                continue;
            }
            var childSize = new UVSize(orientation,
                itemWidthSet ? itemWidth : child.DesiredSize.Width,
                itemHeightSet ? itemHeight : child.DesiredSize.Height);

            var nextSpacing = itemExists ? itemSpacing : 0;
            if (isWrap && itemExists && MathUtils.GreaterThan(curLineSize.U + childSize.U + nextSpacing, uvFinalSize.U)) // Need to switch to another line
            {
                accumulatedV += lineExists ? lineSpacing : 0; // add spacing to arrange line first
                ArrangeLine(curLineSize.U, curLineSize.V, firstInLine, i);
                accumulatedV += curLineSize.V; // add the height of the line just arranged
                curLineSize  =  childSize;

                firstInLine = i;

                itemExists = true;
                lineExists = true;
            }
            else // Continue to accumulate a line
            {
                curLineSize.U += childSize.U + nextSpacing;
                curLineSize.V =  Math.Max(childSize.V, curLineSize.V);

                itemExists = true;
            }
        }

        // Arrange the last line, if any
        if (firstInLine < children.Count)
        {
            accumulatedV += lineExists ? lineSpacing : 0; // add spacing to arrange line first
            ArrangeLine(curLineSize.U, curLineSize.V, firstInLine, children.Count);
        }

        return finalSize;

        void ArrangeLine(double lineU, double lineV, int start, int end)
        {
            bool   useItemU = isHorizontal ? itemWidthSet : itemHeightSet;
            bool   useItemV = isHorizontal ? itemHeightSet : itemWidthSet;

            // Distribute the remaining main-axis space of this line; it never resizes items.
            int visibleCount = 0;
            for (int i = start; i < end; ++i)
            {
                if (children[i] is Control { IsVisible: true })
                {
                    ++visibleCount;
                }
            }
            double remaining = Math.Max(0, uvFinalSize.U - lineU);
            double leadSpace = 0;
            double extraGap  = 0;
            if (visibleCount > 0 && remaining > 0)
            {
                switch (justify)
                {
                    case SpaceJustify.End:
                        leadSpace = remaining;
                        break;
                    case SpaceJustify.Center:
                        leadSpace = remaining / 2;
                        break;
                    case SpaceJustify.Between:
                        if (visibleCount > 1)
                        {
                            extraGap = remaining / (visibleCount - 1);
                        }
                        break;
                    case SpaceJustify.Around:
                        leadSpace = remaining / (2 * visibleCount);
                        extraGap  = remaining / visibleCount;
                        break;
                    case SpaceJustify.Evenly:
                        leadSpace = remaining / (visibleCount + 1);
                        extraGap  = leadSpace;
                        break;
                }
            }

            double v     = accumulatedV;
            double u     = leadSpace;
            bool   first = true;
            for (int i = start; i < end; ++i)
            {
                var    newChild    = children[i] as Control;
                Debug.Assert(newChild != null);
                if (!newChild.IsVisible)
                {
                    continue;
                }
                double layoutSlotV = GetChildV(i);
                double layoutSlotU = GetChildU(i);
                if (!first)
                {
                    u += itemSpacing + extraGap;
                }
                switch (itemsAlign)
                {
                    case SpaceItemsAlignment.Start:
                        v = accumulatedV;
                        break;
                    case SpaceItemsAlignment.Center:
                        v = accumulatedV + (lineV - layoutSlotV) / 2;
                        break;
                    case SpaceItemsAlignment.End:
                        v = accumulatedV + lineV - layoutSlotV;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(ItemsAlignment), itemsAlign, null);
                }
                newChild.Arrange(isHorizontal ? new(u + frameInset.Left, v + frameInset.Top, layoutSlotU, layoutSlotV)
                                              : new(v + frameInset.Left, u + frameInset.Top, layoutSlotV, layoutSlotU));
                u += layoutSlotU;
                first = false;
            }

            return;

            double GetChildU(int i)
            {
                var c = children[i] as Control;
                Debug.Assert(c != null);
                return useItemU ? itemU :
                    isHorizontal ? c.DesiredSize.Width : c.DesiredSize.Height;
            }

            double GetChildV(int i)
            {
                var c = children[i] as Control;
                Debug.Assert(c != null);
                return useItemV ? itemV :
                    isHorizontal ? c.DesiredSize.Height : c.DesiredSize.Width;
            }
        }
    }

    #region 实现 IChildIndexProvider 接口

    event EventHandler<ChildIndexChangedEventArgs>? IChildIndexProvider.ChildIndexChanged
    {
        add
        {
            if (_childIndexChanged is null)
            {
                Children.PropertyChanged -= HandleChildrenPropertyChanged;
                Children.PropertyChanged += HandleChildrenPropertyChanged;
            }
            _childIndexChanged += value;
        }

        remove
        {
            _childIndexChanged -= value;
            if (_childIndexChanged is null)
            {
                Children.PropertyChanged -= HandleChildrenPropertyChanged;
            }
        }
    }

    int IChildIndexProvider.GetChildIndex(ILogical child)
    {
        return child is Control control ? Children.IndexOf(control) : -1;
    }

    bool IChildIndexProvider.TryGetTotalCount(out int count)
    {
        count = Children.Count;
        return true;
    }

    #endregion

    #region 实现 INavigableContainer 接口

    IInputElement? INavigableContainer.GetControl(NavigationDirection direction, IInputElement? from, bool wrap)
    {
        var  orientation = Orientation;
        var  children    = Children;
        bool horiz       = orientation == Orientation.Horizontal;
        int  index       = from is not null ? Children.IndexOf((Control)from) : -1;

        switch (direction)
        {
            case NavigationDirection.First:
                index = 0;
                break;
            case NavigationDirection.Last:
                index = children.Count - 1;
                break;
            case NavigationDirection.Next:
                ++index;
                break;
            case NavigationDirection.Previous:
                --index;
                break;
            case NavigationDirection.Left:
                index = horiz ? index - 1 : -1;
                break;
            case NavigationDirection.Right:
                index = horiz ? index + 1 : -1;
                break;
            case NavigationDirection.Up:
                index = horiz ? -1 : index - 1;
                break;
            case NavigationDirection.Down:
                index = horiz ? -1 : index + 1;
                break;
        }

        if (index >= 0 && index < children.Count)
        {
            return children[index];
        }
        return null;
    }

    #endregion

    private void HandleChildrenPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Children.Count) || e.PropertyName is null)
        {
            _childIndexChanged?.Invoke(this, ChildIndexChangedEventArgs.TotalCountChanged);
        }
    }

    private struct UVSize
    {
        internal UVSize(Orientation orientation, double width, double height)
        {
            U            = V = 0d;
            _orientation = orientation;
            Width        = width;
            Height       = height;
        }

        internal UVSize(Orientation orientation)
        {
            U            = V = 0d;
            _orientation = orientation;
        }

        internal double U;
        internal double V;
        private Orientation _orientation;

        internal double Width
        {
            get => _orientation == Orientation.Horizontal ? U : V;
            set { if (_orientation == Orientation.Horizontal)
                {
                    U = value;
                }
                else
                {
                    V = value;
                }
            }
        }
        internal double Height
        {
            get => _orientation == Orientation.Horizontal ? V : U;
            set { if (_orientation == Orientation.Horizontal)
                {
                    V = value;
                }
                else
                {
                    U = value;
                }
            }
        }
    }
}
