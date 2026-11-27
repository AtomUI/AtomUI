using Avalonia;

namespace AtomUI.Controls;

/// <summary>
/// A menu element that decides whether the popups below it scroll their items.
/// </summary>
public interface IScrollAwareControl
{
    /// <summary>
    /// <see langword="true" /> hosts the items of a menu popup inside a scroll viewer, so that a popup taller than
    /// its maximum height can be scrolled. <see langword="false" /> hosts the items without a scroll viewer. The
    /// default is <see langword="true" />.
    /// </summary>
    bool IsScrollEnabled { get; set; }
}

/// <summary>
/// Registers the <see cref="IsScrollEnabledProperty" /> property that menu elements share.
/// </summary>
public abstract class ScrollAwareControlProperty
{
    /// <summary>
    /// The registered name of <see cref="IsScrollEnabledProperty" />.
    /// </summary>
    public const string IsScrollEnabledPropertyName = "IsScrollEnabled";

    /// <summary>
    /// Whether the items of a menu popup are hosted inside a scroll viewer, shared by the menu elements. A menu
    /// element adds itself as owner with <c>AddOwner</c> and exposes the value through an <c>IsScrollEnabled</c>
    /// property of its own. The value inherits, so the menu items and popups below an element follow the value that
    /// is set on it.
    /// </summary>
    public static readonly StyledProperty<bool> IsScrollEnabledProperty =
        AvaloniaProperty.Register<StyledElement, bool>(
            IsScrollEnabledPropertyName,
            defaultValue: true,
            inherits: true);
}
