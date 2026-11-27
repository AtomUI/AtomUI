using AtomUI.Controls;
using AtomUI.Generated.AtomUIDesktopControls;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.LogicalTree;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// The box that presents the content of a <see cref="Flyout" /> inside the popup, styled by the shared
/// <c>ArrowDecoratedBox</c> theme.
/// </summary>
public class FlyoutPresenter : ArrowDecoratedBox
{
    /// <summary>
    /// Lets the presenter take the theme of <c>ArrowDecoratedBox</c> instead of a theme of its own, because it adds
    /// no elements or styles to that box.
    /// </summary>
    protected override Type StyleKeyOverride => typeof(ArrowDecoratedBox);

    public FlyoutPresenter()
    {
        SetValue(CursorProperty, new Cursor(StandardCursorType.Arrow), BindingPriority.Template);
    }

    /// <summary>
    /// Marks the content decorator, the content presenter and the arrow indicator of the applied template with the
    /// classes <c>PopupContainerClass</c>, <c>PopupContentClass</c> and <c>PopupArrowClass</c> of
    /// <c>FlyoutHostSemanticParts</c>, so that the styles of the flyout host semantic parts reach them. Overrides
    /// must call the base implementation first: it resolves the decorator and the arrow indicator.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        // The flyout popup is created in code and lives in the popup's own visual root, so the marker classes cannot
        // be declared statically in the shared ArrowDecoratedBox theme: that would also hit the other arrow-decorated
        // boxes. They are injected here instead, when the template is applied.
        e.NameScope.Get<Border>("PART_ContentDecorator")
                  .Classes.Add(FlyoutHostSemanticParts.PopupContainerClass);
        e.NameScope.Get<ContentPresenter>("ContentPresenter")
                  .Classes.Add(FlyoutHostSemanticParts.PopupContentClass);
        e.NameScope.Get<ArrowIndicator>("PART_ArrowIndicator")
                  .Classes.Add(FlyoutHostSemanticParts.PopupArrowClass);
    }

    /// <summary>
    /// Closes the popup that hosts the presenter when it receives the Escape key: sets <c>IsOpen</c> of the logical
    /// ancestor <c>Popup</c> to <see langword="false" /> and marks the key as handled. Without such an ancestor the
    /// key stays unhandled. The base implementation is called afterwards.
    /// </summary>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            var host = this.FindLogicalAncestorOfType<Popup>();
            if (host != null)
            {
                host.IsOpen = false;
                e.Handled   = true;
            }
        }

        base.OnKeyDown(e);
    }
}
