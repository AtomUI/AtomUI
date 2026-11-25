using AtomUI.Controls.Commons;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.LogicalTree;

namespace AtomUI.Desktop.Controls;

using ToolTipControl = AtomUI.Desktop.Controls.ToolTip;

internal static class BackTopRingResources
{
    public static BackTopProgressRingTokenResourceKeys ProgressRingTokenResourceKeys { get; } = new(
        ControlTokenResourceKey.Global(BackTopFloatButtonTokens.Identity, SharedTokenKind.LineWidthBold),
        ControlTokenResourceKey.Global(BackTopFloatButtonTokens.Identity, SharedTokenKind.ColorBorderSecondary),
        ControlTokenResourceKey.Global(BackTopFloatButtonTokens.Identity, SharedTokenKind.ColorPrimary));
}

public partial class BackTopFloatButton : AbstractBackTopFloatButton
{
    private FloatButtonAppearanceBinding? _themeInputs;

    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        _themeInputs = new FloatButtonAppearanceBinding(this);
    }

    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        _themeInputs?.Dispose();
        _themeInputs = null;
        base.OnDetachedFromLogicalTree(e);
    }

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
