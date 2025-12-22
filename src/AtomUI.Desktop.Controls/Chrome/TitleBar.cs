using System.Reactive.Disposables;
using AtomUI.Animations;
using AtomUI.Controls;
using AtomUI.Desktop.Controls.Themes;
using AtomUI.Theme;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Metadata;

namespace AtomUI.Desktop.Controls;

[PseudoClasses(StdPseudoClass.Active)]
[PseudoClasses(StdPseudoClass.Normal, StdPseudoClass.Minimized, StdPseudoClass.Maximized, StdPseudoClass.Fullscreen)]
internal class TitleBar : TemplatedControl, 
                          IControlSharedTokenResourcesHost,
                          IMotionAwareControl, 
                          IOperationSystemAware
{
    #region 公共属性定义

    public static readonly StyledProperty<Control?> LogoProperty =
        AvaloniaProperty.Register<TitleBar, Control?>(nameof(Logo));
    
    public static readonly StyledProperty<object?> TitleProperty =
        AvaloniaProperty.Register<TitleBar, object?>(nameof(Title));
    
    public static readonly StyledProperty<IDataTemplate?> TitleTemplateProperty = 
        AvaloniaProperty.Register<TitleBar, IDataTemplate?>(nameof (TitleTemplate));
    
    public static readonly StyledProperty<object?> LeftAddOnProperty =
        AvaloniaProperty.Register<TitleBar, object?>(nameof(LeftAddOn));

    public static readonly StyledProperty<IDataTemplate?> LeftAddOnTemplateProperty =
        AvaloniaProperty.Register<TitleBar, IDataTemplate?>(nameof(LeftAddOnTemplate));
    
    public static readonly StyledProperty<object?> RightAddOnProperty =
        AvaloniaProperty.Register<TitleBar, object?>(nameof(RightAddOn));

    public static readonly StyledProperty<IDataTemplate?> RightAddOnTemplateProperty =
        AvaloniaProperty.Register<TitleBar, IDataTemplate?>(nameof(RightAddOnTemplate));
    
    public static readonly StyledProperty<bool> IsWindowActiveProperty = 
        AvaloniaProperty.Register<TitleBar, bool>(nameof(IsWindowActive));
    
    public static readonly StyledProperty<bool> IsMotionEnabledProperty = 
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<TitleBar>();
    
    public static readonly StyledProperty<OsType> OsTypeProperty =
        OperationSystemAwareControlProperty.OsTypeProperty.AddOwner<TitleBar>();
    
    public static readonly StyledProperty<Version> OsVersionProperty =
        OperationSystemAwareControlProperty.OsVersionProperty.AddOwner<TitleBar>();

    public Control? Logo
    {
        get => GetValue(LogoProperty);
        set => SetValue(LogoProperty, value);
    }
    
    [DependsOn(nameof(TitleTemplate))]
    public object? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }
    
    public IDataTemplate? TitleTemplate
    {
        get => GetValue(TitleTemplateProperty);
        set => SetValue(TitleTemplateProperty, value);
    }

    [DependsOn(nameof(LeftAddOnTemplate))]
    public object? LeftAddOn
    {
        get => GetValue(LeftAddOnProperty);
        set => SetValue(LeftAddOnProperty, value);
    }

    public IDataTemplate? LeftAddOnTemplate
    {
        get => GetValue(LeftAddOnTemplateProperty);
        set => SetValue(LeftAddOnTemplateProperty, value);
    }
    
    [DependsOn(nameof(RightAddOnTemplate))]
    public object? RightAddOn
    {
        get => GetValue(RightAddOnProperty);
        set => SetValue(RightAddOnProperty, value);
    }

    public IDataTemplate? RightAddOnTemplate
    {
        get => GetValue(RightAddOnTemplateProperty);
        set => SetValue(RightAddOnTemplateProperty, value);
    }

    public bool IsWindowActive
    {
        get => GetValue(IsWindowActiveProperty);
        set => SetValue(IsWindowActiveProperty, value);
    }
    
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    public OsType OsType => GetValue(OsTypeProperty);
    public Version OsVersion => GetValue(OsVersionProperty);
    #endregion

    #region 公共属性定义

    public event EventHandler? MaximizeWindowRequested;

    #endregion

    #region 内部属性定义
    
    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => ChromeToken.ID;

    #endregion

    private CaptionButtonGroup? _captionButtonGroup;
    private CompositeDisposable? _disposables;

    static TitleBar()
    {
        FontSizeProperty.OverrideDefaultValue<TitleBar>(13);
        FontWeightProperty.OverrideDefaultValue<TitleBar>(FontWeight.Bold);
    }

    public TitleBar()
    {
        this.ConfigureOsType();
        this.RegisterResources();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _captionButtonGroup?.Detach();
        _captionButtonGroup = e.NameScope.Find<CaptionButtonGroup>(TitleBarThemeConstants.CaptionButtonGroupPart);
        if (VisualRoot is Window window)
        {
            _captionButtonGroup?.Attach(window);
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        if (VisualRoot is Window window)
        {
            _disposables = new CompositeDisposable(6)
            {
                window.GetObservable(Window.WindowStateProperty).Subscribe(x =>
                      {
                          PseudoClasses.Set(StdPseudoClass.Minimized, x == WindowState.Minimized);
                          PseudoClasses.Set(StdPseudoClass.Normal, x == WindowState.Normal);
                          PseudoClasses.Set(StdPseudoClass.Maximized, x == WindowState.Maximized);
                          PseudoClasses.Set(StdPseudoClass.Fullscreen, x == WindowState.FullScreen);
                      }),
                window.GetObservable(WindowBase.IsActiveProperty).Subscribe(isActive =>
                {
                    PseudoClasses.Set(StdPseudoClass.Active, isActive);
                    IsWindowActive = isActive;
                })
            };
        }
    }
    
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _disposables?.Dispose();
        _captionButtonGroup?.Detach();
        _captionButtonGroup = null;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (IsLoaded)
        {
            if (change.Property == IsMotionEnabledProperty)
            {
                ConfigureTransitions(true);
            }
        }
    }

    private void ConfigureTransitions(bool force)
    {
        if (IsMotionEnabled)
        {
            if (force || Transitions == null)
            {
                Transitions = [
                    TransitionUtils.CreateTransition<SolidColorBrushTransition>(ForegroundProperty)
                ];
            }
        }
        else
        {
            Transitions = null;
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (e.ClickCount == 2 && e.Properties.IsLeftButtonPressed)
        {
            MaximizeWindowRequested?.Invoke(this, EventArgs.Empty);
        }
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        ConfigureTransitions(false);
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        Transitions = null;
    }
    
    void IOperationSystemAware.SetOsType(OsType osType)
    {
        SetValue(OsTypeProperty, osType);
    }
    
    void IOperationSystemAware.SetOsVersion(Version version)
    {
        SetValue(OsVersionProperty, version);
    }
}