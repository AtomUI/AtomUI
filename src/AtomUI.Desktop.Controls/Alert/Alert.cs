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

/// <summary>
/// The severity of an alert.
/// </summary>
public enum AlertType
{
    /// <summary>
    /// A completed or successful operation.
    /// </summary>
    Success,
    /// <summary>
    /// General information that is neither a warning nor an error.
    /// </summary>
    Info,
    /// <summary>
    /// A condition the user should check before going on.
    /// </summary>
    Warning,
    /// <summary>
    /// A failure that needs the user's attention.
    /// </summary>
    Error
}

/// <summary>
/// The pseudo-classes that <see cref="Alert" /> sets from its content, for use in style selectors.
/// </summary>
public static class AlertPseudoClass
{
    /// <summary>
    /// Set on an <see cref="Alert" /> while its <see cref="Alert.Description" /> is neither <see langword="null" />
    /// nor empty.
    /// </summary>
    public const string HasDescription = ":has-description";
    /// <summary>
    /// Set on an <see cref="Alert" /> while its <see cref="Alert.ExtraAction" /> is not <see langword="null" />.
    /// </summary>
    public const string HasExtraAction = ":has-extra-action";
}

/// <summary>
/// A feedback banner that shows a status message of one of four severities, with an optional status icon,
/// description, action content and close button.
/// The control never hides itself: the close button only raises <see cref="CloseRequest" />.
/// </summary>
[PseudoClasses(AlertPseudoClass.HasDescription, AlertPseudoClass.HasExtraAction)]
public partial class Alert : TemplatedControl
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="Type" /> property.
    /// </summary>
    public static readonly StyledProperty<AlertType> TypeProperty =
        AvaloniaProperty.Register<Alert, AlertType>(nameof(Type));

    /// <summary>
    /// Identifies the <see cref="IsShowIcon" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsShowIconProperty =
        AvaloniaProperty.Register<Alert, bool>(nameof(IsShowIcon));

    /// <summary>
    /// Identifies the <see cref="IsMessageMarqueeEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMessageMarqueeEnabledProperty =
        AvaloniaProperty.Register<Alert, bool>(nameof(IsMessageMarqueeEnabled));

    /// <summary>
    /// Identifies the <see cref="IsClosable" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsClosableProperty =
        AvaloniaProperty.Register<Alert, bool>(nameof(IsClosable));

    /// <summary>
    /// Identifies the <see cref="CloseIcon" /> property.
    /// </summary>
    public static readonly StyledProperty<PathIcon?> CloseIconProperty =
        AvaloniaProperty.Register<Alert, PathIcon?>(nameof(CloseIcon));

    /// <summary>
    /// Identifies the <see cref="Message" /> property.
    /// </summary>
    public static readonly StyledProperty<string> MessageProperty =
        AvaloniaProperty.Register<Alert, string>(nameof(Message));

    /// <summary>
    /// Identifies the <see cref="Description" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> DescriptionProperty =
        AvaloniaProperty.Register<Alert, string?>(nameof(Description));

    /// <summary>
    /// Identifies the <see cref="ExtraAction" /> property.
    /// </summary>
    public static readonly StyledProperty<Control?> ExtraActionProperty =
        AvaloniaProperty.Register<Alert, Control?>(nameof(ExtraAction));

    /// <summary>
    /// Identifies the <see cref="StrokeDashArray" /> property.
    /// </summary>
    public static readonly StyledProperty<IReadOnlyList<double>?> StrokeDashArrayProperty =
        AvaloniaProperty.Register<Alert, IReadOnlyList<double>?>(nameof(StrokeDashArray));

    /// <summary>
    /// The severity of the alert. It selects the background, border, status icon and text color.
    /// The default is <see cref="AlertType.Success" />.
    /// </summary>
    public AlertType Type
    {
        get => GetValue(TypeProperty);
        set => SetValue(TypeProperty, value);
    }

    /// <summary>
    /// Shows the status icon that belongs to <see cref="Type" /> before the content when
    /// <see langword="true" />. The default is <see langword="false" />.
    /// </summary>
    public bool IsShowIcon
    {
        get => GetValue(IsShowIconProperty);
        set => SetValue(IsShowIconProperty, value);
    }

    /// <summary>
    /// Shows <see cref="Message" /> in a <c>MarqueeLabel</c>, which scrolls the text horizontally instead of
    /// wrapping it, when <see langword="true" />. The default is <see langword="false" />.
    /// </summary>
    public bool IsMessageMarqueeEnabled
    {
        get => GetValue(IsMessageMarqueeEnabledProperty);
        set => SetValue(IsMessageMarqueeEnabledProperty, value);
    }

    /// <summary>
    /// Shows a close button at the trailing end of the alert when <see langword="true" />.
    /// Clicking it raises <see cref="CloseRequest" /> and does not hide the alert.
    /// The default is <see langword="false" />.
    /// </summary>
    public bool IsClosable
    {
        get => GetValue(IsClosableProperty);
        set => SetValue(IsClosableProperty, value);
    }

    /// <summary>
    /// The icon of the close button. When the value is <see langword="null" /> at template application, or when
    /// <see cref="IsClosable" /> changes while the control is attached to the visual tree, the control installs
    /// <see cref="CloseOutlined" /> at template priority in its place, so that a local value still overrides it.
    /// </summary>
    public PathIcon? CloseIcon
    {
        get => GetValue(CloseIconProperty);
        set => SetValue(CloseIconProperty, value);
    }

    /// <summary>
    /// The title text of the alert, in the first row of the content section. It wraps, unless
    /// <see cref="IsMessageMarqueeEnabled" /> is <see langword="true" />.
    /// It is the XAML content property: a bare text child of an <see cref="Alert" /> element is assigned to it.
    /// </summary>
    [Content]
    public string Message
    {
        get => GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    /// <summary>
    /// The secondary text below <see cref="Message" />. It is hidden while <see langword="null" /> or empty.
    /// When it is set, the alert switches to the padding, status icon size and message font size of an alert with
    /// a description.
    /// </summary>
    public string? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    /// <summary>
    /// The content of the action area at the trailing end of the alert, before the close button.
    /// The area is hidden while the value is <see langword="null" />.
    /// </summary>
    public Control? ExtraAction
    {
        get => GetValue(ExtraActionProperty);
        set => SetValue(ExtraActionProperty, value);
    }

    /// <summary>
    /// The dash pattern of the alert border, as alternating dash and gap lengths in device independent units.
    /// <see langword="null" /> or an empty list paints a solid border.
    /// </summary>
    public IReadOnlyList<double>? StrokeDashArray
    {
        get => GetValue(StrokeDashArrayProperty);
        set => SetValue(StrokeDashArrayProperty, value);
    }

    #endregion

    #region 公共事件定义

    /// <summary>
    /// Raised when the user clicks the close button, which is shown only while <see cref="IsClosable" /> is
    /// <see langword="true" />. The alert keeps its state; hiding it is up to the handler.
    /// </summary>
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

    /// <summary>
    /// Called when a styled property changes. It updates the
    /// <see cref="AlertPseudoClass.HasDescription" /> and <see cref="AlertPseudoClass.HasExtraAction" />
    /// pseudo-classes when <see cref="Description" /> or <see cref="ExtraAction" /> changes, and applies the
    /// default close icon in place of a <see langword="null" /> <see cref="CloseIcon" /> when
    /// <see cref="IsClosable" /> changes while the control is attached to the visual tree.
    /// Overrides must call the base member first.
    /// </summary>
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

    /// <summary>
    /// Locates the close button of the new template and subscribes to its click, after detaching the button of
    /// the previous template. Then it updates the pseudo-classes and applies the default close icon in place of a
    /// <see langword="null" /> <see cref="CloseIcon" />.
    /// Overrides must call the base member first.
    /// </summary>
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
