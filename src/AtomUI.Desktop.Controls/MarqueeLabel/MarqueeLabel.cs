using AtomUI.Controls.Commons;
using AtomUI.Data;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Data;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// A <see cref="AbstractMarqueeLabel" /> that binds its text gap and scrolling speed to the design tokens of
/// <see cref="MarqueeLabelToken" /> when it is attached, because it has no control theme that supplies them.
/// </summary>
public class MarqueeLabel : AbstractMarqueeLabel
{
    private OwnerTokenPropertyBindings? _tokenBindings;

    /// <summary>
    /// Binds <see cref="AbstractMarqueeLabel.CycleSpace" /> and <see cref="AbstractMarqueeLabel.MoveSpeed" /> to
    /// the design tokens of <see cref="MarqueeLabelToken" /> at template priority, so a value set directly on the
    /// label still wins. Calls the base member first.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _tokenBindings?.Dispose();
        // The standalone label draws its own text and has no ControlTheme template for these defaults.
        var tokens = new OwnerTokenPropertyBindings(this, 2, BindingPriority.Template);
        var ownerType = GetType();
        try
        {
            tokens.Bind(0, CycleSpaceProperty, ControlTokenResourceKey.Own(ownerType, MarqueeLabelTokenKind.CycleSpace));
            tokens.Bind(1, MoveSpeedProperty, ControlTokenResourceKey.Own(ownerType, MarqueeLabelTokenKind.DefaultSpeed));
            _tokenBindings = tokens;
        }
        catch
        {
            tokens.Dispose();
            throw;
        }
    }

    /// <summary>
    /// Disposes the token bindings created on attach before the base member stops the scrolling. Derived labels
    /// must call the base member, or the bindings outlive the attachment.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _tokenBindings?.Dispose();
        _tokenBindings = null;
        base.OnDetachedFromVisualTree(e);
    }
}
