using AtomUI.Theme.Resources;
using Avalonia.Controls;
using Avalonia.Input;

namespace AtomUI.Desktop.Controls;

internal class AutoCompleteSearchEditBox : SearchEdit, IControlThemeGlobalOwnerSource
{
    protected override Type StyleKeyOverride => typeof(SearchEdit);

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

    #region 实现 IControlThemeGlobalOwnerSource 接口

    // The template parent owns this private input; AXAML cannot parameterize the reused theme keys.
    Control? IControlThemeGlobalOwnerSource.ThemeGlobalOwner => TemplatedParent as Control;

    #endregion
}
