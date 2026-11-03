using AtomUI.Theme.Resources;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

internal static class TourThemeOwner
{
    internal static Control? Resolve(Control control)
    {
        for (Control? current = control.TemplatedParent as Control ?? control.Parent as Control;
             current is not null;
             current = current.TemplatedParent as Control ?? current.Parent as Control)
        {
            if (current is Tour owner)
            {
                return owner;
            }
            if (current is IControlThemeGlobalOwnerSource { ThemeGlobalOwner: { } actualOwner })
            {
                return actualOwner;
            }
        }
        return null;
    }
}
