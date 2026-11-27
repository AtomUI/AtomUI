using AtomUI.Controls.Commons;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// The desktop count badge. It adds to <see cref="AbstractCountBadge" /> the adorner that paints the count
/// indicator.
/// </summary>
public partial class CountBadge : AbstractCountBadge
{
    public CountBadge()
    {
    }
    
    /// <summary>
    /// Returns the indicator of the count badge. The <see cref="CountBadgeAdorner" /> is created on the first call;
    /// it then gets the <c>semantic-scope-indicator</c> class and is set up for the current token bindings,
    /// decorated target and color of the badge. Later calls return that same instance.
    /// </summary>
    private protected override AbstractCountBadgeAdorner CreateBadgeAdorner()
    {
        if (_badgeAdorner is null)
        {
            _badgeAdorner = new CountBadgeAdorner();
            _badgeAdorner.Classes.Add("semantic-scope-indicator");
            SetupTokenBindings();
            NotifyDecoratedTargetChanged();
            ConfigureDotColor(BadgeColor);
        }

        return _badgeAdorner;
    }
}
