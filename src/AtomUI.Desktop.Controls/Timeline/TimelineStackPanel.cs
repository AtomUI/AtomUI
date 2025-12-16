using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;

namespace AtomUI.Desktop.Controls;

internal class TimelineStackPanel : StackPanel
{
    #region 公共属性定义

    public static readonly StyledProperty<bool> IsReverseProperty =
        AvaloniaProperty.Register<TimelineStackPanel, bool>(nameof(IsReverse));

    public bool IsReverse
    {
        get => GetValue(IsReverseProperty);
        set => SetValue(IsReverseProperty, value);
    }

    #endregion

    static TimelineStackPanel()
    {
        AffectsArrange<TimelineStackPanel>(IsReverseProperty);
    }

    public TimelineStackPanel()
    {
        Orientation = Orientation.Vertical;
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        Size   stackDesiredSize = new Size();
        var    children         = Children;
        Size   layoutSlotSize   = availableSize;
        bool   fHorizontal      = (Orientation == Orientation.Horizontal);
        double spacing          = Spacing;
        bool   hasVisibleChild  = false;
    
        //
        // Initialize child sizing and iterator data
        // Allow children as much size as they want along the stack.
        //
        if (fHorizontal)
        {
            layoutSlotSize = layoutSlotSize.WithWidth(Double.PositiveInfinity);
        }
        else
        {
            layoutSlotSize = layoutSlotSize.WithHeight(Double.PositiveInfinity);
        }
    
        //
        //  Iterate through children.
        //  While we still supported virtualization, this was hidden in a child iterator (see source history).
        //
        var idx = IsReverse ? children.Count - 1 : 0;
        var end = IsReverse ? 0 : children.Count - 1;
        while (IsReverse ? idx >= end : idx <= end)
        {
            var  child     = children[idx];
            bool isVisible = child.IsVisible;
    
            if (isVisible && !hasVisibleChild)
            {
                hasVisibleChild = true;
            }
    
            // Measure the child.
            child.Measure(layoutSlotSize);
            Size childDesiredSize = child.DesiredSize;
    
            // Accumulate child size.
            if (fHorizontal)
            {
                stackDesiredSize =
                    stackDesiredSize.WithWidth(stackDesiredSize.Width + (isVisible ? spacing : 0) +
                                               childDesiredSize.Width);
                stackDesiredSize =
                    stackDesiredSize.WithHeight(Math.Max(stackDesiredSize.Height, childDesiredSize.Height));
            }
            else
            {
                stackDesiredSize = stackDesiredSize.WithWidth(Math.Max(stackDesiredSize.Width, childDesiredSize.Width));
                stackDesiredSize =
                    stackDesiredSize.WithHeight(stackDesiredSize.Height + (isVisible ? spacing : 0) +
                                                childDesiredSize.Height);
            }
    
            if (IsReverse)
            {
                idx--;
            }
            else
            {
                idx++;
            }
        }
    
        if (fHorizontal)
        {
            stackDesiredSize = stackDesiredSize.WithWidth(stackDesiredSize.Width - (hasVisibleChild ? spacing : 0));
        }
        else
        {
            stackDesiredSize = stackDesiredSize.WithHeight(stackDesiredSize.Height - (hasVisibleChild ? spacing : 0));
        }
    
        return stackDesiredSize;
    }
    
    protected override Size ArrangeOverride(Size finalSize)
    {
        var    children          = Children;
        bool   fHorizontal       = (Orientation == Orientation.Horizontal);
        Rect   rcChild           = new Rect(finalSize);
        double previousChildSize = 0.0;
        var    spacing           = Spacing;
        
        int idx = IsReverse ? children.Count - 1 : 0;
        int end = IsReverse ? 0 : children.Count - 1;
        while (IsReverse ? idx >= end : idx <= end)
        {
            var child = children[idx];
            if (IsReverse)
            {
                idx--;
            }
            else
            {
                idx++;
            }
            if (!child.IsVisible)
            {
                continue;
            }
    
            if (fHorizontal)
            {
                rcChild           =  rcChild.WithX(rcChild.X + previousChildSize);
                previousChildSize =  child.DesiredSize.Width;
                rcChild           =  rcChild.WithWidth(previousChildSize);
                rcChild           =  rcChild.WithHeight(Math.Max(finalSize.Height, child.DesiredSize.Height));
                previousChildSize += spacing;
            }
            else
            {
                rcChild           =  rcChild.WithY(rcChild.Y + previousChildSize);
                previousChildSize =  child.DesiredSize.Height;
                rcChild           =  rcChild.WithHeight(previousChildSize);
                rcChild           =  rcChild.WithWidth(Math.Max(finalSize.Width, child.DesiredSize.Width));
                previousChildSize += spacing;
            }
            
            child.Arrange(rcChild);
        }
    
        RaiseEvent(new RoutedEventArgs(Orientation == Orientation.Horizontal
            ? HorizontalSnapPointsChangedEvent
            : VerticalSnapPointsChangedEvent));
    
        return finalSize;
    }
}