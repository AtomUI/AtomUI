using Avalonia.Input;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

internal class MentionOptionListItem : ListItem
{
    internal MentionOptionList _ownerList;
    private bool _isPressed;

    public MentionOptionListItem(MentionOptionList ownerList)
    {
        _ownerList = ownerList;
    }
    
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            _isPressed = true;
            e.Handled  = true;
        }
    }
    
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        if (_isPressed && e.InitialPressMouseButton == MouseButton.Left)
        {
            _isPressed = false;
            e.Handled  = true;

            if (this.GetVisualsAt(e.GetPosition(this)).Any(c => this == c || this.IsVisualAncestorOf(c)))
            {
                _ownerList.NotifyItemClicked(this);
            }
        }
    }
}