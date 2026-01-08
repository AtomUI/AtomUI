using System.Collections.Specialized;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

internal class SelectWrapPanel : WrapPanel
{
    private SelectFilterTextBox? _searchTextBox;
    
    protected override Size ArrangeOverride(Size finalSize)
    {
        var size = base.ArrangeOverride(finalSize);
        if (_searchTextBox != null)
        {
            if (Children.Count == 1)
            {
                _searchTextBox.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));    
            }
            else
            {
                // 找最后一个
                var indexOf    = Children.IndexOf(_searchTextBox);
                Debug.Assert(indexOf != -1);
                var lastIndex  = indexOf - 1;
                var lastTag    =  Children[lastIndex];
                if (!lastTag.IsVisible)
                {
                    lastTag    =  Children[lastIndex - 1];
                }
                var lastBounds = lastTag.Bounds;
                var offestX    = lastBounds.Right + ItemSpacing;
                var offsetY    = lastBounds.Top;
                var width      = finalSize.Width - offestX;
                
                if ((offestX + _searchTextBox.DesiredSize.Width) > finalSize.Width)
                {
                    offestX =  0;
                    offsetY += lastBounds.Height + LineSpacing;
                    width   =  finalSize.Width;
                }
                var targetSearchBoxBounds = new Rect(offestX, offsetY, width, _searchTextBox.DesiredSize.Height);
                _searchTextBox.Arrange(targetSearchBoxBounds);
            }
        }
   
        return size;
    }

    protected override void ChildrenChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        base.ChildrenChanged(sender, e);
        if (e.OldItems != null)
        {
            foreach (var oldItem in e.OldItems)
            {
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
                if (newItem is SelectFilterTextBox searchTextBox)
                {
                    _searchTextBox = searchTextBox;
                }
            }
        }
    }
}