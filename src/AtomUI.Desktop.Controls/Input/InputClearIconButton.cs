using AtomUI.Icons.AntDesign;
using Avalonia;
using Avalonia.Controls;

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

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IconProperty)
        {
            if (change.OldValue is PathIcon oldIcon)
            {
                oldIcon.Classes.Remove("skip-status");
            }

            if (change.NewValue is PathIcon newIcon)
            {
                newIcon.Classes.Add("skip-status");
            }
        }
    }
}