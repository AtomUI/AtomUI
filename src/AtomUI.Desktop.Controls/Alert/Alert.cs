using AtomUI.Icons.AntDesign;
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
public partial class Alert : TemplatedControl
{
    #region 公共属性定义

    public static readonly StyledProperty<AlertType> TypeProperty =
        AvaloniaProperty.Register<Alert, AlertType>(nameof(Type));

    public static readonly StyledProperty<bool> IsShowIconProperty =
        AvaloniaProperty.Register<Alert, bool>(nameof(IsShowIcon));

    public static readonly StyledProperty<bool> IsMessageMarqueeEnabledProperty =
        AvaloniaProperty.Register<Alert, bool>(nameof(IsMessageMarqueeEnabled));

    public static readonly StyledProperty<bool> IsClosableProperty =
        AvaloniaProperty.Register<Alert, bool>(nameof(IsClosable));

    public static readonly StyledProperty<PathIcon?> CloseIconProperty =
        AvaloniaProperty.Register<Alert, PathIcon?>(nameof(CloseIcon));

    public static readonly StyledProperty<string> MessageProperty =
        AvaloniaProperty.Register<Alert, string>(nameof(Message));

    public static readonly StyledProperty<string?> DescriptionProperty =
        AvaloniaProperty.Register<Alert, string?>(nameof(Description));

    public static readonly StyledProperty<Control?> ExtraActionProperty =
        AvaloniaProperty.Register<Alert, Control?>(nameof(ExtraAction));

    public static readonly StyledProperty<IReadOnlyList<double>?> StrokeDashArrayProperty =
        AvaloniaProperty.Register<Alert, IReadOnlyList<double>?>(nameof(StrokeDashArray));

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

    public bool IsMessageMarqueeEnabled
    {
        get => GetValue(IsMessageMarqueeEnabledProperty);
        set => SetValue(IsMessageMarqueeEnabledProperty, value);
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

    public IReadOnlyList<double>? StrokeDashArray
    {
        get => GetValue(StrokeDashArrayProperty);
        set => SetValue(StrokeDashArrayProperty, value);
    }

    #endregion

    #region 公共事件定义

    public event EventHandler? CloseRequest;

    #endregion
    
    private IconButton? _closeButton;

    static Alert()
    {
        AffectsMeasure<Alert>(IsClosableProperty,
            IsShowIconProperty,
            MessageProperty,
            DescriptionProperty,
            IsMessageMarqueeEnabledProperty,
            PaddingProperty,
            ExtraActionProperty,
            IsMessageMarqueeEnabledProperty);
        AffectsRender<Alert>(TypeProperty);
    }

    public Alert()
    {
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
        
        if (change.Property == DescriptionProperty ||
            change.Property == ExtraActionProperty)
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
        _closeButton = e.NameScope.Find<IconButton>("PART_CloseBtn");
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
