using AtomUI.Icons.AntDesign;
using AtomUI.Theme;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Metadata;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

internal class PopupConfirmContainer : TemplatedControl, IControlThemeGlobalOwnerSource, IThemeInheritanceSource
{
    #region 公共属性定义

    public static readonly StyledProperty<string> OkTextProperty =
        PopupConfirm.OkTextProperty.AddOwner<PopupConfirmContainer>();

    public static readonly StyledProperty<string> CancelTextProperty =
        PopupConfirm.CancelTextProperty.AddOwner<PopupConfirmContainer>();

    public static readonly StyledProperty<ButtonType> OkButtonTypeProperty =
        PopupConfirm.OkButtonTypeProperty.AddOwner<PopupConfirmContainer>();

    public static readonly StyledProperty<bool> IsShowCancelButtonProperty =
        PopupConfirm.IsShowCancelButtonProperty.AddOwner<PopupConfirmContainer>();

    public static readonly StyledProperty<string> TitleProperty =
        PopupConfirm.TitleProperty.AddOwner<PopupConfirmContainer>();
    
    public static readonly StyledProperty<object?> ConfirmContentProperty =
        PopupConfirm.ConfirmContentProperty.AddOwner<PopupConfirmContainer>();

    public static readonly StyledProperty<IDataTemplate?> ConfirmContentTemplateProperty =
        PopupConfirm.ConfirmContentTemplateProperty.AddOwner<PopupConfirmContainer>();

    public static readonly StyledProperty<PathIcon?> IconProperty =
        PopupConfirm.IconProperty.AddOwner<PopupConfirmContainer>();

    public static readonly StyledProperty<PopupConfirmStatus> ConfirmStatusProperty =
        PopupConfirm.ConfirmStatusProperty.AddOwner<PopupConfirmContainer>();

    public string OkText
    {
        get => GetValue(OkTextProperty);
        set => SetValue(OkTextProperty, value);
    }

    public string CancelText
    {
        get => GetValue(CancelTextProperty);
        set => SetValue(CancelTextProperty, value);
    }

    public ButtonType OkButtonType
    {
        get => GetValue(OkButtonTypeProperty);
        set => SetValue(OkButtonTypeProperty, value);
    }

    public bool IsShowCancelButton
    {
        get => GetValue(IsShowCancelButtonProperty);
        set => SetValue(IsShowCancelButtonProperty, value);
    }

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    [DependsOn(nameof(ConfirmContentTemplate))]
    public object? ConfirmContent
    {
        get => GetValue(ConfirmContentProperty);
        set => SetValue(ConfirmContentProperty, value);
    }

    public IDataTemplate? ConfirmContentTemplate
    {
        get => GetValue(ConfirmContentTemplateProperty);
        set => SetValue(ConfirmContentTemplateProperty, value);
    }

    public PathIcon? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public PopupConfirmStatus ConfirmStatus
    {
        get => GetValue(ConfirmStatusProperty);
        set => SetValue(ConfirmStatusProperty, value);
    }

    #endregion
    
    internal WeakReference<PopupConfirm> PopupConfirmRef { get; set; }
    private Button? _okButton;
    private Button? _cancelButton;
    private ThemeContextResourceBridge? _themeResourceBridge;

    static PopupConfirmContainer()
    {
        Button.ClickEvent.AddClassHandler<PopupConfirmContainer>(
            (container, args) => container.HandleButtonClick(args));
    }

    public PopupConfirmContainer(PopupConfirm popupConfirm)
    {
        PopupConfirmRef = new WeakReference<PopupConfirm>(popupConfirm);
    }

    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        UpdateThemeResourceBridge();
    }

    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        ClearThemeResourceBridge();
        base.OnDetachedFromLogicalTree(e);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        SetupDefaultIcon();
        _okButton     = e.NameScope.Find<Button>("PART_OkButton");
        _cancelButton = e.NameScope.Find<Button>("PART_CancelButton");
        if (_okButton is not null)
        {
            _okButton.Width  =  double.NaN;
            _okButton.Height =  double.NaN;
        }

        if (_cancelButton is not null)
        {
            _cancelButton.Width  =  double.NaN;
            _cancelButton.Height =  double.NaN;
        }
        UpdatePseudoClasses();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ThemeScope.ContextProperty && ((ILogical)this).IsAttachedToLogicalTree)
        {
            UpdateThemeResourceBridge();
        }
        if (this.IsAttachedToVisualTree())
        {
            if (change.Property == IconProperty)
            {
                SetupDefaultIcon();
            }
        }

        if (change.Property == ConfirmContentProperty ||
            change.Property == ConfirmContentTemplateProperty)
        {
            UpdatePseudoClasses();
        }
    }

    #region 实现 IControlThemeGlobalOwnerSource 接口

    Control? IControlThemeGlobalOwnerSource.ThemeGlobalOwner =>
        PopupConfirmRef.TryGetTarget(out var owner) ? owner : null;

    #endregion

    #region 实现 IThemeInheritanceSource 接口

    AvaloniaObject? IThemeInheritanceSource.ThemeInheritanceParent => InheritanceParent;

    #endregion

    private void HandleButtonClick(RoutedEventArgs args)
    {
        var isConfirmed = false;
        if (ReferenceEquals(args.Source, _okButton))
        {
            isConfirmed = true;
        }
        else if (ReferenceEquals(args.Source, _cancelButton))
        {
            if (!IsShowCancelButton)
            {
                return;
            }
        }
        else
        {
            return;
        }

        if (PopupConfirmRef.TryGetTarget(out var popupConfirm))
        {
            var eventArgs = new RoutedEventArgs(isConfirmed
                ? PopupConfirm.ConfirmedEvent
                : PopupConfirm.CancelledEvent);
            popupConfirm.RaiseEvent(eventArgs);

            var popupEventArgs = new PopupConfirmClickEventArgs(PopupConfirm.PopupClickEvent, isConfirmed);
            popupConfirm.RaiseEvent(popupEventArgs);
            popupConfirm.HideFlyout(true);
        }
    }

    private void SetupDefaultIcon()
    {
        if (Icon == null)
        {
            ClearValue(IconProperty);
            SetValue(IconProperty, new ExclamationCircleFilled(), BindingPriority.Template);
        }
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(PopupConfirmPseudoClass.EmptyContent, ConfirmContent == null);
    }

    private void UpdateThemeResourceBridge()
    {
        var context = GetValue(ThemeScope.ContextProperty) ?? ThemeScope.ResolveContext(this);
        if (context is not null && ReferenceEquals(_themeResourceBridge?.OwnerContext, context))
        {
            return;
        }
        ClearThemeResourceBridge();
        if (context is not null)
        {
            // The runtime flyout root is outside its owner's logical children.
            // AXAML cannot forward that missing publication; reuse the scoped bridge.
            _themeResourceBridge = new ThemeContextResourceBridge(context);
            Resources.MergedDictionaries.Add(_themeResourceBridge);
        }
    }

    private void ClearThemeResourceBridge()
    {
        if (_themeResourceBridge is { } bridge)
        {
            _themeResourceBridge = null;
            Resources.MergedDictionaries.Remove(bridge);
            bridge.Dispose();
        }
    }

}
