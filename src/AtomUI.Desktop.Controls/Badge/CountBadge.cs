using AtomUI.Controls.Commons;

namespace AtomUI.Desktop.Controls;

public partial class CountBadge : AbstractCountBadge
{
    public CountBadge()
    {
    }
    
    private protected override AbstractCountBadgeAdorner CreateBadgeAdorner()
    {
        if (_badgeAdorner is null)
        {
            _badgeAdorner = new CountBadgeAdorner();
            _badgeAdorner.Classes.Add("semantic-scope-indicator");
            SetupTokenBindings();
            NotifyDecoratedTargetChanged();
            if (BadgeColor is not null)
            {
                ConfigureDotColor(BadgeColor);
            }
        }

        return _badgeAdorner;
    }
}
