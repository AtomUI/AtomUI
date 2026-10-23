using AtomUI.Theme.Resources;
using AtomUI.Controls;
using AtomUI.Controls.Commons;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;

namespace AtomUI.Desktop.Controls;

public class HyperLinkButton : AbstractHyperLinkButton,
    IThemeInheritanceSource
{
    private IDisposable? _loadingIconMarginBinding;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _loadingIconMarginBinding?.Dispose();
        _loadingIconMarginBinding = null;

        var buttonIcon  = e.NameScope.Find("PART_ButtonIcon") as IconPresenter;
        var loadingIcon = e.NameScope.Find("PART_LoadingIcon") as Icon;
        if (buttonIcon is not null && loadingIcon is not null)
        {
            // ControlTheme's compiled ElementName binding cannot resolve a name inside
            // the template from a style Setter (AVLN2000). Reflection name bindings
            // are not an AOT fallback. Mirror the actual sibling here at Template
            // priority so native Style triggers and local properties remain authoritative.
            // Both ends belong to this template; reapplication disposes the old binding.
            _loadingIconMarginBinding = loadingIcon.Bind(
                Layoutable.MarginProperty,
                buttonIcon.GetObservable(Layoutable.MarginProperty),
                BindingPriority.Template);
        }
    }
    // AXAML cannot read AvaloniaObject's protected, potentially non-logical parent.
    AvaloniaObject? IThemeInheritanceSource.ThemeInheritanceParent => InheritanceParent;


}
