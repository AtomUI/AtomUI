using AtomUI.Controls.Commons;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// The desktop dot badge. It adds to <see cref="AbstractDotBadge" /> the adorner that paints the status dot.
/// </summary>
public partial class DotBadge : AbstractDotBadge
{
    public DotBadge()
    {
    }
    
    /// <summary>
    /// Returns the indicator of the dot badge. The <see cref="DotBadgeAdorner" /> is created on the first call; it
    /// then gets the <c>semantic-scope-indicator</c> class and is set up for the current token bindings, decorated
    /// target and color of the badge. Later calls return that same instance.
    /// </summary>
    private protected override AbstractDotBadgeAdorner CreateDotBadgeAdorner()
    {
        if (_dotBadgeAdorner is null)
        {
            _dotBadgeAdorner = new DotBadgeAdorner();
            _dotBadgeAdorner.Classes.Add("semantic-scope-indicator");
            SetupTokenBindings();
            NotifyDecoratedTargetChanged();
            ConfigureDotColor(DotColor);
        }

        return _dotBadgeAdorner;
    }
}
