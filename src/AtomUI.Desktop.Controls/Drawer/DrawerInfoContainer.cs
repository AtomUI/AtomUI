using AtomUI.Controls;
using AtomUI.Desktop.Controls.Themes;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls;

internal class DrawerInfoContainer : HeaderedContentControl
{
    #region 内部属性定义
    
    internal static readonly DirectProperty<DrawerInfoContainer, DrawerPlacement> PlacementProperty =
        AvaloniaProperty.RegisterDirect<DrawerInfoContainer, DrawerPlacement>(nameof(Placement),
            o => o.Placement,
            (o, v) => o.Placement = v);
    
    internal static readonly DirectProperty<DrawerInfoContainer, bool> IsShowCloseButtonProperty =
        AvaloniaProperty.RegisterDirect<DrawerInfoContainer, bool>(nameof(IsShowCloseButton),
            o => o.IsShowCloseButton,
            (o, v) => o.IsShowCloseButton = v);
    
    internal static readonly DirectProperty<DrawerInfoContainer, string> TitleProperty =
        AvaloniaProperty.RegisterDirect<DrawerInfoContainer, string>(nameof(Title),
            o => o.Title,
            (o, v) => o.Title = v);
    
    internal static readonly DirectProperty<DrawerInfoContainer, object?> FooterProperty =
        AvaloniaProperty.RegisterDirect<DrawerInfoContainer, object?>(nameof(Footer),
            o => o.Footer,
            (o, v) => o.Footer = v);
    
    internal static readonly DirectProperty<DrawerInfoContainer, IDataTemplate?> FooterTemplateProperty =
        AvaloniaProperty.RegisterDirect<DrawerInfoContainer, IDataTemplate?>(nameof(FooterTemplate),
            o => o.FooterTemplate,
            (o, v) => o.FooterTemplate = v);
    
    internal static readonly DirectProperty<DrawerInfoContainer, object?> ExtraProperty =
        AvaloniaProperty.RegisterDirect<DrawerInfoContainer, object?>(nameof(Extra),
            o => o.Extra,
            (o, v) => o.Extra = v);
    
    internal static readonly DirectProperty<DrawerInfoContainer, IDataTemplate?> ExtraTemplateProperty =
        AvaloniaProperty.RegisterDirect<DrawerInfoContainer, IDataTemplate?>(nameof(ExtraTemplate),
            o => o.ExtraTemplate,
            (o, v) => o.ExtraTemplate = v);
    
    internal static readonly DirectProperty<DrawerInfoContainer, double> DialogSizeProperty =
        AvaloniaProperty.RegisterDirect<DrawerInfoContainer, double>(nameof(DialogSize),
            o => o.DialogSize,
            (o, v) => o.DialogSize = v);

    internal static readonly DirectProperty<DrawerInfoContainer, bool> HasFooterProperty =
        AvaloniaProperty.RegisterDirect<DrawerInfoContainer, bool>(nameof(HasFooter),
            o => o.HasFooter,
            (o, v) => o.HasFooter = v);

    internal static readonly DirectProperty<DrawerInfoContainer, bool> HasExtraProperty =
        AvaloniaProperty.RegisterDirect<DrawerInfoContainer, bool>(nameof(HasExtra),
            o => o.HasExtra,
            (o, v) => o.HasExtra = v);
    
    internal static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<DrawerInfoContainer>();

    private DrawerPlacement _placement = DrawerPlacement.Right;

    internal DrawerPlacement Placement
    {
        get => _placement;
        set => SetAndRaise(PlacementProperty, ref _placement, value);
    }

    private bool _isShowCloseButton = true;

    internal bool IsShowCloseButton
    {
        get => _isShowCloseButton;
        set => SetAndRaise(IsShowCloseButtonProperty, ref _isShowCloseButton, value);
    }

    private string _title = string.Empty;

    internal string Title
    {
        get => _title;
        set => SetAndRaise(TitleProperty, ref _title, value);
    }

    private object? _footer;

    internal object? Footer
    {
        get => _footer;
        set => SetAndRaise(FooterProperty, ref _footer, value);
    }
    
    private IDataTemplate? _footerTemplate;

    internal IDataTemplate? FooterTemplate
    {
        get => _footerTemplate;
        set => SetAndRaise(FooterTemplateProperty, ref _footerTemplate, value);
    }
    
    private object? _extra;

    internal object? Extra
    {
        get => _extra;
        set => SetAndRaise(ExtraProperty, ref _extra, value);
    }
    
    private IDataTemplate? _extraTemplate;

    internal IDataTemplate? ExtraTemplate
    {
        get => _extraTemplate;
        set => SetAndRaise(ExtraTemplateProperty, ref _extraTemplate, value);
    }
    
    private double _dialogSize;

    internal double DialogSize
    {
        get => _dialogSize;
        set => SetAndRaise(DialogSizeProperty, ref _dialogSize, value);
    }
    
    private bool _hasFooter;

    internal bool HasFooter
    {
        get => _hasFooter;
        set => SetAndRaise(HasFooterProperty, ref _hasFooter, value);
    }
    
    private bool _hasExtra;

    internal bool HasExtra
    {
        get => _hasExtra;
        set => SetAndRaise(HasExtraProperty, ref _hasExtra, value);
    }
    
    internal bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    #endregion
    
    internal event EventHandler? CloseRequested;
    private IconButton? _closeButton;

    private void ConfigureTransitions(bool force)
    {
        if (IsMotionEnabled)
        {
            if (force || Transitions == null)
            {
                Transitions = [
                    TransitionUtils.CreateTransition<TransformOperationsTransition>(RenderTransformProperty)
                ];
            }
        }
        else
        {
            Transitions = null;
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == FooterProperty || change.Property == FooterTemplateProperty)
        {
            HasFooter = Footer != null || FooterTemplate != null;
        } 
        else if (change.Property == ExtraProperty || change.Property == ExtraTemplateProperty)
        {
            HasExtra = Extra != null || ExtraTemplate != null;
        }

        if (IsLoaded)
        {
            if (change.Property == IsMotionEnabledProperty)
            {
                ConfigureTransitions(true);
            }
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            e.Handled = true;
        }
    }
    
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        if (e.InitialPressMouseButton == MouseButton.Left)
        {
            e.Handled = true;
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (_closeButton != null)
        {
            _closeButton.Click -= HandleCloseButtonClick;
        }
        _closeButton = e.NameScope.Find<IconButton>(DrawerInfoContainerThemeConstants.CloseButtonPart);
        if (_closeButton != null)
        {
            _closeButton.Click += HandleCloseButtonClick;
        }
    }

    private void HandleCloseButtonClick(object? sender, RoutedEventArgs e)
    {
        CloseRequested?.Invoke(this, EventArgs.Empty);
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