using Avalonia;

namespace AtomUI.Controls;

/// <summary>
/// A control that publishes the responsive breakpoint of its own width, so that descendant controls can resolve
/// their responsive values from it.
/// </summary>
/// <remarks>
/// A descendant takes the breakpoint from the nearest control in its visual ancestry that implements this
/// interface. Such a control stores a new breakpoint in <see cref="MediaBreakPoint" /> before it raises
/// <see cref="MediaBreakPointChanged" />.
/// </remarks>
public interface IMediaBreakAwareControl
{
    /// <summary>
    /// The container name that the breakpoint queries of the theme target. The queries test the width of the
    /// container, so a control that relies on them sets <c>Container.Name</c> to this value on the element whose
    /// width decides the breakpoint.
    /// </summary>
    public const string GlobalQueryContainerName = "PART_GlobalQueryContainer";
    /// <summary>
    /// Gets the breakpoint that the width of the control currently resolves to.
    /// </summary>
    MediaBreakPoint MediaBreakPoint { get; }
    /// <summary>
    /// Occurs after <see cref="MediaBreakPoint" /> has changed. The arguments carry the new breakpoint.
    /// </summary>
    /// <remarks>
    /// A control may also raise the event once for its initial breakpoint, so a subscriber cannot count on
    /// receiving an event for the value that is current when it attaches.
    /// </remarks>
    event EventHandler<MediaBreakPointChangedEventArgs>? MediaBreakPointChanged;
}

/// <summary>
/// The shared declaration of the breakpoint property of a control that implements
/// <see cref="IMediaBreakAwareControl" />. Such a control adds <see cref="MediaBreakPointProperty" /> to itself
/// and wraps it in its own <c>MediaBreakPoint</c> property.
/// </summary>
public abstract class MediaBreakAwareControlProperty
{
    /// <summary>
    /// The name that <see cref="MediaBreakPointProperty" /> is registered under, which every control that adds the
    /// property shares.
    /// </summary>
    public const string MediaBreakPointPropertyName = "MediaBreakPoint";
    
    /// <summary>
    /// The property that stores the breakpoint of a control that implements <see cref="IMediaBreakAwareControl" />.
    /// </summary>
    /// <remarks>
    /// The default value is <see cref="MediaBreakPoint.Large" />. The value is not inherited: each control holds
    /// its own value, and a descendant reads the value of the nearest control that implements the interface.
    /// </remarks>
    public static readonly StyledProperty<MediaBreakPoint> MediaBreakPointProperty = 
        AvaloniaProperty.Register<StyledElement, MediaBreakPoint>(MediaBreakPointPropertyName, MediaBreakPoint.Large);
}