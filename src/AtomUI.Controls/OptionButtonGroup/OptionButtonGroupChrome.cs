using AtomUI.Controls.Commons;
using Avalonia.Controls;
using Avalonia.Media;

namespace AtomUI.Controls;

/// <summary>
/// Template part that paints the outer frame, option separators and selected-option outline for an
/// <see cref="AbstractOptionButtonGroup" />. Omitting this part from a custom template lets a theme render its own
/// group chrome purely in XAML.
/// </summary>
public sealed class OptionButtonGroupChrome : Control
{
    /// <summary>
    /// Delegates chrome rendering to the owning <see cref="AbstractOptionButtonGroup" /> when hosted inside its
    /// control template.
    /// </summary>
    public override void Render(DrawingContext context)
    {
        if (TemplatedParent is AbstractOptionButtonGroup owner)
        {
            owner.RenderGroupChrome(context, this);
        }
    }
}
