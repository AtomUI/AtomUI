using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace AtomUI.Controls;

/// <summary>
/// 启动 Splash。在内核提交之前显示，只依赖全局 Token 与系统字体；布局完全由 ControlTheme 决定。
/// 可选模板部件 PART_CloseButton：错误态下点击即请求关闭。
/// </summary>
[TemplatePart(CloseButtonPart, typeof(Control))]
[PseudoClasses(ErrorPseudoClass, OpaquePseudoClass)]
public class Splash : TemplatedControl
{
    internal const string CloseButtonPart = "PART_CloseButton";
    private const string ErrorPseudoClass = ":error";
    private const string OpaquePseudoClass = ":opaque";

    public static readonly StyledProperty<IImage?> LogoProperty =
        AvaloniaProperty.Register<Splash, IImage?>(nameof(Logo));

    public static readonly StyledProperty<IImage?> BackgroundImageProperty =
        AvaloniaProperty.Register<Splash, IImage?>(nameof(BackgroundImage));

    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<Splash, string?>(nameof(Title));

    public static readonly StyledProperty<string?> MessageProperty =
        AvaloniaProperty.Register<Splash, string?>(nameof(Message));

    public static readonly StyledProperty<double> ProgressProperty =
        AvaloniaProperty.Register<Splash, double>(nameof(Progress), coerce: static (_, value) => Math.Clamp(value, 0d, 1d));

    public static readonly StyledProperty<bool> IsIndeterminateProperty =
        AvaloniaProperty.Register<Splash, bool>(nameof(IsIndeterminate), true);

    public static readonly StyledProperty<SplashStatus> StatusProperty =
        AvaloniaProperty.Register<Splash, SplashStatus>(nameof(Status));

    public static readonly RoutedEvent<RoutedEventArgs> CloseRequestedEvent =
        RoutedEvent.Register<Splash, RoutedEventArgs>(nameof(CloseRequested), RoutingStrategies.Bubble);

    private Control? _closeButton;

    public IImage? Logo
    {
        get => GetValue(LogoProperty);
        set => SetValue(LogoProperty, value);
    }

    public IImage? BackgroundImage
    {
        get => GetValue(BackgroundImageProperty);
        set => SetValue(BackgroundImageProperty, value);
    }

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string? Message
    {
        get => GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public double Progress
    {
        get => GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, value);
    }

    public bool IsIndeterminate
    {
        get => GetValue(IsIndeterminateProperty);
        set => SetValue(IsIndeterminateProperty, value);
    }

    public SplashStatus Status
    {
        get => GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    public event EventHandler<RoutedEventArgs>? CloseRequested
    {
        add => AddHandler(CloseRequestedEvent, value);
        remove => RemoveHandler(CloseRequestedEvent, value);
    }

    internal void SetOpaque(bool opaque) => PseudoClasses.Set(OpaquePseudoClass, opaque);

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
            // 关闭按钮只能是基础元素（提交失败后没有任何主题），因此用指针事件而不是 Button.Click。
            _closeButton.PointerReleased += HandleClosePointerReleased;
        }
    }

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
