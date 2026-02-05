// This source file is adapted from the Windows Presentation Foundation project. 
// (https://github.com/dotnet/wpf/) 
// 
// Licensed to The Avalonia Project under MIT License, courtesy of The .NET Foundation.

using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using AtomUI.Controls;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme;
using AtomUI.Theme.Styling;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Metadata;
using Avalonia.Styling;
using Avalonia.Utilities;
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

public class Space : Control,
                     IChildIndexProvider, 
                     IControlSharedTokenResourcesHost, 
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
    
    [Content]
    public AvaloniaList<Control> Children { get; } = new();
    
    public bool IsItemsHost { get; internal set; }
    
    event EventHandler<ChildIndexChangedEventArgs>? IChildIndexProvider.ChildIndexChanged
    {
        add
        {
            if (_childIndexChanged is null)
            {
                Children.PropertyChanged += ChildrenPropertyChanged;
            }
            _childIndexChanged += value;
        }

        remove
        {
            _childIndexChanged -= value;
            if (_childIndexChanged is null)
            {
                Children.PropertyChanged -= ChildrenPropertyChanged;
            }
        }
    }
    
    #endregion
    
    #region 内部属性定义
    private EventHandler<ChildIndexChangedEventArgs>? _childIndexChanged;
    
    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => SpaceToken.ID;
    #endregion
    
    static Space()
    {
        AffectsMeasure<Space>(ItemSpacingProperty, 
            LineSpacingProperty,
            OrientationProperty, 
            ItemWidthProperty,
            ItemHeightProperty,
            SizeTypeProperty);
        AffectsArrange<Space>(ItemsAlignmentProperty);
        CustomizableSizeTypeControlProperty.SizeTypeProperty.OverrideDefaultValue<Space>(CustomizableSizeType.Small);
    }
    
    public Space()
    {
        this.RegisterResources();
        ConfigureInstanceStyle();
        Children.CollectionChanged += ChildrenChanged;
    }

    private void ConfigureInstanceStyle()
    {
        {
            var middleStyle = new Style(x =>
                x.PropertyEquals(SizeTypeProperty, CustomizableSizeType.Middle));
            middleStyle.Add(ItemSpacingProperty, SpaceTokenKey.GapMiddleSize);
            middleStyle.Add(LineSpacingProperty, SpaceTokenKey.GapMiddleSize);
            Styles.Add(middleStyle);
        }
        {
            var smallStyle = new Style(x =>
                x.PropertyEquals(SizeTypeProperty, CustomizableSizeType.Small));
            smallStyle.Add(ItemSpacingProperty, SpaceTokenKey.GapSmallSize);
            smallStyle.Add(LineSpacingProperty, SpaceTokenKey.GapSmallSize);
            Styles.Add(smallStyle);
        }
        {
            var largeStyle = new Style(x =>
                x.PropertyEquals(SizeTypeProperty, CustomizableSizeType.Large));
            largeStyle.Add(ItemSpacingProperty, SpaceTokenKey.GapLargeSize);
            largeStyle.Add(LineSpacingProperty, SpaceTokenKey.GapLargeSize);
            Styles.Add(largeStyle);
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (this.IsAttachedToVisualTree())
        {
            if (change.Property == SplitTemplateProperty)
            {
                HandleSplitTemplateChanged();
            }
        }
    }

    protected virtual void ChildrenChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (SplitTemplate == null)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (!IsItemsHost)
                    {
                        LogicalChildren.InsertRange(e.NewStartingIndex, e.NewItems!.OfType<Control>().ToList());
                    }
                    VisualChildren.InsertRange(e.NewStartingIndex, e.NewItems!.OfType<Visual>());
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
                        LogicalChildren.RemoveAll(e.OldItems!.OfType<Control>().ToList());
                    }
                    VisualChildren.RemoveAll(e.OldItems!.OfType<Visual>());
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
        else
        {
            HandleSplitTemplateChanged();
        }
    }

    private void HandleSplitTemplateChanged()
    {
        LogicalChildren.Clear();
        VisualChildren.Clear();
        for (var i = 0; i < Children.Count; ++i)
        {
            var child = Children[i];
            if (!IsItemsHost)
            {
                LogicalChildren.Add(child);
            }
            VisualChildren.Add(child);
            if (SplitTemplate != null)
            {
                if (i != Children.Count - 1)
                {
                    var split = SplitTemplate.Build();
                    if (!IsItemsHost)
                    {
                        LogicalChildren.Add(split);
                    }
                    VisualChildren.Add(split);
                }
            }
        }
    }

    private protected virtual void InvalidateMeasureOnChildrenChanged()
    {
        InvalidateMeasure();
    }
    
    private void ChildrenPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Children.Count) || e.PropertyName is null)
        {
            _childIndexChanged?.Invoke(this, ChildIndexChangedEventArgs.TotalCountChanged);
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
    
    protected override Size MeasureOverride(Size constraint)
    {
        double itemWidth     = ItemWidth;
        double itemHeight    = ItemHeight;
        double itemSpacing   = ItemSpacing;
        double lineSpacing   = LineSpacing;
        var    orientation   = Orientation;
        var    children      = VisualChildren;
        var    curLineSize   = new UVSize(orientation);
        var    panelSize     = new UVSize(orientation);
        var    uvConstraint  = new UVSize(orientation, constraint.Width, constraint.Height);
        bool   itemWidthSet  = !double.IsNaN(itemWidth);
        bool   itemHeightSet = !double.IsNaN(itemHeight);
        bool   itemExists    = false;
        bool   lineExists    = false;

        var childConstraint = new Size(
            itemWidthSet ? itemWidth : constraint.Width,
            itemHeightSet ? itemHeight : constraint.Height);

        for (int i = 0, count = children.Count; i < count; ++i)
        {
            var child = children[i] as Control;
            Debug.Assert(child != null);
            // Flow passes its own constraint to children
            child.Measure(childConstraint);

            // This is the size of the child in UV space
            UVSize childSize = new UVSize(orientation,
                itemWidthSet ? itemWidth : child.DesiredSize.Width,
                itemHeightSet ? itemHeight : child.DesiredSize.Height);

            var nextSpacing = itemExists && child.IsVisible ? itemSpacing : 0;
            if (MathUtilities.GreaterThan(curLineSize.U + childSize.U + nextSpacing, uvConstraint.U)) // Need to switch to another line
            {
                panelSize.U =  Math.Max(curLineSize.U, panelSize.U);
                panelSize.V += curLineSize.V + (lineExists ? lineSpacing : 0);
                curLineSize =  childSize;

                itemExists = child.IsVisible;
                lineExists = true;
            }
            else // Continue to accumulate a line
            {
                curLineSize.U += childSize.U + nextSpacing;
                curLineSize.V =  Math.Max(childSize.V, curLineSize.V);
                    
                itemExists |= child.IsVisible; // keep true
            }
        }

        // The last line size, if any should be added
        panelSize.U =  Math.Max(curLineSize.U, panelSize.U);
        panelSize.V += curLineSize.V + (lineExists ? lineSpacing : 0);

        // Go from UV space to W/H space
        return new Size(panelSize.Width, panelSize.Height);
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
        int    firstInLine   = 0;
        double accumulatedV  = 0;
        double itemU         = isHorizontal ? itemWidth : itemHeight;
        double itemV         = isHorizontal ? itemHeight : itemWidth;
        var    curLineSize   = new UVSize(orientation);
        var    uvFinalSize   = new UVSize(orientation, finalSize.Width, finalSize.Height);
        bool   itemWidthSet  = !double.IsNaN(itemWidth);
        bool   itemHeightSet = !double.IsNaN(itemHeight);
        bool   itemExists    = false;
        bool   lineExists    = false;

        for (int i = 0; i < children.Count; ++i)
        {
            var child = children[i] as Control;
            Debug.Assert(child != null);
            var childSize = new UVSize(orientation,
                itemWidthSet ? itemWidth : child.DesiredSize.Width,
                itemHeightSet ? itemHeight : child.DesiredSize.Height);

            var nextSpacing = itemExists && child.IsVisible ? itemSpacing : 0;
            if (MathUtilities.GreaterThan(curLineSize.U + childSize.U + nextSpacing, uvFinalSize.U)) // Need to switch to another line
            {
                accumulatedV += lineExists ? lineSpacing : 0; // add spacing to arrange line first
                ArrangeLine(curLineSize.V, firstInLine, i);
                accumulatedV += curLineSize.V; // add the height of the line just arranged
                curLineSize  =  childSize;

                firstInLine = i;

                itemExists = child.IsVisible;
                lineExists = true;
            }
            else // Continue to accumulate a line
            {
                curLineSize.U += childSize.U + nextSpacing;
                curLineSize.V =  Math.Max(childSize.V, curLineSize.V);

                itemExists |= child.IsVisible; // keep true
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
            for (int i = start; i < end; ++i)
            {
                var    newChild    = children[i] as Control;
                Debug.Assert(newChild != null);
                double layoutSlotV = GetChildV(i);
                double layoutSlotU = GetChildU(i);
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
                newChild.Arrange(isHorizontal ? new(u, v, layoutSlotU, layoutSlotV) : new(v, u, layoutSlotV, layoutSlotU));
                u += layoutSlotU + (!children[i].IsVisible ? 0 : itemSpacing);
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
            set { if (_orientation == Orientation.Horizontal) U = value; else V = value; }
        }
        internal double Height
        {
            get => _orientation == Orientation.Horizontal ? V : U;
            set { if (_orientation == Orientation.Horizontal) V = value; else U = value; }
        }
    }
}