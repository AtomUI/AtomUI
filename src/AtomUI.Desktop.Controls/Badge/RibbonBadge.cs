using AtomUI.Controls.Commons;
using AtomUI.Generated.AtomUIDesktopControls;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// The desktop ribbon badge. It adds to <see cref="AbstractRibbonBadge" /> the adorner that paints the ribbon.
/// </summary>
public partial class RibbonBadge : AbstractRibbonBadge
{
    public RibbonBadge()
    {
    }
    
    /// <summary>
    /// Returns the ribbon indicator of the badge. The <see cref="RibbonBadgeAdorner" /> is created on the first
    /// call; it then gets the class that <see cref="RibbonBadgeSemanticParts.IndicatorClass" /> names and is set up
    /// for the current token bindings and ribbon color of the badge. Later calls return that same instance.
    /// </summary>
    private protected override AbstractRibbonBadgeAdorner CreateBadgeAdorner()
    {
        if (_ribbonBadgeAdorner is null)
        {
            _ribbonBadgeAdorner = new RibbonBadgeAdorner();
            _ribbonBadgeAdorner.Classes.Add(RibbonBadgeSemanticParts.IndicatorClass);
            SetupTokenBindings();
            ConfigureRibbonColor();
        }

        return _ribbonBadgeAdorner;
    }
}
