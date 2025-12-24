using AtomUI.Desktop.Controls.Themes;
using AtomUI.Icons.AntDesign;
using AtomUI.Theme;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Metadata;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

public enum AlertType
{
    Success,
    Info,
    Warning,
    Error
}

public static class AlertPseudoClass
{
    public const string HasDescription = ":has-description";
    public const string HasExtraAction = ":has-extra-action";
}

[PseudoClasses(AlertPseudoClass.HasDescription, AlertPseudoClass.HasExtraAction)]
public class Alert : TemplatedControl, IControlSharedTokenResourcesHost
{
    #region 公共属性定义

    public static readonly StyledProperty<AlertType> TypeProperty =
        AvaloniaProperty.Register<Alert, AlertType>(nameof(Type));

    public static readonly StyledProperty<bool> IsShowIconProperty =
        AvaloniaProperty.Register<Alert, bool>(nameof(IsShowIcon));

    public static readonly StyledProperty<bool> IsMessageMarqueEnabledProperty =
        AvaloniaProperty.Register<Alert, bool>(nameof(IsMessageMarqueEnabled));

    public static readonly StyledProperty<bool> IsClosableProperty =
        AvaloniaProperty.Register<Alert, bool>(nameof(IsClosable));

    public static readonly StyledProperty<PathIcon?> CloseIconProperty =
        AvaloniaProperty.Register<Alert, PathIcon?>(nameof(CloseIcon));

    public static readonly StyledProperty<string> MessageProperty =
        AvaloniaProperty.Register<Alert, string>(nameof(Message));

    public static readonly StyledProperty<string?> DescriptionProperty =
        AvaloniaProperty.Register<Alert, string?>(nameof(Description));

    public static readonly StyledProperty<Control?> ExtraActionProperty =
        AvaloniaProperty.Register<Alert, Control?>(nameof(Description));

    public AlertType Type
    {
        get => GetValue(TypeProperty);
        set => SetValue(TypeProperty, value);
    }

    public bool IsShowIcon
    {
        get => GetValue(IsShowIconProperty);
        set => SetValue(IsShowIconProperty, value);
    }

    public bool IsMessageMarqueEnabled
    {
        get => GetValue(IsMessageMarqueEnabledProperty);
        set => SetValue(IsMessageMarqueEnabledProperty, value);
    }

    public bool IsClosable
    {
        get => GetValue(IsClosableProperty);
        set => SetValue(IsClosableProperty, value);
    }

    public PathIcon? CloseIcon
    {
        get => GetValue(CloseIconProperty);
        set => SetValue(CloseIconProperty, value);
    }

    [Content]
    public string Message
    {
        get => GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public string? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public Control? ExtraAction
    {
        get => GetValue(ExtraActionProperty);
        set => SetValue(ExtraActionProperty, value);
    }

    #endregion

    #region 公共事件定义

    public event EventHandler? CloseRequest;

    #endregion

    #region 内部属性定义

    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => AlertToken.ID;

    #endregion
    
    private IconButton? _closeButton;

    static Alert()
    {
        AffectsMeasure<Alert>(IsClosableProperty,
            IsShowIconProperty,
            MessageProperty,
            DescriptionProperty,
            IsMessageMarqueEnabledProperty,
            PaddingProperty,
            ExtraActionProperty,
            IsMessageMarqueEnabledProperty);
        AffectsRender<Alert>(TypeProperty);
    }

    public Alert()
    {
        this.RegisterResources();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (this.IsAttachedToVisualTree())
        {
            if (change.Property == IsClosableProperty)
            {
                SetupCloseButton();
            }
        }
        
        if (change.Property == DescriptionProperty)
        {
            UpdatePseudoClasses();
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (_closeButton != null)
        {
            _closeButton.Click -= HandleCloseBtnClick;
        }
        _closeButton = e.NameScope.Find<IconButton>(AlertThemeConstants.CloseBtnPart);
        if (_closeButton != null)
        {
            _closeButton.Click += HandleCloseBtnClick;
        }
        UpdatePseudoClasses();
        SetupCloseButton();
    }

    private void HandleCloseBtnClick(object? sender, RoutedEventArgs e)
    {
        CloseRequest?.Invoke(this, EventArgs.Empty);
    }

    private void SetupCloseButton()
    {
        if (CloseIcon == null)
        {
            ClearValue(CloseIconProperty);
            SetValue(CloseIconProperty, new CloseOutlined(), BindingPriority.Template);
        }
    }
    
    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(AlertPseudoClass.HasDescription, !string.IsNullOrEmpty(Description));
        PseudoClasses.Set(AlertPseudoClass.HasExtraAction, ExtraAction != null);
    }
}