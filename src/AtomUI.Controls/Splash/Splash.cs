using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace AtomUI.Controls;

/// <summary>
/// Content of the startup splash, shown by <see cref="SplashWindow" /> before the kernel is committed. It relies
/// only on global Tokens and system fonts, and its layout is defined entirely by its ControlTheme. The template
/// part <c>PART_CloseButton</c> is optional: in the error state, a click on it raises
/// <see cref="CloseRequested" />.
/// </summary>
[TemplatePart(CloseButtonPart, typeof(Control))]
[PseudoClasses(ErrorPseudoClass, OpaquePseudoClass)]
public class Splash : TemplatedControl
{
    internal const string CloseButtonPart = "PART_CloseButton";
    private const string ErrorPseudoClass = ":error";
    private const string OpaquePseudoClass = ":opaque";

    /// <summary>
    /// Identifies the <see cref="Logo" /> property.
    /// </summary>
    public static readonly StyledProperty<IImage?> LogoProperty =
        AvaloniaProperty.Register<Splash, IImage?>(nameof(Logo));

    /// <summary>
    /// Identifies the <see cref="BackgroundImage" /> property.
    /// </summary>
    public static readonly StyledProperty<IImage?> BackgroundImageProperty =
        AvaloniaProperty.Register<Splash, IImage?>(nameof(BackgroundImage));

    /// <summary>
    /// Identifies the <see cref="Title" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<Splash, string?>(nameof(Title));

    /// <summary>
    /// Identifies the <see cref="Message" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> MessageProperty =
        AvaloniaProperty.Register<Splash, string?>(nameof(Message));

    /// <summary>
    /// Identifies the <see cref="Progress" /> property.
    /// </summary>
    public static readonly StyledProperty<double> ProgressProperty =
        AvaloniaProperty.Register<Splash, double>(nameof(Progress), coerce: static (_, value) => Math.Clamp(value, 0d, 1d));

    /// <summary>
    /// Identifies the <see cref="IsIndeterminate" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsIndeterminateProperty =
        AvaloniaProperty.Register<Splash, bool>(nameof(IsIndeterminate), true);

    /// <summary>
    /// Identifies the <see cref="Status" /> property.
    /// </summary>
    public static readonly StyledProperty<SplashStatus> StatusProperty =
        AvaloniaProperty.Register<Splash, SplashStatus>(nameof(Status));

    /// <summary>
    /// Identifies the <see cref="CloseRequested" /> routed event.
    /// </summary>
    public static readonly RoutedEvent<RoutedEventArgs> CloseRequestedEvent =
        RoutedEvent.Register<Splash, RoutedEventArgs>(nameof(CloseRequested), RoutingStrategies.Bubble);

    private Control? _closeButton;

    /// <summary>
    /// Image shown above the title of the splash card; <see langword="null" /> leaves it out.
    /// </summary>
    public IImage? Logo
    {
        get => GetValue(LogoProperty);
        set => SetValue(LogoProperty, value);
    }

    /// <summary>
    /// Image painted behind the content of the splash card. It is scaled to fill the card and cropped to keep its
    /// aspect ratio; <see langword="null" />, the default, shows the card background instead.
    /// </summary>
    public IImage? BackgroundImage
    {
        get => GetValue(BackgroundImageProperty);
        set => SetValue(BackgroundImageProperty, value);
    }

    /// <summary>
    /// Heading shown below the logo of the splash card.
    /// </summary>
    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>
    /// Status text shown below the title. A progress report that carries a message replaces it with that message, and
    /// a startup failure replaces it with the error message.
    /// </summary>
    public string? Message
    {
        get => GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    /// <summary>
    /// Completed fraction of the startup progress bar, from 0 to 1; values outside that range are clamped to it. The
    /// bar shows it only while <see cref="IsIndeterminate" /> is false, and the default theme hides the bar in the
    /// <see cref="SplashStatus.Error" /> state.
    /// </summary>
    public double Progress
    {
        get => GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, value);
    }

    /// <summary>
    /// While true, the default, the progress bar loops and ignores <see cref="Progress" />; while false, the bar shows
    /// <see cref="Progress" />. The first progress report that carries a fraction sets it to false.
    /// </summary>
    public bool IsIndeterminate
    {
        get => GetValue(IsIndeterminateProperty);
        set => SetValue(IsIndeterminateProperty, value);
    }

    /// <summary>
    /// State of the startup. <see cref="SplashStatus.Error" /> switches the default theme to the failure presentation:
    /// the message is painted in the error color, the progress bar is hidden and the close button is shown, so the
    /// user can dismiss the splash.
    /// </summary>
    public SplashStatus Status
    {
        get => GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    /// <summary>
    /// Raised when the user asks to close the splash while <see cref="Status" /> is
    /// <see cref="SplashStatus.Error" />: when the pointer is released on the close button or when Escape is pressed in
    /// the splash window. The event bubbles up from the splash and cannot be canceled; the startup code handles it by
    /// closing the splash window.
    /// </summary>
    public event EventHandler<RoutedEventArgs>? CloseRequested
    {
        add => AddHandler(CloseRequestedEvent, value);
        remove => RemoveHandler(CloseRequestedEvent, value);
    }

    internal void SetOpaque(bool opaque) => PseudoClasses.Set(OpaquePseudoClass, opaque);

    /// <summary>
    /// Looks up the optional <c>PART_CloseButton</c> part and subscribes to its pointer released event. Overrides must
    /// call the base member.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (_closeButton is not null)
        {
            _closeButton.PointerReleased -= HandleClosePointerReleased;
        }
        _closeButton = e.NameScope.Find<Control>(CloseButtonPart);
        if (_closeButton is not null)
        {
            // The close button has to be a base element: after a failed kernel commit there is no theme at all. The
            // pointer release is handled here instead of Button.Click.
            _closeButton.PointerReleased += HandleClosePointerReleased;
        }
    }

    /// <summary>
    /// Sets the <c>:error</c> pseudo-class from <see cref="Status" />. Overrides must call the base member.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == StatusProperty)
        {
            PseudoClasses.Set(ErrorPseudoClass, Status == SplashStatus.Error);
        }
    }

    private void HandleClosePointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (Status == SplashStatus.Error)
        {
            RaiseEvent(new RoutedEventArgs(CloseRequestedEvent));
        }
    }
}
