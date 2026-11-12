using AtomUI.Icons.AntDesign;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

namespace AtomUI.Desktop.Controls;

// The accessible name (Ant Design locale.global.clear) is a ControlTheme setter bound to the language resource, so
// application styles and local values override it and language changes update it without code.
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

    /// <summary>
    /// Gets how the host should move focus back to its input after this button clears the value, or <c>null</c> when
    /// the button does not hold focus. The button hides once the value is gone, which would otherwise drop focus.
    /// </summary>
    internal NavigationMethod? GetFocusReturnMethod()
    {
        if (!IsFocused)
        {
            return null;
        }

        // Keyboard activation keeps the focus visual on the input that receives focus.
        return Classes.Contains(":focus-visible") ? NavigationMethod.Tab : NavigationMethod.Pointer;
    }
}
