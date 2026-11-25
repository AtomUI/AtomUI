using AtomUI.Controls.Commons;
using AtomUI.Data;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Data;

namespace AtomUI.Desktop.Controls;

public class MarqueeLabel : AbstractMarqueeLabel
{
    private OwnerTokenPropertyBindings? _tokenBindings;

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

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _tokenBindings?.Dispose();
        _tokenBindings = null;
        base.OnDetachedFromVisualTree(e);
    }
}
