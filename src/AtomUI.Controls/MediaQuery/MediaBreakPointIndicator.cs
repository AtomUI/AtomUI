using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Controls;

/// <summary>
/// Template part without visuals that reports the breakpoint of its host's width. Breakpoint styles select it by
/// <see cref="MediaQueryIndicatorName" /> and set <see cref="MediaBreakPoint" /> through container queries on the
/// enclosing query container, and the host follows the changes through <see cref="MediaBreakPointChanged" />.
/// </summary>
public class MediaBreakPointIndicator : Control
{
    /// <summary>
    /// Name of the indicator part in a host template. The breakpoint styles select the indicator by it, and the host
    /// finds the indicator by it in its name scope.
    /// </summary>
    public const string MediaQueryIndicatorName = "PART_MediaQueryIndicator";

    /// <summary>
    /// Identifies the <see cref="MediaBreakPoint" /> property.
    /// </summary>
    public static readonly StyledProperty<MediaBreakPoint> MediaBreakPointProperty =
        MediaBreakAwareControlProperty.MediaBreakPointProperty.AddOwner<MediaBreakPointIndicator>();

    /// <summary>
    /// Occurs after <see cref="MediaBreakPoint" /> changed; the arguments carry the new breakpoint. It is raised only
    /// for changes, not for the value that is current when a subscriber attaches.
    /// </summary>
    public event EventHandler<MediaBreakPointChangedEventArgs>? MediaBreakPointChanged;

    /// <summary>
    /// The breakpoint that the container queries resolved for the enclosing query container. The default is
    /// <see cref="MediaBreakPoint.Large" /> until the breakpoint styles set a value.
    /// </summary>
    public MediaBreakPoint MediaBreakPoint
    {
        get => GetValue(MediaBreakPointProperty);
        set => SetValue(MediaBreakPointProperty, value);
    }

    /// <summary>
    /// Calls the base member, then raises <see cref="MediaBreakPointChanged" /> after
    /// <see cref="MediaBreakPoint" /> changed.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == MediaBreakPointProperty)
        {
            MediaBreakPointChanged?.Invoke(this, new MediaBreakPointChangedEventArgs(MediaBreakPoint));
        }
    }
}
