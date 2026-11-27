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

/// <summary>
/// One item of a <see cref="Breadcrumb" />, with an optional icon before its content and an optional
/// separator after it.
/// </summary>
/// <remarks>
/// A <see cref="Breadcrumb" /> generates an item for every object in its items that is not a
/// <see cref="BreadcrumbItem" />, and transfers the values of an <see cref="IBreadcrumbItemData" /> to it.
/// The item relies on being the logical child of a <see cref="Breadcrumb" />; a debug build asserts this
/// when the item is attached to the logical tree.
/// </remarks>
[PseudoClasses(StdPseudoClass.Pressed, BreadcrumbPseudoClass.HasIcon, BreadcrumbPseudoClass.IsLast)]
public class BreadcrumbItem : AvaloniaButton
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="Icon" /> property.
    /// </summary>
    public static readonly StyledProperty<PathIcon?> IconProperty =
        AvaloniaProperty.Register<BreadcrumbItem, PathIcon?>(nameof(Icon));
    
    /// <summary>
    /// Identifies the <see cref="NavigateContext" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> NavigateContextProperty =
        AvaloniaProperty.Register<BreadcrumbItem, object?>(nameof(NavigateContext));
    
    /// <summary>
    /// Identifies the <see cref="NavigateUri" /> property.
    /// </summary>
    public static readonly StyledProperty<Uri?> NavigateUriProperty =
        AvaloniaProperty.Register<BreadcrumbItem, Uri?>(
            nameof(NavigateUri),
            defaultValue: null);
    
    /// <summary>
    /// Identifies the <see cref="Separator" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> SeparatorProperty =
        Breadcrumb.SeparatorProperty.AddOwner<BreadcrumbItem>();
    
    /// <summary>
    /// Identifies the <see cref="SeparatorTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> SeparatorTemplateProperty =
        Breadcrumb.SeparatorTemplateProperty.AddOwner<BreadcrumbItem>();

    /// <summary>
    /// Icon shown before the content, or <see langword="null" /> to show none.
    /// </summary>
    public PathIcon? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>
    /// Application-defined navigation target of the item, or <see langword="null" /> for none.
    /// </summary>
    /// <remarks>
    /// A value other than <see langword="null" /> makes a click raise <see cref="Breadcrumb.NavigateRequest" />
    /// on the breadcrumb, where a handler reads this value from the clicked item, and makes the default theme
    /// paint the item as a link.
    /// </remarks>
    public object? NavigateContext
    {
        get => GetValue(NavigateContextProperty);
        set => SetValue(NavigateContextProperty, value);
    }
    
    /// <summary>
    /// URI that is handed to the system launcher when the item is clicked, or <see langword="null" /> to open
    /// none.
    /// </summary>
    /// <remarks>
    /// A value other than <see langword="null" /> also makes a click raise
    /// <see cref="Breadcrumb.NavigateRequest" /> on the breadcrumb before the launch, and makes the default
    /// theme paint the item as a link.
    /// </remarks>
    public Uri? NavigateUri
    {
        get => GetValue(NavigateUriProperty);
        set => SetValue(NavigateUriProperty, value);
    }
    
    /// <summary>
    /// Content drawn as the separator between this item and the next one. It overrides the
    /// <see cref="Breadcrumb.Separator" /> of the breadcrumb.
    /// </summary>
    /// <remarks>
    /// The last item has no separator, so the value has no effect there. <see langword="null" /> leaves the
    /// separator without content.
    /// </remarks>
    [DependsOn(nameof(SeparatorTemplate))]
    public object? Separator
    {
        get => GetValue(SeparatorProperty);
        set => SetValue(SeparatorProperty, value);
    }
    
    /// <summary>
    /// Template that builds the separator between this item and the next one. It overrides the
    /// <see cref="Breadcrumb.SeparatorTemplate" /> of the breadcrumb.
    /// </summary>
    /// <remarks>
    /// The last item has no separator, so the value has no effect there. <see langword="null" /> displays
    /// <see cref="Separator" /> without a template.
    /// </remarks>
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

    /// <summary>
    /// After the base call, completes the transition initialization, which ends the suppression of the item's
    /// transitions.
    /// </summary>
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _transitionInitialization.CompleteInitialization();
    }

    /// <summary>
    /// Before the base call, resets the transition initialization, so that the item's transitions are suppressed
    /// again until it is loaded once more.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _transitionInitialization.Reset();
        base.OnDetachedFromVisualTree(e);
    }

    /// <summary>
    /// Before the base call, resets the transition initialization, so that the item's transitions are suppressed
    /// again until the new template is applied.
    /// </summary>
    protected override void OnTemplateChanged(AvaloniaPropertyChangedEventArgs e)
    {
        _transitionInitialization?.Reset();
        base.OnTemplateChanged(e);
    }

    /// <summary>
    /// After the base call, asserts in a debug build that the parent is a <see cref="Breadcrumb" />, which the
    /// item needs to raise <see cref="Breadcrumb.NavigateRequest" />.
    /// </summary>
    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        Debug.Assert(Parent is Breadcrumb, "BreadcrumbItem's Parent must be Breadcrumb Control.");
    }
    
    /// <summary>
    /// After the base call, updates the pseudo-classes when <see cref="Icon" /> or <c>IsLast</c> changes, and
    /// updates whether the item reacts to navigation when <see cref="NavigateUri" /> or
    /// <see cref="NavigateContext" /> changes.
    /// </summary>
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
    
    /// <summary>
    /// After the base call, updates the pseudo-classes and completes the transition initialization.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        UpdatePseudoClasses();
        _transitionInitialization.CompleteInitialization();
    }

    /// <summary>
    /// After the base call, raises <see cref="Breadcrumb.NavigateRequest" /> on the breadcrumb when
    /// <see cref="NavigateUri" /> or <see cref="NavigateContext" /> is set, and afterwards hands a set
    /// <see cref="NavigateUri" /> to the system launcher.
    /// </summary>
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
