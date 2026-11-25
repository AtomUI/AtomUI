using AtomUI.Desktop.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace AtomUI.Controls;

public abstract class AbstractOtpInput : TemplatedControl, IInputControlStatusAware, IFormItemAware
{
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<AbstractOtpInput, string?>(
            nameof(Text),
            defaultBindingMode: BindingMode.TwoWay,
            coerce: CoerceText);

    public static readonly StyledProperty<int> LengthProperty =
        AvaloniaProperty.Register<AbstractOtpInput, int>(
            nameof(Length),
            6,
            coerce: (owner, value) => ((AbstractOtpInput)owner).NormalizeLength(value));

    public static readonly StyledProperty<OtpLineEditInputMode> InputModeProperty =
        AvaloniaProperty.Register<AbstractOtpInput, OtpLineEditInputMode>(nameof(InputMode));

    public static readonly StyledProperty<Func<string, string>?> FormatterProperty =
        AvaloniaProperty.Register<AbstractOtpInput, Func<string, string>?>(nameof(Formatter));

    public static readonly StyledProperty<bool> IsMaskedProperty =
        AvaloniaProperty.Register<AbstractOtpInput, bool>(nameof(IsMasked));

    public static readonly StyledProperty<char> MaskCharProperty =
        AvaloniaProperty.Register<AbstractOtpInput, char>(nameof(MaskChar), '•');

    public static readonly StyledProperty<bool> IsReadOnlyProperty =
        AvaloniaProperty.Register<AbstractOtpInput, bool>(nameof(IsReadOnly));

    public static readonly StyledProperty<InputControlStatus> StatusProperty =
        InputControlStatusProperty.StatusProperty.AddOwner<AbstractOtpInput>();

    public static readonly RoutedEvent<TextChangedEventArgs> TextChangedEvent =
        RoutedEvent.Register<AbstractOtpInput, TextChangedEventArgs>(nameof(TextChanged), RoutingStrategies.Bubble);

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

    public bool IsReadOnly
    {
        get => GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    public InputControlStatus Status
    {
        get => GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    protected static readonly DirectProperty<AbstractOtpInput, InputControlStatus> EffectiveStatusProperty =
        AvaloniaProperty.RegisterDirect<AbstractOtpInput, InputControlStatus>(
            nameof(EffectiveStatus),
            o => o.EffectiveStatus);

    internal static readonly StyledProperty<FormValidateStatus> FormStatusProperty =
        InputControlState.FormStatusProperty.AddOwner<AbstractOtpInput>();

    protected static readonly DirectProperty<AbstractOtpInput, int> ActiveIndexProperty =
        AvaloniaProperty.RegisterDirect<AbstractOtpInput, int>(nameof(ActiveIndex), input => input.ActiveIndex);

    private InputControlStatus _effectiveStatus;

    protected InputControlStatus EffectiveStatus
    {
        get => _effectiveStatus;
        private set => SetAndRaise(EffectiveStatusProperty, ref _effectiveStatus, value);
    }

    internal FormValidateStatus FormStatus => GetValue(FormStatusProperty);

    private bool _isCompleted;
    private int _activeIndex;
    private EventHandler? _formValueChanged;

    public event EventHandler<OtpLineEditCompletedEventArgs>? Completed;

    public event EventHandler<TextChangedEventArgs>? TextChanged
    {
        add => AddHandler(TextChangedEvent, value);
        remove => RemoveHandler(TextChangedEvent, value);
    }

    protected int ActiveIndex
    {
        get => _activeIndex;
        set => SetAndRaise(ActiveIndexProperty, ref _activeIndex, Math.Clamp(value, 0, GetActiveIndexUpperBound()));
    }

    /// <summary>Returns the physical index of a cell when layout does not mirror itself.</summary>
    protected int GetVisualCellIndex(int logicalIndex) =>
        FlowDirection == FlowDirection.RightToLeft ? Length - 1 - logicalIndex : logicalIndex;

    static AbstractOtpInput()
    {
        FocusableProperty.OverrideDefaultValue<AbstractOtpInput>(true);
    }

    protected AbstractOtpInput()
    {
        UpdateEffectiveStatus();
    }

    public void Clear()
    {
        SetCurrentValue(TextProperty, null);
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
        }

        if (change.Property == TextProperty)
        {
            _formValueChanged?.Invoke(this, EventArgs.Empty);
            RaiseEvent(new TextChangedEventArgs(TextChangedEvent));
        }

        if (change.Property == StatusProperty ||
            change.Property == FormStatusProperty ||
            change.Property == DataValidationErrors.HasErrorsProperty ||
            change.Property == DataValidationErrors.ErrorsProperty)
        {
            UpdateEffectiveStatus();
        }

        if (change.Property == IsKeyboardFocusWithinProperty && change.GetNewValue<bool>())
        {
            MoveActiveIndexToFirstEmptyCell();
        }
    }

    protected override void OnGotFocus(FocusChangedEventArgs e)
    {
        base.OnGotFocus(e);
        MoveActiveIndexToFirstEmptyCell();
    }

    protected override void OnTextInput(TextInputEventArgs e)
    {
        if (!ReferenceEquals(e.Source, this) || !TryHandleTextInput(e))
        {
            base.OnTextInput(e);
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (!ReferenceEquals(e.Source, this) || !TryHandleKeyDown(e))
        {
            base.OnKeyDown(e);
        }
    }

    protected virtual int NormalizeLength(int length) => length > 0 ? length : 6;

    protected bool InsertText(string text)
    {
        if (!CanEdit() || string.IsNullOrEmpty(text))
        {
            return false;
        }

        ReplaceTextFromActiveIndex(text);
        return true;
    }

    protected bool Backspace()
    {
        if (!CanEdit())
        {
            return false;
        }

        RemoveWithBackspace();
        return true;
    }

    protected bool TryHandleTextInput(TextInputEventArgs e)
    {
        if (e.Text is not { } text || !InsertText(text))
        {
            return false;
        }

        e.Handled = true;
        return true;
    }

    protected bool TryHandleKeyDown(KeyEventArgs e)
    {
        if (!CanEdit())
        {
            return false;
        }

        switch (e.Key)
        {
            case Key.Back:
                Backspace();
                e.Handled = true;
                return true;
            case Key.Delete:
                RemoveAtActiveIndex();
                e.Handled = true;
                return true;
            case Key.Left:
                MoveActiveIndex(FlowDirection == FlowDirection.RightToLeft ? 1 : -1);
                e.Handled = true;
                return true;
            case Key.Right:
                MoveActiveIndex(FlowDirection == FlowDirection.RightToLeft ? -1 : 1);
                e.Handled = true;
                return true;
            case Key.Home:
                ActiveIndex = 0;
                e.Handled = true;
                return true;
            case Key.End:
                MoveActiveIndexToFirstEmptyCell();
                e.Handled = true;
                return true;
            default:
                return false;
        }
    }

    private static string? CoerceText(AvaloniaObject sender, string? value)
    {
        var otpLineEdit = (AbstractOtpInput)sender;
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
        var raiseCompleted = isCompleted && !_isCompleted;
        _isCompleted = isCompleted;
        if (raiseCompleted && Text is { } text)
        {
            Completed?.Invoke(this, new OtpLineEditCompletedEventArgs(text));
        }
    }

    protected bool CanEdit()
    {
        return IsEffectivelyEnabled && !IsReadOnly;
    }

    private void ReplaceTextFromActiveIndex(string text)
    {
        if (Formatter is null)
        {
            // Invalid pasted characters must not overwrite an existing valid cell.
            text = FilterByInputMode(text);
            if (text.Length == 0)
            {
                return;
            }
        }

        var current    = Text ?? string.Empty;
        var startIndex = Math.Clamp(ActiveIndex, 0, Length - 1);
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
        var activeIndex      = Math.Clamp(ActiveIndex, 0, Length - 1);
        var removeIndex      = Math.Min(activeIndex, current.Length - 1);

        ActiveIndex = Math.Clamp(removeIndex, 0, Length - 1);
        SetCurrentValue(TextProperty, RemoveAt(current, removeIndex));
    }

    private void RemoveAtActiveIndex()
    {
        var current = Text;
        if (string.IsNullOrEmpty(current) || ActiveIndex >= current.Length)
        {
            return;
        }

        var removeIndex     = Math.Clamp(ActiveIndex, 0, current.Length - 1);
        var nextText        = RemoveAt(current, removeIndex);
        var nextTextLength  = nextText?.Length ?? 0;
        var nextActiveIndex = nextTextLength == 0
            ? 0
            : Math.Min(removeIndex, nextTextLength - 1);

        ActiveIndex = Math.Clamp(nextActiveIndex, 0, Length - 1);
        SetCurrentValue(TextProperty, nextText);
    }

    private static string? RemoveAt(string value, int index)
    {
        var result = value.Remove(index, 1);
        return result.Length == 0 ? null : result;
    }

    private void MoveActiveIndex(int delta)
    {
        ActiveIndex += delta;
    }

    private void MoveActiveIndexToFirstEmptyCell()
    {
        SetActiveIndexToFirstEmptyCell();
    }

    private void ConfigureActiveIndexForValueChange()
    {
        if (IsKeyboardFocusWithin)
        {
            ActiveIndex = _activeIndex;
        }
        else
        {
            SetActiveIndexToFirstEmptyCell();
        }
    }

    private void SetActiveIndexToFirstEmptyCell()
    {
        ActiveIndex = GetActiveIndexUpperBound();
    }

    private int GetActiveIndexUpperBound()
    {
        return Math.Clamp(Text?.Length ?? 0, 0, Length - 1);
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
}
