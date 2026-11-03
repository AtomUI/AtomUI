using AtomUI.Theme.Resources;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

// Private template assets retain the public entry's Global identity in both hosts.
internal static class ImagePreviewThemeOwner
{
    internal static Control? Resolve(Control control)
    {
        for (Control? current = control.TemplatedParent as Control ?? control.Parent as Control;
             current is not null;
             current = current.TemplatedParent as Control ?? current.Parent as Control)
        {
            if (current is AbstractImagePreviewer owner)
            {
                return owner;
            }
            if (current is IControlThemeGlobalOwnerSource { ThemeGlobalOwner: { } hostOwner })
            {
                return hostOwner;
            }
        }
        return null;
    }
}
