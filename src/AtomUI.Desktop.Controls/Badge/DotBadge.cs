using AtomUI.Controls.Commons;

namespace AtomUI.Desktop.Controls;

public partial class DotBadge : AbstractDotBadge
{
    public DotBadge()
    {
    }
    
    private protected override AbstractDotBadgeAdorner CreateDotBadgeAdorner()
    {
        if (_dotBadgeAdorner is null)
        {
            _dotBadgeAdorner = new DotBadgeAdorner();
            _dotBadgeAdorner.Classes.Add("semantic-scope-indicator");
            SetupTokenBindings();
            NotifyDecoratedTargetChanged();
            if (DotColor is not null)
            {
                ConfigureDotColor(DotColor);
            }
        }

        return _dotBadgeAdorner;
    }
}
