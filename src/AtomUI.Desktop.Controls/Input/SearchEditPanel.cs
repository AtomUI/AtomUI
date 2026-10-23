using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

internal class SearchEditPanel : Panel
{
    private Control? _contentFrame;
    private Button? _searchButton;
    private Control? _leftAddOn;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        foreach (var child in Children)
        {
            if (child.Name == "PART_LeftAddOn")
            {
                _leftAddOn = child;
            }
            else if (child.Name == "PART_RightAddOn")
            {
                _searchButton = child as Button;
            }
            else if (child.Name == "PART_ContentFrame")
            {
                _contentFrame = child;
            }
        }
    }
    
    protected override Size MeasureOverride(Size availableSize)
    {
        var desiredSize = base.MeasureOverride(availableSize);
        var contentHeight = _contentFrame?.DesiredSize.Height ?? 0;
        var addOnHeight = _leftAddOn?.DesiredSize.Height ?? 0;
        var buttonMinHeight = _searchButton is { IsVisible: true } button
            ? button.MinHeight + button.Margin.Top + button.Margin.Bottom
            : 0;

        // Button.Height follows the arranged frame so its shape and width fit the input.
        // Feeding that previous height back into measure prevents the input from shrinking
        // after padding/font changes. Current content and minimums own the natural height.
        return new Size(desiredSize.Width, Math.Max(Math.Max(contentHeight, addOnHeight), buttonMinHeight));
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var availableWidth  = Math.Max(0, finalSize.Width);
        var rightAddOnWidth = Math.Min(_searchButton?.DesiredSize.Width ?? 0, availableWidth);
        var remainingWidth  = availableWidth - rightAddOnWidth;
        var leftAddOnWidth  = Math.Min(_leftAddOn?.DesiredSize.Width ?? 0, remainingWidth);
        var height          = finalSize.Height;

        if (_searchButton != null)
        {
            var offsetX = availableWidth - rightAddOnWidth;
            _searchButton.Arrange(new Rect(offsetX, 0, rightAddOnWidth, height));
        }

        if (_leftAddOn != null)
        {
            _leftAddOn.Arrange(new Rect(0, 0, leftAddOnWidth, height));
        }

        if (_contentFrame != null)
        {
            var sharedBorderOverlap = 0.0d;
            if (_searchButton != null)
            {
                sharedBorderOverlap = Math.Min(
                    BorderUtils.BuildRenderScaleAwareThickness(_searchButton, _searchButton.BorderThickness.Left),
                    rightAddOnWidth);
            }

            var contentWidth = Math.Max(0, remainingWidth - leftAddOnWidth + sharedBorderOverlap);
            _contentFrame.Arrange(new Rect(leftAddOnWidth, 0, contentWidth, height));
        }

        return finalSize;
    }
}
