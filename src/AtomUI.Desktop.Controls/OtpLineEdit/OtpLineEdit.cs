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

/// <summary>
/// One-time password input for the desktop. It presents the code one character per cell, with an optional
/// separator between the cells and an optional clear button beside them; <see cref="AbstractOtpInput" /> owns
/// the text and the editing.
/// </summary>
[global::System.Runtime.Versioning.UnsupportedOSPlatform("browser")]
public partial class OtpLineEdit : AbstractOtpInput,
                                   IMotionAwareControl,
                                   ICustomizableSizeTypeAware,
                                   IInputControlStyleVariantAware,
                                   IFormItemFeedbackAware
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="Text" /> property.
    /// </summary>
    public new static readonly StyledProperty<string?> TextProperty =
        AbstractOtpInput.TextProperty.AddOwner<OtpLineEdit>();

    /// <summary>
    /// Identifies the <see cref="Length" /> property.
    /// </summary>
    public new static readonly StyledProperty<int> LengthProperty =
        AbstractOtpInput.LengthProperty.AddOwner<OtpLineEdit>();

    /// <summary>
    /// Identifies the <see cref="InputMode" /> property.
    /// </summary>
    public new static readonly StyledProperty<OtpLineEditInputMode> InputModeProperty =
        AbstractOtpInput.InputModeProperty.AddOwner<OtpLineEdit>();

    /// <summary>
    /// Identifies the <see cref="Formatter" /> property.
    /// </summary>
    public new static readonly StyledProperty<Func<string, string>?> FormatterProperty =
        AbstractOtpInput.FormatterProperty.AddOwner<OtpLineEdit>();

    /// <summary>
    /// Identifies the <see cref="IsMasked" /> property.
    /// </summary>
    public new static readonly StyledProperty<bool> IsMaskedProperty =
        AbstractOtpInput.IsMaskedProperty.AddOwner<OtpLineEdit>();

    /// <summary>
    /// Identifies the <see cref="MaskChar" /> property.
    /// </summary>
    public new static readonly StyledProperty<char> MaskCharProperty =
        AbstractOtpInput.MaskCharProperty.AddOwner<OtpLineEdit>();

    /// <summary>
    /// Identifies the <see cref="PlaceholderText" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> PlaceholderTextProperty =
        AvaloniaProperty.Register<OtpLineEdit, string?>(nameof(PlaceholderText));

    /// <summary>
    /// Identifies the <see cref="IsReadOnly" /> property.
    /// </summary>
    public new static readonly StyledProperty<bool> IsReadOnlyProperty =
        AbstractOtpInput.IsReadOnlyProperty.AddOwner<OtpLineEdit>();

    /// <summary>
    /// Identifies the <see cref="IsAllowClear" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsAllowClearProperty =
        AvaloniaProperty.Register<OtpLineEdit, bool>(nameof(IsAllowClear));

    /// <summary>
    /// Identifies the <see cref="ClearIcon" /> property.
    /// </summary>
    public static readonly StyledProperty<PathIcon?> ClearIconProperty =
        AvaloniaProperty.Register<OtpLineEdit, PathIcon?>(nameof(ClearIcon));

    /// <summary>
    /// Identifies the <see cref="Separator" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> SeparatorProperty =
        AvaloniaProperty.Register<OtpLineEdit, object?>(nameof(Separator));

    /// <summary>
    /// Identifies the <see cref="SeparatorTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> SeparatorTemplateProperty =
        AvaloniaProperty.Register<OtpLineEdit, IDataTemplate?>(nameof(SeparatorTemplate));

    /// <summary>
    /// Identifies the <see cref="CellWidth" /> property.
    /// </summary>
    public static readonly StyledProperty<double?> CellWidthProperty =
        AvaloniaProperty.Register<OtpLineEdit, double?>(nameof(CellWidth));

    /// <summary>
    /// Identifies the <see cref="CellBorderBrush" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> CellBorderBrushProperty =
        AvaloniaProperty.Register<OtpLineEdit, IBrush?>(nameof(CellBorderBrush));

    /// <summary>
    /// Identifies the <see cref="SizeType" /> property.
    /// </summary>
    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<OtpLineEdit>();

    /// <summary>
    /// Identifies the <see cref="StyleVariant" /> property.
    /// </summary>
    public static readonly StyledProperty<InputControlStyleVariant> StyleVariantProperty =
        InputControlStyleVariantProperty.StyleVariantProperty.AddOwner<OtpLineEdit>();

    /// <summary>
    /// Identifies the <see cref="Status" /> property.
    /// </summary>
    public new static readonly StyledProperty<InputControlStatus> StatusProperty =
        AbstractOtpInput.StatusProperty.AddOwner<OtpLineEdit>();

    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<OtpLineEdit>();

    /// <summary>
    /// The code as a single string, one character per cell. An assigned value passes through
    /// <see cref="Formatter" />, loses the characters that <see cref="InputMode" /> does not allow and is cut to
    /// <see cref="Length" /> characters.
    /// </summary>
    public new string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>
    /// Number of character cells. An assigned value below 1 becomes 1, where the base class would use 6. Text
    /// longer than the new value is cut, and shortening the value to the length of the text raises
    /// <see cref="Completed" />.
    /// </summary>
    public new int Length
    {
        get => GetValue(LengthProperty);
        set => SetValue(LengthProperty, value);
    }

    /// <summary>
    /// Character set that <see cref="Text" /> is restricted to. Changing it normalizes the current text, so
    /// characters that are no longer allowed are removed.
    /// </summary>
    public new OtpLineEditInputMode InputMode
    {
        get => GetValue(InputModeProperty);
        set => SetValue(InputModeProperty, value);
    }

    /// <summary>
    /// Function that transforms text before it is stored in <see cref="Text" />, when <see cref="Text" /> is
    /// assigned and when the user types or pastes. Its result is still filtered by <see cref="InputMode" /> and
    /// cut to <see cref="Length" />. <see langword="null" />, the default, stores text unchanged.
    /// </summary>
    public new Func<string, string>? Formatter
    {
        get => GetValue(FormatterProperty);
        set => SetValue(FormatterProperty, value);
    }

    /// <summary>
    /// <see langword="true" /> to show <see cref="MaskChar" /> in every filled cell instead of the character.
    /// </summary>
    public new bool IsMasked
    {
        get => GetValue(IsMaskedProperty);
        set => SetValue(IsMaskedProperty, value);
    }

    /// <summary>
    /// Character shown in filled cells while <see cref="IsMasked" /> is <see langword="true" />. The default is
    /// a bullet (U+2022).
    /// </summary>
    public new char MaskChar
    {
        get => GetValue(MaskCharProperty);
        set => SetValue(MaskCharProperty, value);
    }

    /// <summary>
    /// Hint shown in the cells that hold no character. A string with exactly <see cref="Length" /> characters puts
    /// one character in each cell; any other non-empty string is shown in every empty cell.
    /// <see langword="null" /> or an empty string shows no hint.
    /// </summary>
    public string? PlaceholderText
    {
        get => GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }

    /// <summary>
    /// <see langword="true" /> stops keyboard editing: typing, Backspace and Delete are ignored, and the clear
    /// button is not shown, while <see cref="Text" /> can still be assigned.
    /// </summary>
    public new bool IsReadOnly
    {
        get => GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    /// <summary>
    /// <see langword="true" /> shows a clear button beside the cells while <see cref="Text" /> is not null or
    /// empty. The button stays hidden while <see cref="IsReadOnly" /> is <see langword="true" />. Clicking it calls
    /// <see cref="Clear" />.
    /// </summary>
    public bool IsAllowClear
    {
        get => GetValue(IsAllowClearProperty);
        set => SetValue(IsAllowClearProperty, value);
    }

    /// <summary>
    /// Icon of the clear button. When it is still <see langword="null" /> when the control is initialized, a
    /// <c>CloseCircleFilled</c> icon is set as the value instead.
    /// </summary>
    public PathIcon? ClearIcon
    {
        get => GetValue(ClearIconProperty);
        set => SetValue(ClearIconProperty, value);
    }

    /// <summary>
    /// Content shown between two adjacent cells, or <see langword="null" />, the default, to show no separator.
    /// It is shown after every cell except the last. Without a separator the theme joins the cells without a gap
    /// when the <c>JoinCells</c> token is on; with one the cells are spaced by the <c>CellGap</c> token. With a
    /// <see cref="SeparatorTemplate" /> the content is wrapped in a separator context for the template.
    /// </summary>
    public object? Separator
    {
        get => GetValue(SeparatorProperty);
        set => SetValue(SeparatorProperty, value);
    }

    /// <summary>
    /// Template that presents the <see cref="Separator" /> content of every separator, or <see langword="null" />,
    /// the default, to present the content itself. Its data context is an <c>OtpLineEditSeparatorContext</c> that
    /// holds the content and the zero-based indexes of the cells before and after the separator.
    /// </summary>
    public IDataTemplate? SeparatorTemplate
    {
        get => GetValue(SeparatorTemplateProperty);
        set => SetValue(SeparatorTemplateProperty, value);
    }

    /// <summary>
    /// Width of every character cell, in device-independent pixels. <see langword="null" />, the default, takes
    /// the width of the <see cref="SizeType" />: the <c>CellWidthLG</c> token for Large, <c>CellWidthSM</c> for
    /// Small and the <c>CellWidth</c> token for Middle and Custom.
    /// </summary>
    public double? CellWidth
    {
        get => GetValue(CellWidthProperty);
        set => SetValue(CellWidthProperty, value);
    }

    /// <summary>
    /// Border of every character cell. The cell theme applies it over the border brush of every state, and it wins
    /// over the <c>BorderBrush</c> of the control. When both are <see langword="null" />, the default, the theme
    /// paints the border of each cell by its state.
    /// </summary>
    public IBrush? CellBorderBrush
    {
        get => GetValue(CellBorderBrushProperty);
        set => SetValue(CellBorderBrushProperty, value);
    }

    /// <summary>
    /// Size of the character cells. Every cell takes its width, height and font size from it. With
    /// <c>Custom</c> the cells keep the size of <c>Middle</c> unless <see cref="CellWidth" /> is set.
    /// </summary>
    public CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    /// <summary>
    /// Frame style of the character cells: outlined, filled, borderless or underlined. Every cell follows it.
    /// </summary>
    public InputControlStyleVariant StyleVariant
    {
        get => GetValue(StyleVariantProperty);
        set => SetValue(StyleVariantProperty, value);
    }

    /// <summary>
    /// Validation state painted on the cells: <see cref="InputControlStatus.Warning" /> or
    /// <see cref="InputControlStatus.Error" /> colors them accordingly, and
    /// <see cref="InputControlStatus.Default" /> leaves them neutral. Data validation errors and an error reported
    /// by the form take precedence and force the error state; a warning reported by the form forces the warning
    /// state.
    /// </summary>
    public new InputControlStatus Status
    {
        get => GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    /// <summary>
    /// Whether the cells and the clear button play their motion. The theme sets it from the
    /// <c>EnableMotion</c> token; <see langword="false" /> turns the motion off.
    /// </summary>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    #endregion

    #region 公共事件定义

    /// <summary>
    /// Raised after <see cref="Text" /> reaches <see cref="Length" /> characters while it was not complete
    /// before. It is not raised again until the text is shorter once more. For the same change it is raised
    /// before the <see cref="AbstractOtpInput.TextChanged" /> event.
    /// </summary>
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

    /// <summary>
    /// Sets <see cref="Text" /> to <see langword="null" />, which empties every cell and hides the clear button.
    /// </summary>
    public new void Clear() => base.Clear();

    /// <summary>
    /// Clamps every value assigned to <see cref="Length" /> to at least 1, so the input keeps one cell. This
    /// override does not call the base method, which replaces values of 0 or less with the default of 6.
    /// </summary>
    /// <param name="length">
    /// The value assigned to <see cref="Length" />.
    /// </param>
    /// <returns>
    /// The value to store in <see cref="Length" />.
    /// </returns>
    protected override int NormalizeLength(int length) => Math.Max(1, length);

    /// <summary>
    /// Supplies a default for <see cref="ClearIcon" /> once the base initialization has run: a new
    /// <c>CloseCircleFilled</c> when the property is still <see langword="null" />. The value is set with
    /// <c>SetCurrentValue</c>, so a binding on the property is kept.
    /// </summary>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        if (ClearIcon == null)
        {
            SetCurrentValue(ClearIconProperty, new CloseCircleFilled());
        }
    }

    /// <summary>
    /// Calls the base method first and then keeps the derived state in step with the changed property. A
    /// change of <see cref="Text" />, <see cref="Length" /> or of any property that feeds the cell items
    /// (placeholder, mask, separators, cell joining, size, style, read-only state, motion, status, active
    /// cell, focus, enabled state) refreshes every cell item. A change of <see cref="CellGap" />,
    /// <see cref="JoinCells" /> or <c>Separator</c> sets <see cref="EffectiveCellGap" /> to 0 when
    /// <see cref="JoinCells" /> is <see langword="true" /> and no <c>Separator</c> is set, and to
    /// <see cref="CellGap" /> otherwise. A change of <see cref="Text" />, <see cref="Length" />,
    /// <see cref="IsAllowClear" /> or <c>IsReadOnly</c> recalculates
    /// <see cref="IsEffectiveShowClearButton" />. A change of <see cref="FormFeedback" /> re-creates the
    /// subscription to the validation status of the feedback.
    /// </summary>
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

    /// <summary>
    /// Refreshes every cell item after the base method has handled the event, so that the active state of the
    /// cells, which depends on whether the focus is inside the control, follows the focus just received.
    /// </summary>
    protected override void OnGotFocus(FocusChangedEventArgs e)
    {
        base.OnGotFocus(e);

        UpdateCellItems();
    }

    /// <summary>
    /// Connects the clear button of the applied template to the control after the base call: the <c>Click</c>
    /// handler is removed from the clear button of the previous template, <c>PART_ClearButton</c> is looked up
    /// in the new template and, when found, a click on it clears the input. Whether the clear button is shown
    /// and the cell items are updated afterwards.
    /// </summary>
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

    /// <summary>
    /// Calls the base method, then subscribes to the validation status of <see cref="FormFeedback" />, which
    /// drives <see cref="IsFormFeedbackVisible" />. The subscription is created only while the control is
    /// attached to a logical tree; while no feedback is set, <see cref="IsFormFeedbackVisible" /> is set to
    /// <see langword="false" />.
    /// </summary>
    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        ConfigureFormFeedbackSubscription();
    }

    /// <summary>
    /// Disposes the subscription to the validation status of <see cref="FormFeedback" /> after the base call,
    /// so a detached control no longer reacts to status changes; <see cref="IsFormFeedbackVisible" /> keeps
    /// its last value. The subscription is created again when the control is attached to a logical tree.
    /// </summary>
    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromLogicalTree(e);
        _feedbackStatusSubscription?.Dispose();
        _feedbackStatusSubscription = null;
    }

    /// <inheritdoc />
    protected override void OnTextInput(TextInputEventArgs e) => base.OnTextInput(e);

    /// <inheritdoc />
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
