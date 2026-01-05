using AtomUI.Desktop.Controls.Themes;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;

namespace AtomUI.Desktop.Controls;

internal class SearchEditPanel : Panel
{
    private Border? _contentFrame;
    private SearchButton? _searchButton;
    private ContentPresenter? _leftAddOn;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        foreach (var child in Children)
        {
            if (child.Name == AddOnDecoratedBoxThemeConstants.LeftAddOnPart)
            {
                _leftAddOn = child as ContentPresenter;
            }
            else if (child.Name == AddOnDecoratedBoxThemeConstants.RightAddOnPart)
            {
                _searchButton = child as SearchButton;
            }
            else if (child.Name == AddOnDecoratedBoxThemeConstants.ContentFramePart)
            {
                _contentFrame = child as Border;
            }
        }
    }
    
    protected override Size ArrangeOverride(Size finalSize)
    {
        var leftAddOnWidth  = 0.0d;
        var rightAddOnWidth = 0.0d;
        var height = finalSize.Height;
        if (_leftAddOn != null)
        {
            leftAddOnWidth = _leftAddOn.DesiredSize.Width;
            _leftAddOn.Arrange(new Rect(0, 0, leftAddOnWidth, height));
        }
        
        if (_searchButton != null)
        {
            var buttonWidth  = _searchButton.DesiredSize.Width;
            var offsetX      = finalSize.Width - buttonWidth;
            _searchButton.Arrange(new Rect(offsetX, 0, buttonWidth, height));
            rightAddOnWidth = buttonWidth;
        }

        if (_contentFrame != null)
        {
            var delta = 0.0d;
            if (_searchButton != null)
            {
                delta = _searchButton.BorderThickness.Left;
            }
            var width   =  finalSize.Width - rightAddOnWidth - leftAddOnWidth + delta;
            var offsetX = leftAddOnWidth;
            _contentFrame.Arrange(new Rect(offsetX, 0, width, height));
        }
        
        return finalSize;
    }
}