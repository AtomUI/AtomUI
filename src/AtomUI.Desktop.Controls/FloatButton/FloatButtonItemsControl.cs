using System.Collections.Specialized;
using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Metadata;

namespace AtomUI.Desktop.Controls;

using ControlList = Avalonia.Controls.Controls;

internal class FloatButtonItemsControl : TemplatedControl
{
    #region 公共属性定义
    
    public static readonly StyledProperty<FloatButtonShape> ShapeProperty =
        FloatButton.ShapeProperty.AddOwner<FloatButtonItemsControl>();
    
    public static readonly StyledProperty<BoxShadows> BoxShadowProperty =
        Border.BoxShadowProperty.AddOwner<FloatButtonItemsControl>();
    
    public static readonly StyledProperty<Orientation> OrientationProperty =
        StackPanel.OrientationProperty.AddOwner<FloatButtonItemsControl>();
    
    public static readonly StyledProperty<FloatButtonGroupMenuPlacement> MenuPlacementProperty =
        FloatButtonGroup.MenuPlacementProperty.AddOwner<FloatButtonItemsControl>();

    public static readonly StyledProperty<bool> IsTriggerModeProperty =
        AvaloniaProperty.Register<FloatButtonItemsControl, bool>(nameof(IsTriggerMode));
    
    public FloatButtonShape Shape
    {
        get => GetValue(ShapeProperty);
        set => SetValue(ShapeProperty, value);
    }
    
    public BoxShadows BoxShadow
    {
        get => GetValue(BoxShadowProperty);
        set => SetValue(BoxShadowProperty, value);
    }
    
    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }
    
    public FloatButtonGroupMenuPlacement MenuPlacement
    {
        get => GetValue(MenuPlacementProperty);
        set => SetValue(MenuPlacementProperty, value);
    }
    
    public bool IsTriggerMode
    {
        get => GetValue(IsTriggerModeProperty);
        set => SetValue(IsTriggerModeProperty, value);
    }
    
    [Content] 
    public ControlList Children { get; } = new ();
 
    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<IList<(Point, Point)>?> LinesProperty =
        AvaloniaProperty.Register<FloatButtonItemsControl, IList<(Point, Point)>?>(nameof(Lines));
    
    internal IList<(Point, Point)>? Lines
    {
        get => GetValue(LinesProperty);
        set => SetValue(LinesProperty, value);
    }

    #endregion
    
    private StackPanel? _itemsLayout;

    static FloatButtonItemsControl()
    {
        AffectsMeasure<FloatButtonItemsControl>(ShapeProperty, OrientationProperty);
    }
    
    public FloatButtonItemsControl()
    {
        Children.CollectionChanged += NotifyChildrenChanged;
    }
    
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        DetachItemsLayoutChildren();
        _itemsLayout = e.NameScope.Find<StackPanel>("PART_ItemsLayout");
        _itemsLayout?.Children.AddRange(Children);
    }

    private void DetachItemsLayoutChildren()
    {
        if (_itemsLayout is { Children.Count: > 0 } itemsLayout)
        {
            itemsLayout.Children.RemoveRange(0, itemsLayout.Children.Count);
        }
    }
    
    protected virtual void NotifyChildrenChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_itemsLayout != null)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    ControlCollectionChangedUtils.InsertControls(_itemsLayout.Children, e.NewStartingIndex, e.NewItems!);
                    break;

                case NotifyCollectionChangedAction.Move:
                    _itemsLayout.Children.MoveRange(e.OldStartingIndex, e.OldItems!.Count, e.NewStartingIndex);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    _itemsLayout.Children.RemoveRange(e.OldStartingIndex, e.OldItems!.Count);
                    break;

                case NotifyCollectionChangedAction.Replace:
                    for (var i = 0; i < e.OldItems!.Count; ++i)
                    {
                        var index = i + e.OldStartingIndex;
                        var child = (Control)e.NewItems![i]!;
                        _itemsLayout.Children[index] = child;
                    }
                    break;

                case NotifyCollectionChangedAction.Reset:
                    throw new NotSupportedException();
            }
        }
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var size     = base.ArrangeOverride(finalSize);
        if (Shape == FloatButtonShape.Square)
        {
            var count    = CountVisibleChildren();
            if (count <= 1)
            {
                Lines = Array.Empty<(Point, Point)>();
                return size;
            }

            var lines        = new (Point, Point)[count - 1];
            var lineIndex    = 0;
            var visibleIndex = 0;
            Point startPoint = default;
            Point endPoint   = default;
            for (var i = 0; i < Children.Count; ++i)
            {
                var child = Children[i];
                if (!child.IsVisible)
                {
                    continue;
                }

                if (visibleIndex != count - 1)
                {
                    var childPos = child.TranslatePoint(new Point(0, 0), this);
                    if (childPos != null)
                    {
                        var childBounds = child.Bounds;
                        if (Orientation == Orientation.Horizontal)
                        {
                            startPoint = new Point(childPos.Value.X + childBounds.Width, 0);
                            endPoint   = new Point(childPos.Value.X + childBounds.Width, DesiredSize.Height);
                        }
                        else
                        {
                            startPoint = new Point(0, childPos.Value.Y + childBounds.Height);
                            endPoint   = new Point(DesiredSize.Width, childPos.Value.Y + childBounds.Height);
                        }
                        lines[lineIndex++] = (startPoint, endPoint);
                    }
                }

                visibleIndex++;
            }

            if (lineIndex != lines.Length)
            {
                Array.Resize(ref lines, lineIndex);
            }
            Lines = lines;
        }
        return size;
    }

    private int CountVisibleChildren()
    {
        var count = 0;
        foreach (var child in Children)
        {
            if (child.IsVisible)
            {
                count++;
            }
        }

        return count;
    }
}
