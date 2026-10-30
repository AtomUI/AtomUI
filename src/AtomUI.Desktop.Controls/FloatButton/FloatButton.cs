using AtomUI.Controls.Commons;
using Avalonia;

namespace AtomUI.Desktop.Controls;

using ToolTipControl = AtomUI.Desktop.Controls.ToolTip;

public partial class FloatButton : AbstractFloatButton
{
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == TooltipProperty)
        {
            ToolTipControl.SetTip(this, Tooltip);
        }
        else if (change.Property == TooltipColorProperty)
        {
            ToolTipControl.SetColor(this, TooltipColor);
        }
    }
    
    private protected override void ConfigureBadge()
    {
        if (_badgeLayout != null)
        {
            if (IsBadgeEnabled)
            {
                if (IsDotBadge)
                {
                    var dotBadge = new DotBadgeAdorner();
                    dotBadge[!DotBadgeAdorner.BadgeDotColorProperty]   = this[!BadgeEffectiveColorProperty];
                    dotBadge[!DotBadgeAdorner.IsMotionEnabledProperty] = this[!IsMotionEnabledProperty];
                    _badge                                             = dotBadge;
                }
                else
                {
                    var countBadge = new CountBadgeAdorner();
                    countBadge[!CountBadgeAdorner.BadgeColorProperty]      = this[!BadgeEffectiveColorProperty];
                    countBadge[!CountBadgeAdorner.IsMotionEnabledProperty] = this[!IsMotionEnabledProperty];
                    countBadge[!CountBadgeAdorner.CountProperty]           = this[!BadgeCountProperty];
                    countBadge[!CountBadgeAdorner.OverflowCountProperty]   = this[!BadgeOverflowCountProperty];
                    _badge                                                 = countBadge;
                }
                
                _badgeLayout.Children.Add(_badge);
                CalculateBadgePosition();
            }
            else
            {
                if (_badge != null)
                {
                    _badgeLayout.Children.Remove(_badge);
                }
                _badge = null;
            }
        }
    }
}
