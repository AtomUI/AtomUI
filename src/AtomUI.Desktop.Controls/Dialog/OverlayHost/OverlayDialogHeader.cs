using System.Reactive.Disposables;
using AtomUI.Animations;
using AtomUI.Controls;
using AtomUI.Desktop.Controls.Themes;
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
[PseudoClasses(StdPseudoClass.Normal, StdPseudoClass.Minimized, StdPseudoClass.Maximized)]
internal class OverlayDialogHeader : TemplatedControl, IMotionAwareControl
{
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<OverlayDialogHeader, string?>(nameof (Title));
    
    public static readonly StyledProperty<PathIcon?> LogoProperty =
        AvaloniaProperty.Register<OverlayDialogHeader, PathIcon?>(nameof(Logo));
    
    public static readonly StyledProperty<double> TitleFontSizeProperty =
        AvaloniaProperty.Register<OverlayDialogHeader, double>(nameof(TitleFontSize), defaultValue: 14);

    public static readonly StyledProperty<FontWeight> TitleFontWeightProperty =
        AvaloniaProperty.Register<OverlayDialogHeader, FontWeight>(nameof(TitleFontWeight), defaultValue: FontWeight.Bold);
    
    public static readonly StyledProperty<object?> AddOnProperty =
        AvaloniaProperty.Register<OverlayDialogHeader, object?>(nameof(AddOn));

    public static readonly StyledProperty<IDataTemplate?> AddOnTemplateProperty =
        AvaloniaProperty.Register<OverlayDialogHeader, IDataTemplate?>(nameof(AddOnTemplate));
    
    public static readonly StyledProperty<bool> IsActivatedProperty = 
        AvaloniaProperty.Register<OverlayDialogHeader, bool>(nameof(IsActivated));
    
    public static readonly StyledProperty<bool> IsMaximizeButtonEnabledProperty =
        AvaloniaProperty.Register<OverlayDialogHeader, bool>(nameof(IsMaximizeButtonEnabled), defaultValue: false);
    
    public static readonly StyledProperty<bool> IsCloseButtonEnabledProperty =
        AvaloniaProperty.Register<OverlayDialogHeader, bool>(nameof(IsCloseButtonEnabled), defaultValue:true);
    
    public static readonly StyledProperty<bool> IsMotionEnabledProperty = 
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<OverlayDialogHeader>();
    
    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }
    
    public PathIcon? Logo
    {
        get => GetValue(LogoProperty);
        set => SetValue(LogoProperty, value);
    }

    public double TitleFontSize
    {
        get => GetValue(TitleFontSizeProperty);
        set => SetValue(TitleFontSizeProperty, value);
    }

    public FontWeight TitleFontWeight
    {
        get => GetValue(TitleFontWeightProperty);
        set => SetValue(TitleFontWeightProperty, value);
    }

    [DependsOn(nameof(AddOnTemplate))]
    public object? AddOn
    {
        get => GetValue(AddOnProperty);
        set => SetValue(AddOnProperty, value);
    }

    public IDataTemplate? AddOnTemplate
    {
        get => GetValue(AddOnTemplateProperty);
        set => SetValue(AddOnTemplateProperty, value);
    }
    
    public bool IsActivated
    {
        get => GetValue(IsActivatedProperty);
        set => SetValue(IsActivatedProperty, value);
    }
    
    public bool IsMaximizeButtonEnabled
    {
        get => GetValue(IsMaximizeButtonEnabledProperty);
        set => SetValue(IsMaximizeButtonEnabledProperty, value);
    }
    
    public bool IsCloseButtonEnabled
    {
        get => GetValue(IsCloseButtonEnabledProperty);
        set => SetValue(IsCloseButtonEnabledProperty, value);
    }
    
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    #region 内部属性定义
    
    internal static readonly DirectProperty<OverlayDialogHeader, bool> IsDialogMaximizedProperty =
        AvaloniaProperty.RegisterDirect<OverlayDialogHeader, bool>(
            nameof(IsDialogMaximized),
            o => o.IsDialogMaximized,
            (o, v) => o.IsDialogMaximized = v);
    
    private bool _isDialogMaximized;

    internal bool IsDialogMaximized
    {
        get => _isDialogMaximized;
        set => SetAndRaise(IsDialogMaximizedProperty, ref _isDialogMaximized, value);
    }

    #endregion

    #region 内部事件定义

    internal event EventHandler? CloseRequest;
    internal event EventHandler? MaximizeRequest;
    internal event EventHandler? NormalizeRequest;
    internal event EventHandler? Pressed;
    
    #endregion
    
    private CompositeDisposable? _disposables;
    private DialogCaptionButton? _maximizeButton;
    private DialogCaptionButton? _closeButton;
    
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        if (VisualRoot is Window window)
        {
            _disposables = new CompositeDisposable(6)
            {
                window.GetObservable(OverlayDialogHost.WindowStateProperty).Subscribe(x =>
                {
                    PseudoClasses.Set(StdPseudoClass.Normal, x == OverlayDialogState.Normal);
                    PseudoClasses.Set(StdPseudoClass.Maximized, x == OverlayDialogState.Maximized);
                }),
            };
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        
        if (_closeButton != null)
        {
            _closeButton.Click -= HandleCloseButtonClicked;
        }
        
        if (_maximizeButton != null)
        {
            _maximizeButton.Click -= HandleMaximizeButtonClicked;
        }
        
        _closeButton    = e.NameScope.Find<DialogCaptionButton>(OverlayDialogHeaderThemeConstants.CloseButtonPart);
        _maximizeButton = e.NameScope.Find<DialogCaptionButton>(OverlayDialogHeaderThemeConstants.MaximizeButtonPart);
        
        if (_closeButton != null)
        {
            _closeButton.Click += HandleCloseButtonClicked;
        }

        if (_maximizeButton != null)
        {
            _maximizeButton.Click += HandleMaximizeButtonClicked;
        }
    }
    
    private void HandleMaximizeButtonClicked(object? sender, RoutedEventArgs args)
    {
        if (sender is DialogCaptionButton captionButton)
        {
            if (!captionButton.IsChecked)
            {
                MaximizeRequest?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                NormalizeRequest?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        Pressed?.Invoke(this, EventArgs.Empty);
    }

    private void HandleCloseButtonClicked(object? sender, RoutedEventArgs e)
    {
        CloseRequest?.Invoke(this, EventArgs.Empty);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _disposables?.Dispose();
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
}