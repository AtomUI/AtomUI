using AtomUI.Controls;
using AtomUI.Icons.AntDesign;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

using AvaloniaTextBox = Avalonia.Controls.TextBox;

/// <summary>
/// Shared text-input logic for AtomUI desktop text controls.
/// </summary>
public abstract class AbstractTextInput : AvaloniaTextBox,
                                           IMotionAwareControl,
                                           ICustomizableSizeTypeAware,
                                           ICompactSpaceAware,
                                           IFormItemAware,
                                           IFormItemFeedbackAware,
                                           IInputControlStatusAware,
                                           IInputControlStyleVariantAware,
                                           IThemeInheritanceSource
{
    #region 公共属性定义

    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<AbstractTextInput>();

    public static readonly StyledProperty<InputControlStyleVariant> StyleVariantProperty =
        InputControlStyleVariantProperty.StyleVariantProperty.AddOwner<AbstractTextInput>();

    public static readonly StyledProperty<InputControlStatus> StatusProperty =
        InputControlStatusProperty.StatusProperty.AddOwner<AbstractTextInput>();

    public static readonly StyledProperty<bool> IsAllowClearProperty =
        AvaloniaProperty.Register<AbstractTextInput, bool>(nameof(IsAllowClear));

    public static readonly StyledProperty<bool> IsEnableRevealButtonProperty =
        AvaloniaProperty.Register<AbstractTextInput, bool>(nameof(IsEnableRevealButton));

    public static readonly StyledProperty<bool> IsCustomFontSizeProperty =
        AvaloniaProperty.Register<AbstractTextInput, bool>(nameof(IsCustomFontSize));

    public static readonly StyledProperty<bool> IsShowCountProperty =
        AvaloniaProperty.Register<AbstractTextInput, bool>(nameof(IsShowCount));

    public static readonly StyledProperty<PathIcon?> ClearIconProperty =
        AvaloniaProperty.Register<AbstractTextInput, PathIcon?>(nameof(ClearIcon));

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<AbstractTextInput>();

    public CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    public InputControlStyleVariant StyleVariant
    {
        get => GetValue(StyleVariantProperty);
        set => SetValue(StyleVariantProperty, value);
    }

    public InputControlStatus Status
    {
        get => GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    public bool IsAllowClear
    {
        get => GetValue(IsAllowClearProperty);
        set => SetValue(IsAllowClearProperty, value);
    }

    public bool IsEnableRevealButton
    {
        get => GetValue(IsEnableRevealButtonProperty);
        set => SetValue(IsEnableRevealButtonProperty, value);
    }

    public bool IsCustomFontSize
    {
        get => GetValue(IsCustomFontSizeProperty);
        set => SetValue(IsCustomFontSizeProperty, value);
    }

    public bool IsShowCount
    {
        get => GetValue(IsShowCountProperty);
        set => SetValue(IsShowCountProperty, value);
    }

    public PathIcon? ClearIcon
    {
        get => GetValue(ClearIconProperty);
        set => SetValue(ClearIconProperty, value);
    }

    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    #endregion

    #region 内部属性定义

    internal static readonly DirectProperty<AbstractTextInput, bool> IsEffectiveShowClearButtonProperty =
        AvaloniaProperty.RegisterDirect<AbstractTextInput, bool>(
            nameof(IsEffectiveShowClearButton),
            o => o.IsEffectiveShowClearButton,
            (o, value) => o.IsEffectiveShowClearButton = value);

    internal static readonly DirectProperty<AbstractTextInput, string?> CountTextProperty =
        AvaloniaProperty.RegisterDirect<AbstractTextInput, string?>(
            nameof(CountText),
            o => o.CountText,
            (o, value) => o.CountText = value);

    internal static readonly StyledProperty<SpaceItemPosition?> CompactSpaceItemPositionProperty =
        CompactSpaceAwareControlProperty.CompactSpaceItemPositionProperty.AddOwner<AbstractTextInput>();

    internal static readonly StyledProperty<Orientation> CompactSpaceOrientationProperty =
        CompactSpaceAwareControlProperty.CompactSpaceOrientationProperty.AddOwner<AbstractTextInput>();

    internal static readonly StyledProperty<bool> IsUsedInCompactSpaceProperty =
        CompactSpaceAwareControlProperty.IsUsedInCompactSpaceProperty.AddOwner<AbstractTextInput>();

    internal static readonly StyledProperty<FormValidateFeedback?> FormFeedbackProperty =
        AvaloniaProperty.Register<AbstractTextInput, FormValidateFeedback?>(nameof(FormFeedback));

    internal static readonly StyledProperty<FormValidateStatus> FormStatusProperty =
        InputControlState.FormStatusProperty.AddOwner<AbstractTextInput>();

    internal static readonly DirectProperty<AbstractTextInput, bool> IsFormFeedbackVisibleProperty =
        AvaloniaProperty.RegisterDirect<AbstractTextInput, bool>(
            nameof(IsFormFeedbackVisible),
            o => o.IsFormFeedbackVisible,
            (o, value) => o.IsFormFeedbackVisible = value);

    internal static readonly DirectProperty<AbstractTextInput, bool> IsPlaceholderTextVisibleProperty =
        AvaloniaProperty.RegisterDirect<AbstractTextInput, bool>(
            nameof(IsPlaceholderTextVisible),
            o => o.IsPlaceholderTextVisible,
            (o, value) => o.IsPlaceholderTextVisible = value);

    private bool _isEffectiveShowClearButton;

    internal bool IsEffectiveShowClearButton
    {
        get => _isEffectiveShowClearButton;
        private set => SetAndRaise(IsEffectiveShowClearButtonProperty, ref _isEffectiveShowClearButton, value);
    }

    private string? _countText;

    internal string? CountText
    {
        get => _countText;
        private set => SetAndRaise(CountTextProperty, ref _countText, value);
    }

    internal SpaceItemPosition? CompactSpaceItemPosition
    {
        get => GetValue(CompactSpaceItemPositionProperty);
        set => SetValue(CompactSpaceItemPositionProperty, value);
    }

    internal Orientation CompactSpaceOrientation
    {
        get => GetValue(CompactSpaceOrientationProperty);
        set => SetValue(CompactSpaceOrientationProperty, value);
    }

    internal bool IsUsedInCompactSpace
    {
        get => GetValue(IsUsedInCompactSpaceProperty);
        set => SetValue(IsUsedInCompactSpaceProperty, value);
    }

    internal FormValidateFeedback? FormFeedback
    {
        get => GetValue(FormFeedbackProperty);
        private set => SetCurrentValue(FormFeedbackProperty, value);
    }

    internal FormValidateStatus FormStatus
    {
        get => GetValue(FormStatusProperty);
        private set => SetCurrentValue(FormStatusProperty, value);
    }

    private bool _isFormFeedbackVisible;

    internal bool IsFormFeedbackVisible
    {
        get => _isFormFeedbackVisible;
        private set => SetAndRaise(IsFormFeedbackVisibleProperty, ref _isFormFeedbackVisible, value);
    }

    private bool _isPlaceholderTextVisible = true;

    internal bool IsPlaceholderTextVisible
    {
        get => _isPlaceholderTextVisible;
        private set => SetAndRaise(IsPlaceholderTextVisibleProperty, ref _isPlaceholderTextVisible, value);
    }

    #endregion

    #region 内部协作 API

    protected TextPresenter? InputTextPresenter => _textPresenter;

    protected virtual AvaloniaProperty? InnerRightContentTemplatePropertyForBinding => null;

    protected virtual bool CanClearText =>
        !IsReadOnly && !string.IsNullOrEmpty(Text) && (!AcceptsReturn || AllowsClearForMultilineInput);

    /// <summary>
    /// Allows multiline text controls to opt into the same clear semantics as
    /// single-line controls without making the shared layer depend on a concrete type.
    /// </summary>
    protected virtual bool AllowsClearForMultilineInput => false;

    internal void NotifyTextViewportCreated(ScrollViewer scrollViewer)
    {
        SetupTextViewportMetrics(scrollViewer);
    }

    #endregion

    private IconButton? _clearButton;
    private TextPresenter? _textPresenter;
    private InputControlFrame? _inputControlFrame;
    private IDisposable? _preeditTextSubscription;
    private IDisposable? _textViewportSubscription;
    private IDisposable? _feedbackStatusSubscription;
    private EventHandler? _formValueChanged;

    static AbstractTextInput()
    {
        AffectsArrange<AbstractTextInput>(CompactSpaceItemPositionProperty, CompactSpaceOrientationProperty);
        TextChangedEvent.AddClassHandler<AbstractTextInput>((input, _) => input.HandleTextChanged());
    }

    protected AbstractTextInput()
    {
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        if (ClearIcon is null)
        {
            SetCurrentValue(ClearIconProperty, new CloseCircleFilled());
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == AcceptsReturnProperty ||
            change.Property == IsReadOnlyProperty ||
            change.Property == TextProperty ||
            change.Property == IsAllowClearProperty)
        {
            ConfigureEffectiveShowClearButton();
            ConfigurePlaceholderTextVisibility();
        }
        else if (change.Property == IsShowCountProperty)
        {
            HandleInputChanged(Text);
        }
        else if (change.Property == FormFeedbackProperty)
        {
            ConfigureFormFeedbackSubscription();
        }
        else if (change.Property == BorderBrushProperty || change.Property == BackgroundProperty)
        {
            RelayRootSurfaceBrush(change.Property);
        }

        if (change.Property == StatusProperty ||
            change.Property == FormStatusProperty ||
            change.Property == DataValidationErrors.HasErrorsProperty ||
            change.Property == DataValidationErrors.ErrorsProperty)
        {
            UpdateWarningPseudoClass();
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        ReleaseTemplateResources();

        _clearButton = e.NameScope.Find<IconButton>("PART_ClearButton");
        if (_clearButton is not null)
        {
            _clearButton.Click += HandleClearButtonClicked;
        }

        _textPresenter = e.NameScope.Find<TextPresenter>("PART_TextPresenter");
        _preeditTextSubscription?.Dispose();
        _preeditTextSubscription = _textPresenter?.GetObservable(TextPresenter.PreeditTextProperty)
            .Subscribe(_ => ConfigurePlaceholderTextVisibility());

        _inputControlFrame = this.GetVisualDescendants()
                                 .OfType<InputControlFrame>()
                                 .FirstOrDefault();
        RelayRootSurfaceBrush(BorderBrushProperty);
        RelayRootSurfaceBrush(BackgroundProperty);

        SetupTextViewportMetrics(
            e.NameScope.Find<ScrollViewer>("PART_ScrollViewer") ??
            e.NameScope.Find<ScrollViewer>("ScrollViewer"));
        ConfigureEffectiveShowClearButton();
        ConfigurePlaceholderTextVisibility();
        HandleInputChanged(Text);
        UpdateWarningPseudoClass();
    }

    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        ConfigureFormFeedbackSubscription();
    }

    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromLogicalTree(e);
        _feedbackStatusSubscription?.Dispose();
        _feedbackStatusSubscription = null;
    }

    protected virtual void NotifyClearButtonClicked() => Clear();

    protected virtual void NotifySetFormValue(string? value) => SetCurrentValue(TextProperty, value);

    protected virtual string? NotifyGetFormValue() => Text;

    protected virtual void NotifyClearFormValue() => SetCurrentValue(TextProperty, null);

    protected virtual void NotifyValidateStatus(FormValidateStatus status)
    {
        if (FormStatus != status)
        {
            SetCurrentValue(FormStatusProperty, status);
        }
    }

    protected virtual void NotifySetFeedBackControl(FormValidateFeedback? value)
    {
        if (!ReferenceEquals(FormFeedback, value))
        {
            SetCurrentValue(FormFeedbackProperty, value);
        }
    }

    protected virtual double GetBorderThicknessForCompactSpace()
    {
        return CompactSpaceOrientation == Orientation.Horizontal ? BorderThickness.Left : BorderThickness.Top;
    }

    #region 实现 IThemeInheritanceSource 接口

    AvaloniaObject? IThemeInheritanceSource.ThemeInheritanceParent => InheritanceParent;

    #endregion

    #region 实现 ICompactSpaceAware 接口

    void ICompactSpaceAware.NotifyPositionChange(SpaceItemPosition? position)
    {
        SetCurrentValue(IsUsedInCompactSpaceProperty, position is not null);
        SetCurrentValue(CompactSpaceItemPositionProperty, position);
    }

    void ICompactSpaceAware.NotifyOrientationChange(Orientation orientation)
    {
        SetCurrentValue(CompactSpaceOrientationProperty, orientation);
    }

    double ICompactSpaceAware.GetBorderThickness() => GetBorderThicknessForCompactSpace();

    #endregion

    #region 实现 FormItem 接口

    event EventHandler? IFormItemAware.ValueChanged
    {
        add => _formValueChanged += value;
        remove => _formValueChanged -= value;
    }

    void IFormItemAware.SetFormValue(object? value) => NotifySetFormValue(value?.ToString());

    object? IFormItemAware.GetFormValue() => NotifyGetFormValue();

    void IFormItemAware.ClearFormValue() => NotifyClearFormValue();

    void IFormItemAware.NotifyValidateStatus(FormValidateStatus status) => NotifyValidateStatus(status);

    void IFormItemFeedbackAware.SetFeedbackControl(FormValidateFeedback? value) => NotifySetFeedBackControl(value);

    #endregion

    private void HandleTextChanged()
    {
        HandleInputChanged(Text);
        _formValueChanged?.Invoke(this, EventArgs.Empty);
    }

    private void UpdateWarningPseudoClass()
    {
        var isWarning = InputControlState.ResolveEffectiveStatus(this, Status, FormStatus) ==
                        InputControlStatus.Warning;
        PseudoClasses.Set(StdPseudoClass.Warning, isWarning);
    }

    private void HandleInputChanged(string? text)
    {
        if (IsShowCount)
        {
            SetCurrentValue(CountTextProperty, $"{text?.Length ?? 0} / {MaxLength}");
        }
    }

    private void ConfigureEffectiveShowClearButton()
    {
        SetCurrentValue(IsEffectiveShowClearButtonProperty,
            IsAllowClear && CanClearText);
    }

    private void ConfigurePlaceholderTextVisibility()
    {
        SetCurrentValue(IsPlaceholderTextVisibleProperty,
            string.IsNullOrEmpty(Text) && string.IsNullOrEmpty(_textPresenter?.PreeditText));
    }

    private void ConfigureFormFeedbackSubscription()
    {
        _feedbackStatusSubscription?.Dispose();
        _feedbackStatusSubscription = null;
        if (!((ILogical)this).IsAttachedToLogicalTree)
        {
            IsFormFeedbackVisible = false;
            return;
        }

        _feedbackStatusSubscription = FormFeedback?.GetObservable(FormValidateFeedback.ValidateStatusProperty)
            .Subscribe(status => IsFormFeedbackVisible = status != FormValidateStatus.Default);
        if (FormFeedback is null)
        {
            IsFormFeedbackVisible = false;
        }
    }

    private void SetupTextViewportMetrics(ScrollViewer? scrollViewer)
    {
        _textViewportSubscription?.Dispose();
        _textViewportSubscription = null;
        TextViewportMetrics.SetViewportWidth(this, null);
        if (scrollViewer is not null)
        {
            _textViewportSubscription = TextViewportMetrics.PublishViewportWidth(this, scrollViewer, _textPresenter);
        }
    }

    private void HandleClearButtonClicked(object? sender, RoutedEventArgs args)
    {
        NotifyClearButtonClicked();
    }

    private void ReleaseTemplateResources()
    {
        if (_clearButton is not null)
        {
            _clearButton.Click -= HandleClearButtonClicked;
            _clearButton = null;
        }

        _preeditTextSubscription?.Dispose();
        _preeditTextSubscription = null;
        _textViewportSubscription?.Dispose();
        _textViewportSubscription = null;
        TextViewportMetrics.SetViewportWidth(this, null);
        _textPresenter = null;
        _inputControlFrame = null;
    }

    /// <summary>
    /// Relays the owner's root surface brushes onto the input frame as local values so
    /// application-level customization wins over the frame state machine, mirroring antd
    /// inline styles.root semantics. An unset owner value restores the state machine.
    /// </summary>
    // ControlTheme cannot preserve this optional LocalValue contract: on Avalonia 12,
    // an initially-Unset local compiled binding seeds the null metadata default instead
    // of leaving state styles active. Lower binding priorities also change the explicit
    // root-brush-over-hover contract. Keep the conditional write/clear without subscriptions.
    private void RelayRootSurfaceBrush(AvaloniaProperty property)
    {
        if (_inputControlFrame is null)
        {
            return;
        }

        var value = GetValue(property);
        if (value is null || ReferenceEquals(value, AvaloniaProperty.UnsetValue))
        {
            _inputControlFrame.ClearValue(property);
        }
        else
        {
            _inputControlFrame.SetValue(property, value, BindingPriority.LocalValue);
        }
    }
}
