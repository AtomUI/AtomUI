using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;

namespace AtomUI.Desktop.Controls;

internal class SelectMaxTagAwarePanel : StackPanel
{
    private SelectRemainInfoTag? _infoTag;
    private SelectFilterTextBox? _searchTextBox;
    
    private int _remainCount = 0;

    protected override Size MeasureOverride(Size availableSize)
    {
        var size = base.MeasureOverride(availableSize);
        _remainCount = 0;
        var effectiveWidth = 0d;
        if (Children.Count > 0)
        {
            // 计算需要隐藏几个
            var i            = 0;
            var tagInfoWidth = _infoTag?.DesiredSize.Width ?? 0d;
            var offsetX      = tagInfoWidth;
            for (; i < Children.Count; i++)
            {
                var child = Children[i];
                if (child != _infoTag)
                {
                    offsetX += child.DesiredSize.Width + Spacing;
                }
        
                if (offsetX > availableSize.Width)
                {
                    break;
                }
                effectiveWidth += child.DesiredSize.Width + Spacing;
            }

            if (offsetX > availableSize.Width)
            {
                _remainCount = Math.Max(0, Children.Count - 2 - i);
                if (_remainCount > 0)
                {
                    _infoTag?.SetRemainText(_remainCount); 
                }

                effectiveWidth += tagInfoWidth;
            }
        }
      
        return size.WithWidth(effectiveWidth);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        if (Children.Count == 2)
        {
            _searchTextBox?.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));
        }
        else
        {
            var      children          = Children;
            bool     fHorizontal       = (Orientation == Orientation.Horizontal);
            Rect     rcChild           = new Rect(finalSize);
            double   previousChildSize = 0.0;
            var      spacing           = Spacing;
            Control? latestChild       = null;

            for (int i = 0, count = children.Count; i < count; ++i)
            {
                var child = children[i];

                if (!child.IsVisible)
                {
                    continue;
                }
                if (child is SelectRemainInfoTag)
                {
                    continue;
                }

                if (_remainCount > 0)
                {
                    if (i > count - _remainCount - 3)
                    {
                        child.Arrange(new Rect(int.MaxValue, 0, child.DesiredSize.Width, child.DesiredSize.Height));
                        continue;
                    }
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
                latestChild = child;
            }
        
            if (_remainCount > 0 )
            {
                if (_infoTag != null)
                {
                    if (latestChild != null) {
                        _infoTag.Arrange(new Rect(latestChild.Bounds.Right + Spacing, latestChild.Bounds.Y, _infoTag.DesiredSize.Width, _infoTag.DesiredSize.Height));
                    }
                    else
                    {
                        _infoTag.Arrange(new Rect(0, 0, _infoTag.DesiredSize.Width, _infoTag.DesiredSize.Height));
                    }
                    var offsetX              =  _infoTag.Bounds.Right + Spacing;
                    var searchBoxBoundsWidth = Math.Max(0, finalSize.Width - offsetX);
                    _searchTextBox?.Arrange(new Rect(offsetX, _infoTag.Bounds.Y, searchBoxBoundsWidth, finalSize.Height));
                }
            }
            else
            {
                _infoTag?.Arrange(new Rect(int.MaxValue, 0, _infoTag.DesiredSize.Width, _infoTag.DesiredSize.Height));
                if (latestChild != null)
                {
                    var offsetX              =  latestChild.Bounds.Right - Spacing;
                    var searchBoxBoundsWidth = Math.Max(0, finalSize.Width - offsetX);
                    _searchTextBox?.Arrange(new Rect(offsetX, latestChild.Bounds.Y, searchBoxBoundsWidth, finalSize.Height));
                }
            }
        
            RaiseEvent(new RoutedEventArgs(Orientation == Orientation.Horizontal ? HorizontalSnapPointsChangedEvent : VerticalSnapPointsChangedEvent));
        }
        
        return finalSize;
    }

    protected override void ChildrenChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        base.ChildrenChanged(sender, e);
        if (e.OldItems != null)
        {
            foreach (var oldItem in e.OldItems)
            {
                if (oldItem is SelectRemainInfoTag)
                {
                    _infoTag = null;
                }
                if (oldItem is SelectFilterTextBox)
                {
                    _searchTextBox = null;
                }
            }
        }
        
        if (e.NewItems != null)
        {
            foreach (var newItem in e.NewItems)
            {
                if (newItem is SelectRemainInfoTag selectTag)
                {
                    _infoTag = selectTag;
                    _infoTag.SetRemainText(0);
                }
                if (newItem is SelectFilterTextBox searchTextBox)
                {
                    _searchTextBox = searchTextBox;
                }
            }
        }
    }
}