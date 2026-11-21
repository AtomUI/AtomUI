using AtomUI.Controls;
using AtomUI.Icons.AntDesign;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

[global::System.Runtime.Versioning.UnsupportedOSPlatform("browser")]
public partial class OtpLineEdit : AbstractOtpInput,
                                   IMotionAwareControl,
                                   ICustomizableSizeTypeAware,
                                   IInputControlStyleVariantAware,
                                   IFormItemFeedbackAware
{
    #region 公共属性定义

    public new static readonly StyledProperty<string?> TextProperty =
        AbstractOtpInput.TextProperty.AddOwner<OtpLineEdit>();

    public new static readonly StyledProperty<int> LengthProperty =
        AbstractOtpInput.LengthProperty.AddOwner<OtpLineEdit>();

    public new static readonly StyledProperty<OtpLineEditInputMode> InputModeProperty =
        AbstractOtpInput.InputModeProperty.AddOwner<OtpLineEdit>();

    public new static readonly StyledProperty<Func<string, string>?> FormatterProperty =
        AbstractOtpInput.FormatterProperty.AddOwner<OtpLineEdit>();

    public new static readonly StyledProperty<bool> IsMaskedProperty =
        AbstractOtpInput.IsMaskedProperty.AddOwner<OtpLineEdit>();

    public new static readonly StyledProperty<char> MaskCharProperty =
        AbstractOtpInput.MaskCharProperty.AddOwner<OtpLineEdit>();

    public static readonly StyledProperty<string?> PlaceholderTextProperty =
        AvaloniaProperty.Register<OtpLineEdit, string?>(nameof(PlaceholderText));

    public new static readonly StyledProperty<bool> IsReadOnlyProperty =
        AbstractOtpInput.IsReadOnlyProperty.AddOwner<OtpLineEdit>();

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

    public new static readonly StyledProperty<InputControlStatus> StatusProperty =
        AbstractOtpInput.StatusProperty.AddOwner<OtpLineEdit>();

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<OtpLineEdit>();

    public new string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public new int Length
    {
        get => GetValue(LengthProperty);
        set => SetValue(LengthProperty, value);
    }

    public new OtpLineEditInputMode InputMode
    {
        get => GetValue(InputModeProperty);
        set => SetValue(InputModeProperty, value);
    }

    public new Func<string, string>? Formatter
    {
        get => GetValue(FormatterProperty);
        set => SetValue(FormatterProperty, value);
    }

    public new bool IsMasked
    {
        get => GetValue(IsMaskedProperty);
        set => SetValue(IsMaskedProperty, value);
    }

    public new char MaskChar
    {
        get => GetValue(MaskCharProperty);
        set => SetValue(MaskCharProperty, value);
    }

    public string? PlaceholderText
    {
        get => GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }

    public new bool IsReadOnly
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

    public new InputControlStatus Status
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

    public new event EventHandler<OtpLineEditCompletedEventArgs>? Completed
    {
        add => base.Completed += value;
        remove => base.Completed -= value;
    }

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

    internal static readonly StyledProperty<FormValidateFeedback?> FormFeedbackProperty =
        AvaloniaProperty.Register<OtpLineEdit, FormValidateFeedback?>(nameof(FormFeedback));

    internal static readonly DirectProperty<OtpLineEdit, bool> IsFormFeedbackVisibleProperty =
        AvaloniaProperty.RegisterDirect<OtpLineEdit, bool>(
            nameof(IsFormFeedbackVisible),
            o => o.IsFormFeedbackVisible);

    internal static readonly StyledProperty<bool> JoinCellsProperty =
        AvaloniaProperty.Register<OtpLineEdit, bool>(nameof(JoinCells));

    internal static readonly StyledProperty<double> CellGapProperty =
        AvaloniaProperty.Register<OtpLineEdit, double>(nameof(CellGap));

    internal static readonly DirectProperty<OtpLineEdit, double> EffectiveCellGapProperty =
        AvaloniaProperty.RegisterDirect<OtpLineEdit, double>(nameof(EffectiveCellGap), o => o.EffectiveCellGap);

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

    internal FormValidateFeedback? FormFeedback
    {
        get => GetValue(FormFeedbackProperty);
        set => SetValue(FormFeedbackProperty, value);
    }

    private bool _isFormFeedbackVisible;

    internal bool IsFormFeedbackVisible
    {
        get => _isFormFeedbackVisible;
        private set => SetAndRaise(IsFormFeedbackVisibleProperty, ref _isFormFeedbackVisible, value);
    }

    internal bool JoinCells
    {
        get => GetValue(JoinCellsProperty);
        set => SetValue(JoinCellsProperty, value);
    }

    internal double CellGap
    {
        get => GetValue(CellGapProperty);
        set => SetValue(CellGapProperty, value);
    }

    private double _effectiveCellGap;

    internal double EffectiveCellGap
    {
        get => _effectiveCellGap;
        private set => SetAndRaise(EffectiveCellGapProperty, ref _effectiveCellGap, value);
    }

    #endregion

    private IconButton? _clearButton;
    private IDisposable? _feedbackStatusSubscription;

    private bool AreCellsJoined => JoinCells && Separator is null;

    static OtpLineEdit()
    {
        AffectsMeasure<OtpLineEdit>(LengthProperty, SizeTypeProperty);
    }

    public OtpLineEdit()
    {
        AddHandler(TextInputEvent, HandleDescendantTextInput, RoutingStrategies.Bubble, true);
        AddHandler(KeyDownEvent, HandleDescendantKeyDown, RoutingStrategies.Bubble, true);
        AddHandler(RequestBringIntoViewEvent, HandleDescendantRequestBringIntoView, RoutingStrategies.Bubble, true);
        UpdateCellItems();
    }

    public new void Clear() => base.Clear();

    protected override int NormalizeLength(int length) => Math.Max(1, length);

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

        if (change.Property == TextProperty ||
            change.Property == LengthProperty ||
            change.Property == IsMaskedProperty ||
            change.Property == MaskCharProperty ||
            change.Property == PlaceholderTextProperty ||
            change.Property == SeparatorProperty ||
            change.Property == SeparatorTemplateProperty ||
            change.Property == JoinCellsProperty ||
            change.Property == SizeTypeProperty ||
            change.Property == StyleVariantProperty ||
            change.Property == IsReadOnlyProperty ||
            change.Property == IsMotionEnabledProperty ||
            change.Property == EffectiveStatusProperty ||
            change.Property == ActiveIndexProperty ||
            change.Property == IsKeyboardFocusWithinProperty ||
            change.Property == IsEffectivelyEnabledProperty)
        {
            UpdateCellItems();
        }

        if (change.Property == CellGapProperty || change.Property == JoinCellsProperty || change.Property == SeparatorProperty)
        {
            EffectiveCellGap = AreCellsJoined ? 0 : CellGap;
        }

        if (change.Property == TextProperty ||
            change.Property == LengthProperty ||
            change.Property == IsAllowClearProperty ||
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

    protected override void OnTextInput(TextInputEventArgs e) => base.OnTextInput(e);

    protected override void OnKeyDown(KeyEventArgs e) => base.OnKeyDown(e);

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

    private static bool IsCellInputSource(object? source)
    {
        return source is Control control &&
               control.FindAncestorOfType<OtpLineEditCell>(includeSelf: true) is not null;
    }

    private void HandleClearButtonClicked(object? sender, RoutedEventArgs args)
    {
        Clear();
    }

    private void ConfigureEffectiveShowClearButton()
    {
        IsEffectiveShowClearButton = IsAllowClear && !IsReadOnly && !string.IsNullOrEmpty(Text);
    }

    private void UpdateCellItems()
    {
        var text   = Text ?? string.Empty;
        var joined = AreCellsJoined;
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
            item.IsActive           = IsKeyboardFocusWithin && IsEffectivelyEnabled && index == ActiveIndex;
            item.IsInputTarget      = CanEdit() && IsEffectivelyEnabled && index == ActiveIndex;
            item.IsMotionEnabled    = IsMotionEnabled;
            item.EffectiveStatus    = EffectiveStatus;
            item.SizeType           = SizeType;
            item.StyleVariant       = StyleVariant;
            item.Separator          = CreateSeparatorContent(index);
            item.SeparatorTemplate  = SeparatorTemplate;
            item.IsSeparatorVisible = ShouldShowSeparatorAfter(index);
            item.JoinsPreviousCell  = joined && index > 0;
            item.JoinsNextCell      = joined && index < Length - 1;
            item.FocusZIndex        = joined && item.IsActive ? 1 : 0;
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

    void IFormItemFeedbackAware.SetFeedbackControl(FormValidateFeedback? value)
    {
        if (!ReferenceEquals(FormFeedback, value))
        {
            FormFeedback = value;
        }
    }

    #endregion
}
