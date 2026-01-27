using Avalonia.Input;

namespace AtomUI.Desktop.Controls;

internal class AutoCompleteLineEditBox : LineEdit
{
    protected override Type StyleKeyOverride => typeof(LineEdit);
    
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        e.Handled = false;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        e.Handled = false;
    }
}