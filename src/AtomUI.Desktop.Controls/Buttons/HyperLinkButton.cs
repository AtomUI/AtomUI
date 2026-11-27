using AtomUI.Controls;
using AtomUI.Controls.Commons;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// A hyperlink rendered as a text button. The states and the navigation are defined by
/// <see cref="AbstractHyperLinkButton" />; this class only adds the binding that keeps the loading icon aligned
/// with the button icon.
/// </summary>
public class HyperLinkButton : AbstractHyperLinkButton
{
    private IDisposable? _loadingIconMarginBinding;

    /// <summary>
    /// Called when the template is applied. It runs the base member and then binds the margin of
    /// <c>PART_LoadingIcon</c> to the margin of <c>PART_ButtonIcon</c>, so that the loading icon keeps the offset of
    /// the icon it stands in for. The binding has local-value priority and overrides a margin that a style sets on
    /// the loading icon. The binding made by the previous template application is disposed first, and no binding is
    /// made when either part is missing. Derived overrides must call the base member to keep this binding.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _loadingIconMarginBinding?.Dispose();
        _loadingIconMarginBinding = null;

        var buttonIcon  = e.NameScope.Find("PART_ButtonIcon") as IconPresenter;
        var loadingIcon = e.NameScope.Find("PART_LoadingIcon") as Icon;
        if (buttonIcon is not null && loadingIcon is not null)
        {
            _loadingIconMarginBinding = loadingIcon.Bind(
                Layoutable.MarginProperty,
                buttonIcon.GetObservable(Layoutable.MarginProperty),
                BindingPriority.LocalValue);
        }
    }
}
