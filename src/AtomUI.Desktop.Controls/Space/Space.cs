using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Controls.Utils;
using AtomUI.Data;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Generated.AtomUIDesktopControls;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

public enum SpaceItemsAlignment
{
    /// <summary>
    /// Items are laid out so the first one in each column/row touches the top/left of the panel.
    /// </summary>
    Start,

    /// <summary>
    /// Items are laid out so that each column/row is centred vertically/horizontally within the panel.
    /// </summary>
    Center,

    /// <summary>
    /// Items are laid out so the last one in each column/row touches the bottom/right of the panel.
    /// </summary>
    End,
}

public partial class Space : Control,
                             IChildIndexProvider,
                             ICustomizableSizeTypeAware,
                             INavigableContainer
{
    #region 公共属性定义
    public static readonly StyledProperty<double> ItemSpacingProperty =
        AvaloniaProperty.Register<Space, double>(nameof(ItemSpacing));
    
    public static readonly StyledProperty<double> LineSpacingProperty =
        AvaloniaProperty.Register<Space, double>(nameof(LineSpacing));
    
    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<Space, Orientation>(nameof(Orientation), defaultValue: Orientation.Horizontal);
    
    public static readonly StyledProperty<SpaceItemsAlignment> ItemsAlignmentProperty =
        AvaloniaProperty.Register<Space, SpaceItemsAlignment>(nameof(ItemsAlignment), defaultValue: SpaceItemsAlignment.Start);
    
    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<Space>();
    
    public static readonly StyledProperty<double> ItemWidthProperty =
        AvaloniaProperty.Register<Space, double>(nameof(ItemWidth), double.NaN);
    
    public static readonly StyledProperty<double> ItemHeightProperty =
        AvaloniaProperty.Register<Space, double>(nameof(ItemHeight), double.NaN);
    
    public static readonly StyledProperty<ITemplate<Control>?> SplitTemplateProperty =
        AvaloniaProperty.Register<Space, ITemplate<Control>?>(
            nameof(SplitTemplate));

    public static readonly StyledProperty<IBrush?> BackgroundProperty =
        AvaloniaProperty.Register<Space, IBrush?>(nameof(Background));

    public static readonly StyledProperty<IBrush?> BorderBrushProperty =
        AvaloniaProperty.Register<Space, IBrush?>(nameof(BorderBrush));

    public static readonly StyledProperty<Thickness> BorderThicknessProperty =
        AvaloniaProperty.Register<Space, Thickness>(nameof(BorderThickness));

    public static readonly StyledProperty<CornerRadius> CornerRadiusProperty =
        AvaloniaProperty.Register<Space, CornerRadius>(nameof(CornerRadius));

    public static readonly StyledProperty<Thickness> PaddingProperty =
        AvaloniaProperty.Register<Space, Thickness>(nameof(Padding));

    public static readonly StyledProperty<IReadOnlyList<double>?> BorderDashArrayProperty =
        AvaloniaProperty.Register<Space, IReadOnlyList<double>?>(nameof(BorderDashArray));

    public static readonly StyledProperty<double> BorderDashOffsetProperty =
        AvaloniaProperty.Register<Space, double>(nameof(BorderDashOffset));
    
    public CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }
    
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
    
    public ITemplate<Control>? SplitTemplate
    {
        get => GetValue(SplitTemplateProperty);
        set => SetValue(SplitTemplateProperty, value);
    }

    public IBrush? Background
    {
        get => GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    public IBrush? BorderBrush
    {
        get => GetValue(BorderBrushProperty);
        set => SetValue(BorderBrushProperty, value);
    }

    public Thickness BorderThickness
    {
        get => GetValue(BorderThicknessProperty);
        set => SetValue(BorderThicknessProperty, value);
    }

    public CornerRadius CornerRadius
    {
        get => GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public Thickness Padding
    {
        get => GetValue(PaddingProperty);
        set => SetValue(PaddingProperty, value);
    }

    public IReadOnlyList<double>? BorderDashArray
    {
        get => GetValue(BorderDashArrayProperty);
        set => SetValue(BorderDashArrayProperty, value);
    }

    public double BorderDashOffset
    {
        get => GetValue(BorderDashOffsetProperty);
        set => SetValue(BorderDashOffsetProperty, value);
    }
    
    [Content]
    public AvaloniaList<Control> Children { get; } = new();
    
    public bool IsItemsHost { get; internal set; }

    #endregion

    private EventHandler<ChildIndexChangedEventArgs>? _childIndexChanged;
    private CompositeDisposable? _spacingBindings;
    private readonly HashSet<Control> _semanticItemMarkerOwners = new();
    private BorderRenderHelper? _borderRenderHelper;

    static Space()
    {
        AffectsMeasure<Space>(ItemSpacingProperty, 
            LineSpacingProperty,
            OrientationProperty, 
            ItemWidthProperty,
            ItemHeightProperty,
            SizeTypeProperty,
            BorderThicknessProperty,
            PaddingProperty);
        AffectsArrange<Space>(ItemsAlignmentProperty);
        AffectsRender<Space>(BackgroundProperty,
            BorderBrushProperty,
            BorderThicknessProperty,
            CornerRadiusProperty,
            BorderDashArrayProperty,
            BorderDashOffsetProperty);
        CustomizableSizeTypeControlProperty.SizeTypeProperty.OverrideDefaultValue<Space>(CustomizableSizeType.Small);
    }
    
    public Space()
    {
        Children.CollectionChanged += HandleChildrenChanged;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        ApplySpacingTokenBinding();

        if (SplitTemplate != null)
        {
            HandleSplitTemplateChanged();
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _spacingBindings?.Dispose();
        _spacingBindings = null;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == SizeTypeProperty)
        {
            ApplySpacingTokenBinding();
        }
        else if (this.IsAttachedToVisualTree())
        {
            if (change.Property == SplitTemplateProperty)
            {
                HandleSplitTemplateChanged();
            }
        }
    }

    protected virtual void HandleChildrenChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        ConfigureSemanticItemMarkers(e);

        if (SplitTemplate == null)
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
                        EnsureSemanticItemMarker(child);
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
        else
        {
            HandleSplitTemplateChanged();
        }
    }

    private protected virtual void InvalidateMeasureOnChildrenChanged()
    {
        InvalidateMeasure();
    }
    
    public sealed override void Render(DrawingContext context)
    {
        var borderRenderHelper = _borderRenderHelper ??= new BorderRenderHelper();
        borderRenderHelper.Render(
            context,
            Bounds.Size,
            BorderThickness,
            CornerRadius,
            BackgroundSizing.InnerBorderEdge,
            Background,
            BorderBrush,
            BorderDashArray,
            BorderDashOffset);
    }

    protected override Size MeasureOverride(Size constraint)
    {
        double itemWidth     = ItemWidth;
        double itemHeight    = ItemHeight;
        double itemSpacing   = ItemSpacing;
        double lineSpacing   = LineSpacing;
        var    orientation   = Orientation;
        var    children      = VisualChildren;
        var    frameInset    = Padding + BorderThickness;
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
            if (MathUtils.GreaterThan(curLineSize.U + childSize.U + nextSpacing, uvConstraint.U)) // Need to switch to another line
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
        var    orientation   = Orientation;
        bool   isHorizontal  = orientation == Orientation.Horizontal;
        var    children      = VisualChildren;
        var    frameInset    = Padding + BorderThickness;
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
            if (MathUtils.GreaterThan(curLineSize.U + childSize.U + nextSpacing, uvFinalSize.U)) // Need to switch to another line
            {
                accumulatedV += lineExists ? lineSpacing : 0; // add spacing to arrange line first
                ArrangeLine(curLineSize.V, firstInLine, i);
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
            ArrangeLine(curLineSize.V, firstInLine, children.Count);
        }

        return finalSize;

        void ArrangeLine(double lineV, int start, int end)
        {
            bool   useItemU = isHorizontal ? itemWidthSet : itemHeightSet;
            bool   useItemV = isHorizontal ? itemHeightSet : itemWidthSet;
            double v        = accumulatedV;
            double u        = 0;
            bool   first    = true;
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
                    u += itemSpacing;
                }
                if (ItemsAlignment != SpaceItemsAlignment.Start)
                {
                    v = ItemsAlignment switch
                    {
                        SpaceItemsAlignment.Center => (lineV - layoutSlotV) / 2,
                        SpaceItemsAlignment.End => lineV - layoutSlotV,
                        SpaceItemsAlignment.Start => accumulatedV,
                        _ => throw new ArgumentOutOfRangeException(nameof(ItemsAlignment), ItemsAlignment, null),
                    };
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

    private void ApplySpacingTokenBinding()
    {
        _spacingBindings?.Dispose();
        _spacingBindings = null;
        if (!this.IsAttachedToVisualTree())
        {
            return;
        }

        var tokenKind = SizeType switch
        {
            CustomizableSizeType.Small  => SpaceTokenKind.GapSmallSize,
            CustomizableSizeType.Middle => SpaceTokenKind.GapMiddleSize,
            CustomizableSizeType.Large  => SpaceTokenKind.GapLargeSize,
            _                           => SpaceTokenKind.GapSmallSize
        };
        _spacingBindings = new CompositeDisposable
        {
            TokenResourceBinder.CreateControlTokenBinding(this, ItemSpacingProperty, tokenKind),
            TokenResourceBinder.CreateControlTokenBinding(this, LineSpacingProperty, tokenKind)
        };
    }

    private void HandleSplitTemplateChanged()
    {
        LogicalChildren.Clear();
        VisualChildren.Clear();
        for (var i = 0; i < Children.Count; ++i)
        {
            var child = Children[i];
            EnsureSemanticItemMarker(child);
            if (!IsItemsHost)
            {
                LogicalChildren.Add(child);
            }
            VisualChildren.Add(child);
            if (SplitTemplate != null)
            {
                if (i != Children.Count - 1)
                {
                    var split = CreateSemanticSeparator();
                    if (!IsItemsHost)
                    {
                        LogicalChildren.Add(split);
                    }
                    VisualChildren.Add(split);
                }
            }
        }
    }

    private void ConfigureSemanticItemMarkers(NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                foreach (Control child in e.NewItems!)
                {
                    EnsureSemanticItemMarker(child);
                }
                break;

            case NotifyCollectionChangedAction.Remove:
                foreach (Control child in e.OldItems!)
                {
                    ReleaseSemanticItemMarker(child);
                }
                break;

            case NotifyCollectionChangedAction.Replace:
                foreach (Control child in e.OldItems!)
                {
                    ReleaseSemanticItemMarker(child);
                }

                foreach (Control child in e.NewItems!)
                {
                    EnsureSemanticItemMarker(child);
                }
                break;

            case NotifyCollectionChangedAction.Move:
                break;

            case NotifyCollectionChangedAction.Reset:
                ReleaseStaleSemanticItemMarkers();
                break;
        }
    }

    private void EnsureSemanticItemMarker(Control child)
    {
        if (child.Classes.Contains(SpaceSemanticParts.ItemClass))
        {
            return;
        }

        child.Classes.Add(SpaceSemanticParts.ItemClass);
        _semanticItemMarkerOwners.Add(child);
    }

    private void ReleaseSemanticItemMarker(Control child)
    {
        if (_semanticItemMarkerOwners.Remove(child))
        {
            child.Classes.Remove(SpaceSemanticParts.ItemClass);
        }
    }

    private void ReleaseStaleSemanticItemMarkers()
    {
        foreach (var child in _semanticItemMarkerOwners.ToArray())
        {
            if (!Children.Contains(child))
            {
                ReleaseSemanticItemMarker(child);
            }
        }
    }

    private Control CreateSemanticSeparator()
    {
        Debug.Assert(SplitTemplate != null);
        var separator = SplitTemplate.Build();
        if (!separator.Classes.Contains(SpaceSemanticParts.SeparatorClass))
        {
            separator.Classes.Add(SpaceSemanticParts.SeparatorClass);
        }

        return separator;
    }

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
