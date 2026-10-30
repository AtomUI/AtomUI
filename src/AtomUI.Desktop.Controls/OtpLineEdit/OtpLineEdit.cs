using AtomUI.Controls;
using AtomUI.Icons.AntDesign;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

[global::System.Runtime.Versioning.UnsupportedOSPlatform("browser")]
public partial class OtpLineEdit : TemplatedControl,
                                   IMotionAwareControl,
                                   ICustomizableSizeTypeAware,
                                   IInputControlStyleVariantAware,
                                   IInputControlStatusAware,
                                   IFormItemAware,
                                   IFormItemFeedbackAware
{
    #region 公共属性定义

    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<OtpLineEdit, string?>(
            nameof(Text),
            defaultBindingMode: BindingMode.TwoWay,
            coerce: CoerceText);

    public static readonly StyledProperty<int> LengthProperty =
        AvaloniaProperty.Register<OtpLineEdit, int>(
            nameof(Length),
            6,
            coerce: (_, value) => Math.Max(1, value));

    public static readonly StyledProperty<OtpLineEditInputMode> InputModeProperty =
        AvaloniaProperty.Register<OtpLineEdit, OtpLineEditInputMode>(nameof(InputMode));

    public static readonly StyledProperty<Func<string, string>?> FormatterProperty =
        AvaloniaProperty.Register<OtpLineEdit, Func<string, string>?>(nameof(Formatter));

    public static readonly StyledProperty<bool> IsMaskedProperty =
        AvaloniaProperty.Register<OtpLineEdit, bool>(nameof(IsMasked));

    public static readonly StyledProperty<char> MaskCharProperty =
        AvaloniaProperty.Register<OtpLineEdit, char>(nameof(MaskChar), '•');

    public static readonly StyledProperty<string?> PlaceholderTextProperty =
        AvaloniaProperty.Register<OtpLineEdit, string?>(nameof(PlaceholderText));

    public static readonly StyledProperty<bool> IsReadOnlyProperty =
        AvaloniaProperty.Register<OtpLineEdit, bool>(nameof(IsReadOnly));

    public static readonly StyledProperty<bool> IsAllowClearProperty =
        AvaloniaProperty.Register<OtpLineEdit, bool>(nameof(IsAllowClear));

    public static readonly StyledProperty<PathIcon?> ClearIconProperty =
        AvaloniaProperty.Register<OtpLineEdit, PathIcon?>(nameof(ClearIcon));

    public static readonly StyledProperty<object?> SeparatorProperty =
        AvaloniaProperty.Register<OtpLineEdit, object?>(nameof(Separator));

    public static readonly StyledProperty<IDataTemplate?> SeparatorTemplateProperty =
        AvaloniaProperty.Register<OtpLineEdit, IDataTemplate?>(nameof(SeparatorTemplate));

    public static readonly StyledProperty<double?> CellWidthProperty =
        AvaloniaProperty.Register<OtpLineEdit, double?>(nameof(CellWidth));

    public static readonly StyledProperty<IBrush?> CellBorderBrushProperty =
        AvaloniaProperty.Register<OtpLineEdit, IBrush?>(nameof(CellBorderBrush));

    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<OtpLineEdit>();

    public static readonly StyledProperty<InputControlStyleVariant> StyleVariantProperty =
        InputControlStyleVariantProperty.StyleVariantProperty.AddOwner<OtpLineEdit>();

    public static readonly StyledProperty<InputControlStatus> StatusProperty =
        InputControlStatusProperty.StatusProperty.AddOwner<OtpLineEdit>();

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<OtpLineEdit>();

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public int Length
    {
        get => GetValue(LengthProperty);
        set => SetValue(LengthProperty, value);
    }

    public OtpLineEditInputMode InputMode
    {
        get => GetValue(InputModeProperty);
        set => SetValue(InputModeProperty, value);
    }

    public Func<string, string>? Formatter
    {
        get => GetValue(FormatterProperty);
        set => SetValue(FormatterProperty, value);
    }

    public bool IsMasked
    {
        get => GetValue(IsMaskedProperty);
        set => SetValue(IsMaskedProperty, value);
    }

    public char MaskChar
    {
        get => GetValue(MaskCharProperty);
        set => SetValue(MaskCharProperty, value);
    }

    public string? PlaceholderText
    {
        get => GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }

    public bool IsReadOnly
    {
        get => GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    public bool IsAllowClear
    {
        get => GetValue(IsAllowClearProperty);
        set => SetValue(IsAllowClearProperty, value);
    }

    public PathIcon? ClearIcon
    {
        get => GetValue(ClearIconProperty);
        set => SetValue(ClearIconProperty, value);
    }

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

    public double? CellWidth
    {
        get => GetValue(CellWidthProperty);
        set => SetValue(CellWidthProperty, value);
    }

    public IBrush? CellBorderBrush
    {
        get => GetValue(CellBorderBrushProperty);
        set => SetValue(CellBorderBrushProperty, value);
    }

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

    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    #endregion

    #region 公共事件定义

    public event EventHandler<OtpLineEditCompletedEventArgs>? Completed;

    #endregion

    #region 内部属性定义

    internal static readonly DirectProperty<OtpLineEdit, IReadOnlyList<OtpLineEditCellInfo>> CellItemsProperty =
        AvaloniaProperty.RegisterDirect<OtpLineEdit, IReadOnlyList<OtpLineEditCellInfo>>(
            nameof(CellItems),
            o => o.CellItems);

    internal static readonly DirectProperty<OtpLineEdit, bool> IsEffectiveShowClearButtonProperty =
        AvaloniaProperty.RegisterDirect<OtpLineEdit, bool>(
            nameof(IsEffectiveShowClearButton),
            o => o.IsEffectiveShowClearButton);

    internal static readonly DirectProperty<OtpLineEdit, InputControlStatus> EffectiveStatusProperty =
        AvaloniaProperty.RegisterDirect<OtpLineEdit, InputControlStatus>(
            nameof(EffectiveStatus),
            o => o.EffectiveStatus);

    internal static readonly StyledProperty<FormValidateFeedback?> FormFeedbackProperty =
        AvaloniaProperty.Register<OtpLineEdit, FormValidateFeedback?>(nameof(FormFeedback));

    internal static readonly StyledProperty<FormValidateStatus> FormStatusProperty =
        InputControlState.FormStatusProperty.AddOwner<OtpLineEdit>();

    internal static readonly DirectProperty<OtpLineEdit, bool> IsFormFeedbackVisibleProperty =
        AvaloniaProperty.RegisterDirect<OtpLineEdit, bool>(
            nameof(IsFormFeedbackVisible),
            o => o.IsFormFeedbackVisible);

    private readonly AvaloniaList<OtpLineEditCellInfo> _cellItems = [];

    internal IReadOnlyList<OtpLineEditCellInfo> CellItems
    {
        get => _cellItems;
    }

    private bool _isEffectiveShowClearButton;

    internal bool IsEffectiveShowClearButton
    {
        get => _isEffectiveShowClearButton;
        private set => SetAndRaise(IsEffectiveShowClearButtonProperty, ref _isEffectiveShowClearButton, value);
    }

    private InputControlStatus _effectiveStatus;

    internal InputControlStatus EffectiveStatus
    {
        get => _effectiveStatus;
        private set => SetAndRaise(EffectiveStatusProperty, ref _effectiveStatus, value);
    }

    internal FormValidateFeedback? FormFeedback
    {
        get => GetValue(FormFeedbackProperty);
        set => SetValue(FormFeedbackProperty, value);
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

    #endregion

    private bool _isCompleted;
    private int _activeIndex;
    private IconButton? _clearButton;
    private IDisposable? _feedbackStatusSubscription;
    private EventHandler? _formValueChanged;

    static OtpLineEdit()
    {
        AffectsMeasure<OtpLineEdit>(LengthProperty, SizeTypeProperty);
        FocusableProperty.OverrideDefaultValue<OtpLineEdit>(true);
    }

    public OtpLineEdit()
    {
        AddHandler(TextInputEvent, HandleDescendantTextInput, RoutingStrategies.Bubble, true);
        AddHandler(KeyDownEvent, HandleDescendantKeyDown, RoutingStrategies.Bubble, true);
        AddHandler(RequestBringIntoViewEvent, HandleDescendantRequestBringIntoView, RoutingStrategies.Bubble, true);
        UpdateEffectiveStatus();
        UpdateCellItems();
    }

    public void Clear()
    {
        SetCurrentValue(TextProperty, null);
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        if (ClearIcon == null)
        {
            SetCurrentValue(ClearIconProperty, new CloseCircleFilled());
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == LengthProperty ||
            change.Property == InputModeProperty ||
            change.Property == FormatterProperty)
        {
            CoerceValue(TextProperty);
        }

        if (change.Property == TextProperty ||
            change.Property == LengthProperty)
        {
            ConfigureActiveIndexForValueChange();
            UpdateCompletedState();
            UpdateValuePseudoClasses();
            ConfigureEffectiveShowClearButton();
        }

        if (change.Property == TextProperty)
        {
            _formValueChanged?.Invoke(this, EventArgs.Empty);
        }

        if (change.Property == StatusProperty ||
            change.Property == FormStatusProperty ||
            change.Property == DataValidationErrors.HasErrorsProperty ||
            change.Property == DataValidationErrors.ErrorsProperty)
        {
            UpdateEffectiveStatus();
        }

        if (change.Property == IsKeyboardFocusWithinProperty)
        {
            if (change.GetNewValue<bool>())
            {
                SetActiveIndexToFirstEmptyCell();
            }

            UpdateCellItems();
        }

        if (change.Property == IsEffectivelyEnabledProperty)
        {
            UpdateCellItems();
        }

        if (change.Property == TextProperty ||
            change.Property == LengthProperty ||
            change.Property == IsMaskedProperty ||
            change.Property == MaskCharProperty ||
            change.Property == PlaceholderTextProperty ||
            change.Property == SeparatorProperty ||
            change.Property == SeparatorTemplateProperty ||
            change.Property == SizeTypeProperty ||
            change.Property == StyleVariantProperty ||
            change.Property == IsReadOnlyProperty ||
            change.Property == IsMotionEnabledProperty ||
            change.Property == EffectiveStatusProperty)
        {
            UpdateCellItems();
        }

        if (change.Property == IsAllowClearProperty ||
            change.Property == IsReadOnlyProperty)
        {
            ConfigureEffectiveShowClearButton();
        }

        if (change.Property == FormFeedbackProperty)
        {
            ConfigureFormFeedbackSubscription();
        }
    }

    protected override void OnGotFocus(FocusChangedEventArgs e)
    {
        base.OnGotFocus(e);

        // 聚焦时活动格定位到第一空格（全满则为最后一格），
        // 保证聚焦即见光标，而不是停留在陈旧位置
        SetActiveIndexToFirstEmptyCell();
        UpdateCellItems();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (_clearButton is not null)
        {
            _clearButton.Click -= HandleClearButtonClicked;
        }

        _clearButton = e.NameScope.Find<IconButton>("PART_ClearButton");
        if (_clearButton is not null)
        {
            _clearButton.Click += HandleClearButtonClicked;
        }

        ConfigureEffectiveShowClearButton();
        UpdateEffectiveStatus();
        UpdateCellItems();
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

    protected override void OnTextInput(TextInputEventArgs e)
    {
        if (!ReferenceEquals(e.Source, this))
        {
            base.OnTextInput(e);
            return;
        }

        if (!TryHandleTextInput(e))
        {
            base.OnTextInput(e);
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (!ReferenceEquals(e.Source, this))
        {
            base.OnKeyDown(e);
            return;
        }

        if (!TryHandleKeyDown(e))
        {
            base.OnKeyDown(e);
        }
    }

    private void HandleDescendantTextInput(object? sender, TextInputEventArgs e)
    {
        if (ReferenceEquals(e.Source, this) || !IsCellInputSource(e.Source))
        {
            return;
        }

        TryHandleTextInput(e);
    }

    private void HandleDescendantKeyDown(object? sender, KeyEventArgs e)
    {
        if (ReferenceEquals(e.Source, this) || !IsCellInputSource(e.Source))
        {
            return;
        }

        TryHandleKeyDown(e);
    }

    private void HandleDescendantRequestBringIntoView(object? sender, RequestBringIntoViewEventArgs e)
    {
        if (!ReferenceEquals(e.TargetObject, this) && IsCellInputSource(e.TargetObject))
        {
            e.Handled = true;
        }
    }

    private bool TryHandleTextInput(TextInputEventArgs e)
    {
        if (!CanEdit() || string.IsNullOrEmpty(e.Text))
        {
            return false;
        }

        ReplaceTextFromActiveIndex(e.Text);
        e.Handled = true;
        return true;
    }

    private bool TryHandleKeyDown(KeyEventArgs e)
    {
        if (!CanEdit())
        {
            return false;
        }

        switch (e.Key)
        {
            case Key.Back:
                RemoveWithBackspace();
                e.Handled = true;
                return true;
            case Key.Delete:
                RemoveAtActiveIndex();
                e.Handled = true;
                return true;
            case Key.Left:
                MoveActiveIndex(-1);
                e.Handled = true;
                return true;
            case Key.Right:
                MoveActiveIndex(1);
                e.Handled = true;
                return true;
            case Key.Home:
                _activeIndex = 0;
                e.Handled    = true;
                return true;
            case Key.End:
                MoveActiveIndexToFirstEmptyCell();
                e.Handled = true;
                return true;
            default:
                return false;
        }
    }

    private static bool IsCellInputSource(object? source)
    {
        return source is Control control &&
               control.FindAncestorOfType<OtpLineEditCell>(includeSelf: true) is not null;
    }

    private static string? CoerceText(AvaloniaObject sender, string? value)
    {
        var otpLineEdit = (OtpLineEdit)sender;
        return otpLineEdit.NormalizeText(value);
    }

    private string? NormalizeText(string? value)
    {
        if (value is null)
        {
            return null;
        }

        var normalized = Formatter?.Invoke(value) ?? value;
        normalized = FilterByInputMode(normalized);

        if (normalized.Length > Length)
        {
            normalized = normalized[..Length];
        }

        return normalized;
    }

    private string FilterByInputMode(string value)
    {
        return InputMode switch
        {
            OtpLineEditInputMode.Numeric      => new string(value.Where(char.IsDigit).ToArray()),
            OtpLineEditInputMode.AlphaNumeric => new string(value.Where(char.IsLetterOrDigit).ToArray()),
            _                                 => value
        };
    }

    private void UpdateCompletedState()
    {
        var isCompleted = Text?.Length == Length;
        if (isCompleted && !_isCompleted && Text is { } text)
        {
            Completed?.Invoke(this, new OtpLineEditCompletedEventArgs(text));
        }

        _isCompleted = isCompleted;
    }

    private bool CanEdit()
    {
        return IsEnabled && !IsReadOnly;
    }

    private void ReplaceTextFromActiveIndex(string text)
    {
        var current    = Text ?? string.Empty;
        var startIndex = Math.Clamp(_activeIndex, 0, Length - 1);
        var before     = startIndex < current.Length ? current[..startIndex] : current;
        var afterStart = startIndex + text.Length;
        var after      = afterStart < current.Length ? current[afterStart..] : string.Empty;

        SetCurrentValue(TextProperty, before + text + after);
        MoveActiveIndexToFirstEmptyCell();
    }

    private void RemoveWithBackspace()
    {
        var current = Text;
        if (string.IsNullOrEmpty(current))
        {
            return;
        }

        // 删除后光标留在被删格（该格已空），继续退格时前移删除
        // 前一个字符
        var activeIndex      = Math.Clamp(_activeIndex, 0, Length - 1);
        var removeIndex      = Math.Min(activeIndex, current.Length - 1);

        _activeIndex = Math.Clamp(removeIndex, 0, Length - 1);
        SetCurrentValue(TextProperty, RemoveAt(current, removeIndex));
    }

    private void RemoveAtActiveIndex()
    {
        var current = Text;
        if (string.IsNullOrEmpty(current) || _activeIndex >= current.Length)
        {
            return;
        }

        var removeIndex     = Math.Clamp(_activeIndex, 0, current.Length - 1);
        var nextText        = RemoveAt(current, removeIndex);
        var nextTextLength  = nextText?.Length ?? 0;
        var nextActiveIndex = nextTextLength == 0
            ? 0
            : Math.Min(removeIndex, nextTextLength - 1);

        _activeIndex = Math.Clamp(nextActiveIndex, 0, Length - 1);
        SetCurrentValue(TextProperty, nextText);
    }

    private static string? RemoveAt(string value, int index)
    {
        var result = value.Remove(index, 1);
        return result.Length == 0 ? null : result;
    }

    private void MoveActiveIndex(int delta)
    {
        var upperBound = GetActiveIndexUpperBound();
        _activeIndex = Math.Clamp(_activeIndex + delta, 0, upperBound);
        UpdateCellItems();
    }

    private void MoveActiveIndexToFirstEmptyCell()
    {
        SetActiveIndexToFirstEmptyCell();
        UpdateCellItems();
    }

    private void ConfigureActiveIndexForValueChange()
    {
        if (IsKeyboardFocusWithin)
        {
            ClampActiveIndexToValueBoundary();
        }
        else
        {
            SetActiveIndexToFirstEmptyCell();
        }
    }

    private void SetActiveIndexToFirstEmptyCell()
    {
        _activeIndex = GetActiveIndexUpperBound();
    }

    private void ClampActiveIndexToValueBoundary()
    {
        _activeIndex = Math.Clamp(_activeIndex, 0, GetActiveIndexUpperBound());
    }

    private int GetActiveIndexUpperBound()
    {
        return Math.Clamp(Text?.Length ?? 0, 0, Length - 1);
    }

    private void HandleClearButtonClicked(object? sender, RoutedEventArgs args)
    {
        Clear();
    }

    private void ConfigureEffectiveShowClearButton()
    {
        IsEffectiveShowClearButton = IsAllowClear && !IsReadOnly && !string.IsNullOrEmpty(Text);
    }

    private void UpdateEffectiveStatus()
    {
        EffectiveStatus = InputControlState.ResolveEffectiveStatus(this, Status, FormStatus);

        PseudoClasses.Set(StdPseudoClass.Warning, EffectiveStatus == InputControlStatus.Warning);
        UpdateValuePseudoClasses();
    }

    private void UpdateValuePseudoClasses()
    {
        PseudoClasses.Set(":filled", Text?.Length == Length);
        PseudoClasses.Set(":empty", string.IsNullOrEmpty(Text));
    }

    private void UpdateCellItems()
    {
        var text  = Text ?? string.Empty;
        SyncCellItemCount();

        for (var index = 0; index < Length; index++)
        {
            var hasCharacter = index < text.Length;
            var displayText  = hasCharacter
                ? IsMasked ? MaskChar.ToString() : text[index].ToString()
                : null;
            var item = _cellItems[index];

            item.DisplayText        = displayText;
            item.PlaceholderText    = GetCellPlaceholder(index);
            item.IsActive           = IsKeyboardFocusWithin && IsEffectivelyEnabled && index == _activeIndex;
            item.IsInputTarget      = CanEdit() && IsEffectivelyEnabled && index == _activeIndex;
            item.IsMotionEnabled    = IsMotionEnabled;
            item.EffectiveStatus    = EffectiveStatus;
            item.SizeType           = SizeType;
            item.StyleVariant       = StyleVariant;
            item.Separator          = CreateSeparatorContent(index);
            item.SeparatorTemplate  = SeparatorTemplate;
            item.IsSeparatorVisible = ShouldShowSeparatorAfter(index);
        }
    }

    private void SyncCellItemCount()
    {
        while (_cellItems.Count < Length)
        {
            _cellItems.Add(new OtpLineEditCellInfo());
        }

        while (_cellItems.Count > Length)
        {
            _cellItems.RemoveAt(_cellItems.Count - 1);
        }
    }

    private string? GetCellPlaceholder(int index)
    {
        if (string.IsNullOrEmpty(PlaceholderText))
        {
            return null;
        }

        return PlaceholderText.Length == Length ? PlaceholderText[index].ToString() : PlaceholderText;
    }

    private object? CreateSeparatorContent(int index)
    {
        if (!ShouldShowSeparatorAfter(index))
        {
            return null;
        }

        return SeparatorTemplate is null
            ? Separator
            : new OtpLineEditSeparatorContext(index, index + 1, Separator, SeparatorTemplate);
    }

    private bool ShouldShowSeparatorAfter(int index)
    {
        return Separator is not null && index < Length - 1;
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

        if (FormFeedback is { } feedback)
        {
            _feedbackStatusSubscription = feedback.GetObservable(FormValidateFeedback.ValidateStatusProperty)
                                                  .Subscribe(status => IsFormFeedbackVisible = status != FormValidateStatus.Default);
        }
        else
        {
            IsFormFeedbackVisible = false;
        }
    }

    #region 实现 FormItem 接口

    event EventHandler? IFormItemAware.ValueChanged
    {
        add => _formValueChanged += value;
        remove => _formValueChanged -= value;
    }

    void IFormItemAware.SetFormValue(object? value)
    {
        SetCurrentValue(TextProperty, value?.ToString());
    }

    object? IFormItemAware.GetFormValue()
    {
        return Text;
    }

    void IFormItemAware.ClearFormValue()
    {
        Clear();
    }

    void IFormItemAware.NotifyValidateStatus(FormValidateStatus status)
    {
        SetCurrentValue(FormStatusProperty, status);
    }

    void IFormItemFeedbackAware.SetFeedbackControl(FormValidateFeedback? value)
    {
        if (!ReferenceEquals(FormFeedback, value))
        {
            FormFeedback = value;
        }
    }

    #endregion
}
