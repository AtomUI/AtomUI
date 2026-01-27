using AtomUI.Icons.AntDesign;

namespace AtomUI.Desktop.Controls;

internal class InputClearIconButton : IconButton
{
    protected override void OnInitialized()
    {
        base.OnInitialized();
        if (Icon == null)
        {
            SetCurrentValue(IconProperty, new CloseCircleFilled());
        }
    }
}