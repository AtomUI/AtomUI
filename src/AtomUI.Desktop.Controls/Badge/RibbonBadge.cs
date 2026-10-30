using AtomUI.Controls.Commons;
using AtomUI.Generated.AtomUIDesktopControls;

namespace AtomUI.Desktop.Controls;

public partial class RibbonBadge : AbstractRibbonBadge
{
    public RibbonBadge()
    {
    }
    
    private protected override AbstractRibbonBadgeAdorner CreateBadgeAdorner()
    {
        if (_ribbonBadgeAdorner is null)
        {
            _ribbonBadgeAdorner = new RibbonBadgeAdorner();
            _ribbonBadgeAdorner.Classes.Add(RibbonBadgeSemanticParts.IndicatorClass);
            SetupTokenBindings();
            if (RibbonColor is not null)
            {
                SetupRibbonColor(RibbonColor);
            }
        }

        return _ribbonBadgeAdorner;
    }
}
