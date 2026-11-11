using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls;

// Keyboard focus stays in TreeSelect (or its filter input); navigation keys are forwarded to the
// popup tree, which owns the active item and the commit policy.
public partial class TreeSelect
{
    private static void ConfigureKeyboardNavigation()
    {
        // Filter inputs consume navigation keys themselves, so keys from a TextBox are routed on tunnel.
        KeyDownEvent.AddClassHandler<TreeSelect>((treeSelect, e) =>
        {
            if (e.Source is TextBox)
            {
                treeSelect.HandleKeyDownCore(e);
            }
        }, RoutingStrategies.Tunnel);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Source is not TextBox)
        {
            HandleKeyDownCore(e);
        }
    }

    private void HandleKeyDownCore(KeyEventArgs e)
    {
        if (e.Handled || !IsEnabled || CascaderView.IsComposingKeyInput(e.Source))
        {
            return;
        }

        var isAlt = e.KeyModifiers.HasFlag(KeyModifiers.Alt);
        if (e.Key == Key.F4 || (isAlt && e.Key is Key.Down or Key.Up))
        {
            SetCurrentValue(IsDropDownOpenProperty, !IsDropDownOpen);
            e.Handled = true;
            return;
        }

        if (!IsDropDownOpen)
        {
            if (e.KeyModifiers == KeyModifiers.None &&
                (e.Key is Key.Down or Key.Up or Key.Enter || (e.Key == Key.Space && !IsEditableInput(e.Source))))
            {
                SetCurrentValue(IsDropDownOpenProperty, true);
                e.Handled = true;
            }
            return;
        }

        if (e.Key == Key.Escape)
        {
            SetCurrentValue(IsDropDownOpenProperty, false);
            e.Handled = true;
            return;
        }

        if (_treeView is null || !ShouldForwardKeyToTreeView(e))
        {
            return;
        }

        _treeView.HandleKeyDown(e);
        if (e.Handled && !IsMultiple && e.Key is Key.Enter or Key.Space && _treeView.SelectedItem is not null)
        {
            SetCurrentValue(IsDropDownOpenProperty, false);
        }
    }

    private static bool ShouldForwardKeyToTreeView(KeyEventArgs e)
    {
        return e.Key switch
        {
            Key.Up or Key.Down or Key.Enter => true,
            // Typing a space or moving the caret in a non-empty search text belongs to the input.
            Key.Space => !IsEditableInput(e.Source),
            Key.Left or Key.Right or Key.Home or Key.End => !(IsEditableInput(e.Source) &&
                                                                !string.IsNullOrEmpty(((TextBox)e.Source!).Text)),
            _ => false
        };
    }

    private static bool IsEditableInput(object? source)
    {
        return source is TextBox { IsReadOnly: false };
    }
}
