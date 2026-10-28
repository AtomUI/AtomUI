using System.Diagnostics;
using AtomUI.Animations;
using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Metadata;

namespace AtomUI.Desktop.Controls;

using AvaloniaButton = Avalonia.Controls.Button;

[PseudoClasses(StdPseudoClass.Pressed, BreadcrumbPseudoClass.HasIcon, BreadcrumbPseudoClass.IsLast)]
public class BreadcrumbItem : AvaloniaButton
{
    #region 公共属性定义

    public static readonly StyledProperty<PathIcon?> IconProperty =
        AvaloniaProperty.Register<BreadcrumbItem, PathIcon?>(nameof(Icon));
    
    public static readonly StyledProperty<object?> NavigateContextProperty =
        AvaloniaProperty.Register<BreadcrumbItem, object?>(nameof(NavigateContext));
    
    public static readonly StyledProperty<Uri?> NavigateUriProperty =
        AvaloniaProperty.Register<BreadcrumbItem, Uri?>(
            nameof(NavigateUri),
            defaultValue: null);
    
    public static readonly StyledProperty<object?> SeparatorProperty =
        Breadcrumb.SeparatorProperty.AddOwner<BreadcrumbItem>();
    
    public static readonly StyledProperty<IDataTemplate?> SeparatorTemplateProperty =
        Breadcrumb.SeparatorTemplateProperty.AddOwner<BreadcrumbItem>();

    public PathIcon? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public object? NavigateContext
    {
        get => GetValue(NavigateContextProperty);
        set => SetValue(NavigateContextProperty, value);
    }
    
    public Uri? NavigateUri
    {
        get => GetValue(NavigateUriProperty);
        set => SetValue(NavigateUriProperty, value);
    }
    
    [DependsOn(nameof(SeparatorTemplate))]
    public object? Separator
    {
        get => GetValue(SeparatorProperty);
        set => SetValue(SeparatorProperty, value);
    }
    
    public IDataTemplate? SeparatorTemplate
    {
        get => GetValue(SeparatorTemplateProperty);
        set => SetValue(SeparatorTemplateProperty, value);
    }

    #endregion

    #region 内部属性定义
    
    internal static readonly StyledProperty<bool> IsMotionEnabledProperty = 
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<BreadcrumbItem>();
    
    internal static readonly DirectProperty<BreadcrumbItem, bool> IsLastProperty =
        AvaloniaProperty.RegisterDirect<BreadcrumbItem, bool>(
            nameof(IsLast),
            o => o.IsLast,
            (o, v) => o.IsLast = v);
    
    internal static readonly DirectProperty<BreadcrumbItem, bool> IsNavigateResponsiveProperty =
        AvaloniaProperty.RegisterDirect<BreadcrumbItem, bool>(
            nameof(IsNavigateResponsive),
            o => o.IsNavigateResponsive,
            (o, v) => o.IsNavigateResponsive = v);
    
    internal bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    private bool _isLast;
    internal bool IsLast
    {
        get => _isLast;
        set => SetAndRaise(IsLastProperty, ref _isLast, value);
    }
    
    private bool _isNavigateResponsive;
    internal bool IsNavigateResponsive
    {
        get => _isNavigateResponsive;
        set => SetAndRaise(IsNavigateResponsiveProperty, ref _isNavigateResponsive, value);
    }

    #endregion
    
    private readonly TransitionInitializationSession _transitionInitialization;

    public BreadcrumbItem()
    {
        _transitionInitialization = new TransitionInitializationSession(this);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _transitionInitialization.CompleteInitialization();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _transitionInitialization.Reset();
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnTemplateChanged(AvaloniaPropertyChangedEventArgs e)
    {
        _transitionInitialization?.Reset();
        base.OnTemplateChanged(e);
    }

    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        Debug.Assert(Parent is Breadcrumb, "BreadcrumbItem's Parent must be Breadcrumb Control.");
    }
    
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IconProperty || 
            change.Property == IsLastProperty)
        {
            UpdatePseudoClasses();
        }
        else if (change.Property == NavigateUriProperty ||
                 change.Property == NavigateContextProperty)
        {
            IsNavigateResponsive = NavigateUri != null || NavigateContext != null;
        }
    }
    
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        UpdatePseudoClasses();
        _transitionInitialization.CompleteInitialization();
    }

    protected override void OnClick()
    {
        base.OnClick();
        if (IsNavigateResponsive)
        {
            if (Parent is Breadcrumb breadcrumb)
            {
                breadcrumb.NotifyNavigateRequest(this);
            }
            Uri? uri = NavigateUri;
            if (uri is not null)
            {
                Dispatcher.InvokeAsync(async () =>
                {
                    var topLevel = TopLevel.GetTopLevel(this);
                    if (topLevel is not null)
                    {
                        await topLevel.Launcher.LaunchUriAsync(uri);
                    }
                });
            }
        }
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(BreadcrumbPseudoClass.HasIcon, Icon is not null);
        PseudoClasses.Set(BreadcrumbPseudoClass.IsLast, IsLast);
    }
}
