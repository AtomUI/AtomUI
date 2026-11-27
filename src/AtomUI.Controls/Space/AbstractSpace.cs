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

    /// <summary>
    /// Identifies the <see cref="ItemSpacing" /> property.
    /// </summary>
    public static readonly StyledProperty<double> ItemSpacingProperty =
        AvaloniaProperty.Register<AbstractSpace, double>(nameof(ItemSpacing));

    /// <summary>
    /// Identifies the <see cref="LineSpacing" /> property.
    /// </summary>
    public static readonly StyledProperty<double> LineSpacingProperty =
        AvaloniaProperty.Register<AbstractSpace, double>(nameof(LineSpacing));

    /// <summary>
    /// Identifies the <see cref="Orientation" /> property.
    /// </summary>
    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<AbstractSpace, Orientation>(nameof(Orientation), defaultValue: Orientation.Horizontal);

    /// <summary>
    /// Identifies the <see cref="ItemsAlignment" /> property.
    /// </summary>
    public static readonly StyledProperty<SpaceItemsAlignment> ItemsAlignmentProperty =
        AvaloniaProperty.Register<AbstractSpace, SpaceItemsAlignment>(nameof(ItemsAlignment), defaultValue: SpaceItemsAlignment.Start);

    /// <summary>
    /// Identifies the <see cref="ItemWidth" /> property.
    /// </summary>
    public static readonly StyledProperty<double> ItemWidthProperty =
        AvaloniaProperty.Register<AbstractSpace, double>(nameof(ItemWidth), double.NaN);

    /// <summary>
    /// Identifies the <see cref="ItemHeight" /> property.
    /// </summary>
    public static readonly StyledProperty<double> ItemHeightProperty =
        AvaloniaProperty.Register<AbstractSpace, double>(nameof(ItemHeight), double.NaN);

    /// <summary>
    /// Identifies the <see cref="IsWrap" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsWrapProperty =
        AvaloniaProperty.Register<AbstractSpace, bool>(nameof(IsWrap), true);

    /// <summary>
    /// Identifies the <see cref="Justify" /> property.
    /// </summary>
    public static readonly StyledProperty<SpaceJustify> JustifyProperty =
        AvaloniaProperty.Register<AbstractSpace, SpaceJustify>(nameof(Justify), SpaceJustify.Start);

    /// <summary>
    /// Gap between two adjacent items of the same line, not added around invisible children. Extra gaps from
    /// <see cref="Justify" /> come on top of it. The default is 0.
    /// </summary>
    public double ItemSpacing
    {
        get => GetValue(ItemSpacingProperty);
        set => SetValue(ItemSpacingProperty, value);
    }

    /// <summary>
    /// Gap between two adjacent lines. It applies only when <see cref="IsWrap" /> is <c>true</c> and the content
    /// needs more than one line; nothing is added before the first line or after the last one.
    /// The default is 0.
    /// </summary>
    public double LineSpacing
    {
        get => GetValue(LineSpacingProperty);
        set => SetValue(LineSpacingProperty, value);
    }

    /// <summary>
    /// Axis along which the items are placed one after another. Lines wrap perpendicular to it: into rows for
    /// <c>Horizontal</c>, into columns for <c>Vertical</c>. The default is <c>Horizontal</c>.
    /// </summary>
    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    /// <summary>
    /// Placement of the items across their line when they differ in cross-axis size: at the start of the line,
    /// centered in it, or at its end. The default is <see cref="SpaceItemsAlignment.Start" />.
    /// </summary>
    public SpaceItemsAlignment ItemsAlignment
    {
        get => GetValue(ItemsAlignmentProperty);
        set => SetValue(ItemsAlignmentProperty, value);
    }

    /// <summary>
    /// Fixed width of every item, whatever the orientation. Each child is measured with this width and is
    /// arranged in a slot of this width. <see cref="double.NaN" /> lets each child keep the width it asked for.
    /// The default is <see cref="double.NaN" />.
    /// </summary>
    public double ItemWidth
    {
        get => GetValue(ItemWidthProperty);
        set => SetValue(ItemWidthProperty, value);
    }

    /// <summary>
    /// Fixed height of every item, whatever the orientation. Each child is measured with this height and is
    /// arranged in a slot of this height. <see cref="double.NaN" /> lets each child keep the height it asked
    /// for. The default is <see cref="double.NaN" />.
    /// </summary>
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

    /// <summary>
    /// The items this control lays out, in layout order. It is the XAML content property. Changing the list
    /// updates the layout, but clearing it throws <see cref="NotSupportedException" />, so remove the items one
    /// by one. Invisible children take no space.
    /// </summary>
    [Content]
    public AvaloniaList<Control> Children { get; } = new();

    /// <summary>
    /// Whether the items are only visual children of this control. When <c>true</c>, a change of
    /// <see cref="Children" /> is mirrored to the visual children only and the logical children are left alone,
    /// so that another control can own the items logically. The default is <c>false</c>.
    /// </summary>
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

    /// <summary>
    /// Mirrors a change of <see cref="Children" /> into the visual children and, unless
    /// <see cref="IsItemsHost" /> is <c>true</c>, into the logical children. It then reports
    /// <c>ChildIndexesReset</c> to <see cref="IChildIndexProvider.ChildIndexChanged" /> subscribers and
    /// invalidates the measure. The <see cref="Children" /> list calls it for every Add, Move, Remove or Replace
    /// change. An override must call the base implementation unless it rebuilds both child collections itself.
    /// </summary>
    /// <param name="sender">
    /// The <see cref="Children" /> list that changed.
    /// </param>
    /// <param name="e">
    /// The collection change. A <see cref="NotifyCollectionChangedAction.Reset" /> change is not supported.
    /// </param>
    /// <exception cref="NotSupportedException">
    /// Thrown when <paramref name="e" /> describes a <see cref="NotifyCollectionChangedAction.Reset" /> change.
    /// </exception>
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

    /// <summary>
    /// Invalidates the measure after <see cref="HandleChildrenChanged" /> mirrored a change of
    /// <see cref="Children" /> into the child collections and reported <c>ChildIndexesReset</c> to the
    /// subscribers of <see cref="IChildIndexProvider.ChildIndexChanged" />.
    /// </summary>
    /// <remarks>
    /// An override that does not call the base member must invalidate the measure itself.
    /// </remarks>
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

    /// <summary>
    /// Measures the children line by line and returns the size of the resulting lines. Invisible children are
    /// skipped.
    /// </summary>
    /// <param name="constraint">
    /// The available size. The layout inset returned by <see cref="GetLayoutInset" /> is deducted from it
    /// before the lines are broken.
    /// </param>
    /// <returns>
    /// The size of all lines, inflated by the layout inset. It is the natural size of the content and is not
    /// expanded to the available size; it exceeds <paramref name="constraint" /> when <see cref="IsWrap" /> is
    /// <c>false</c> and the items do not fit, or when a single item is longer than a line.
    /// </returns>
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
            // A fixed item size, when set, replaces the available size in the child constraint.
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

    /// <summary>
    /// Breaks the children into lines the way the measure pass does, but against <paramref name="finalSize" />,
    /// and places every line: <see cref="Justify" /> distributes the main-axis space left in the line and
    /// <see cref="ItemsAlignment" /> positions each item across the line. Invisible children are skipped.
    /// </summary>
    /// <param name="finalSize">
    /// The size assigned to this control. The layout inset is deducted from it, and the lines are placed inside
    /// what remains.
    /// </param>
    /// <returns>
    /// Always <paramref name="finalSize" />.
    /// </returns>
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
