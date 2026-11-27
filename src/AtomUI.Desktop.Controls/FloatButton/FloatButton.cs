using AtomUI.Controls.Commons;
using Avalonia;
using Avalonia.LogicalTree;

namespace AtomUI.Desktop.Controls;

using ToolTipControl = AtomUI.Desktop.Controls.ToolTip;

/// <summary>
/// A float button for desktop applications: an icon button that can carry a description, a badge and a tooltip. An
/// embedded button is laid out by its parent. <see cref="FloatButtonHost" /> creates its own button and floats it in
/// the scoped overlay layer. The description, set through <see cref="Avalonia.Controls.ContentControl" />, is shown
/// only in the Square shape.
/// </summary>
public partial class FloatButton : AbstractFloatButton
{
    private FloatButtonAppearanceBinding? _themeInputs;

    /// <summary>
    /// Called when the button is attached to a logical tree. It creates the binding that reads the outline tokens of
    /// the active theme and writes the resulting values to the attached properties of
    /// <c>FloatButtonAppearanceBinding</c>, which the theme of the button reads. Overrides must call the base
    /// implementation, otherwise the button ignores the outline look.
    /// </summary>
    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        _themeInputs = new FloatButtonAppearanceBinding(this);
    }

    /// <summary>
    /// Called when the button is detached from its logical tree. It disposes the binding created in
    /// <see cref="OnAttachedToLogicalTree" />, which stops the theme subscription and clears the attached values.
    /// Overrides must call the base implementation, otherwise the button stays subscribed to theme changes.
    /// </summary>
    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        _themeInputs?.Dispose();
        _themeInputs = null;
        base.OnDetachedFromLogicalTree(e);
    }

    /// <summary>
    /// Called when the value of a property of the button changes. After the base implementation, when <see
    /// cref="AbstractFloatButton.Tooltip" /> or <see cref="AbstractFloatButton.TooltipColor" /> changes, it writes the
    /// new value to the attached <c>ToolTip.Tip</c> or <c>ToolTip.Color</c> property of the button. Overrides must call
    /// the base implementation, otherwise the tooltip does not follow these properties.
    /// </summary>
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
    
    /// <summary>
    /// Creates the badge that matches <see cref="AbstractFloatButton.IsBadgeEnabled" /> and
    /// <see cref="AbstractFloatButton.IsDotBadge" />, adds it to <c>PART_BadgeLayout</c> and positions it. The badge
    /// is a <c>DotBadgeAdorner</c>, or a <c>CountBadgeAdorner</c> when the badge is not a dot, and it is bound to
    /// the badge properties of the button. When the badge is disabled, the existing badge is removed instead.
    /// Nothing happens before a template supplies <c>PART_BadgeLayout</c>.
    /// </summary>
    /// <remarks>
    /// The base class calls it after the control template is applied and whenever
    /// <see cref="AbstractFloatButton.IsBadgeEnabled" /> or <see cref="AbstractFloatButton.IsDotBadge" /> changes.
    /// The base class positions the control that <c>_badge</c> references, so an override must leave <c>_badge</c>
    /// pointing at the badge it added, or set it to <see langword="null" />. The member is abstract in the base
    /// class, so there is no base implementation to call.
    /// </remarks>
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
