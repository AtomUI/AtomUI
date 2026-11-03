using System.Reactive.Disposables;
using AtomUI.Data;
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
        RetireBadge();
        if (_badgeLayout is null || !IsBadgeEnabled)
        {
            return;
        }
        // Runtime badge Content is outside this ControlTheme's template scope.
        // Keep the original Local preparation priority with membership-owned
        // typed relay handles, retired on mode/template/tree changes.
        if (IsDotBadge)
        {
            var badge = new DotBadgeAdorner();
            _badgeBindings = new CompositeDisposable
            {
                BindUtils.RelayBind(this, BadgeEffectiveColorProperty, badge, DotBadgeAdorner.BadgeDotColorProperty),
                BindUtils.RelayBind(this, IsMotionEnabledProperty, badge, DotBadgeAdorner.IsMotionEnabledProperty)
            };
            _badge = badge;
        }
        else
        {
            var badge = new CountBadgeAdorner();
            _badgeBindings = new CompositeDisposable
            {
                BindUtils.RelayBind(this, BadgeEffectiveColorProperty, badge, CountBadgeAdorner.BadgeColorProperty),
                BindUtils.RelayBind(this, IsMotionEnabledProperty, badge, CountBadgeAdorner.IsMotionEnabledProperty),
                BindUtils.RelayBind(this, BadgeCountProperty, badge, CountBadgeAdorner.CountProperty),
                BindUtils.RelayBind(this, BadgeOverflowCountProperty, badge, CountBadgeAdorner.OverflowCountProperty)
            };
            _badge = badge;
        }
        _badgeLayout.Children.Add(_badge);
        CalculateBadgePosition();
    }
}
