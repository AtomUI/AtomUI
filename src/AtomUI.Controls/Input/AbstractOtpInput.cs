using AtomUI.Desktop.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace AtomUI.Controls;

/// <summary>
/// Base class of one-time password inputs. It keeps the code in <see cref="Text" />, one character per cell, and
/// handles typing, deletion, navigation between cells, masking, formatting, validation status and form
/// integration. A deriving control renders the cells.
/// </summary>
public abstract class AbstractOtpInput : TemplatedControl, IInputControlStatusAware, IFormItemAware
{
    /// <summary>
    /// Identifies the <see cref="Text" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<AbstractOtpInput, string?>(
            nameof(Text),
            defaultBindingMode: BindingMode.TwoWay,
            coerce: CoerceText);

    /// <summary>
    /// Identifies the <see cref="Length" /> property.
    /// </summary>
    public static readonly StyledProperty<int> LengthProperty =
        AvaloniaProperty.Register<AbstractOtpInput, int>(
            nameof(Length),
            6,
            coerce: (owner, value) => ((AbstractOtpInput)owner).NormalizeLength(value));

    /// <summary>
    /// Identifies the <see cref="InputMode" /> property.
    /// </summary>
    public static readonly StyledProperty<OtpLineEditInputMode> InputModeProperty =
        AvaloniaProperty.Register<AbstractOtpInput, OtpLineEditInputMode>(nameof(InputMode));

    /// <summary>
    /// Identifies the <see cref="Formatter" /> property.
    /// </summary>
    public static readonly StyledProperty<Func<string, string>?> FormatterProperty =
        AvaloniaProperty.Register<AbstractOtpInput, Func<string, string>?>(nameof(Formatter));

    /// <summary>
    /// Identifies the <see cref="IsMasked" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMaskedProperty =
        AvaloniaProperty.Register<AbstractOtpInput, bool>(nameof(IsMasked));

    /// <summary>
    /// Identifies the <see cref="MaskChar" /> property.
    /// </summary>
    public static readonly StyledProperty<char> MaskCharProperty =
        AvaloniaProperty.Register<AbstractOtpInput, char>(nameof(MaskChar), '•');

    /// <summary>
    /// Identifies the <see cref="IsReadOnly" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsReadOnlyProperty =
        AvaloniaProperty.Register<AbstractOtpInput, bool>(nameof(IsReadOnly));

    /// <summary>
    /// Identifies the <see cref="Status" /> property.
    /// </summary>
    public static readonly StyledProperty<InputControlStatus> StatusProperty =
        InputControlStatusProperty.StatusProperty.AddOwner<AbstractOtpInput>();

    /// <summary>
    /// Identifies the <see cref="TextChanged" /> routed event.
    /// </summary>
    public static readonly RoutedEvent<TextChangedEventArgs> TextChangedEvent =
        RoutedEvent.Register<AbstractOtpInput, TextChangedEventArgs>(nameof(TextChanged), RoutingStrategies.Bubble);

    /// <summary>
    /// The code as a single string; each character fills one cell, in order. The value is normalized before it is
    /// stored: <see cref="Formatter" /> is applied, characters not allowed by <see cref="InputMode" /> are removed and
    /// the result is cut to <see cref="Length" /> characters. Binds two-way by default.
    /// </summary>
    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>
    /// Number of cells. An assigned value passes through <see cref="NormalizeLength" />; the base implementation
    /// replaces values of 0 or less with 6, the default. Text longer than the new value is cut, and shortening the
    /// value to the length of the text raises <see cref="Completed" />.
    /// </summary>
    public int Length
    {
        get => GetValue(LengthProperty);
        set => SetValue(LengthProperty, value);
    }

    /// <summary>
    /// Restricts the characters kept in <see cref="Text" />. Changing it normalizes the current text, so characters
    /// that are no longer allowed are removed.
    /// </summary>
    public OtpLineEditInputMode InputMode
    {
        get => GetValue(InputModeProperty);
        set => SetValue(InputModeProperty, value);
    }

    /// <summary>
    /// Function that transforms text before it is stored in <see cref="Text" />: when <see cref="Text" /> is assigned
    /// and when the user types or pastes. Its result is still filtered by <see cref="InputMode" /> and cut to
    /// <see cref="Length" />. Changing it normalizes the current text. <see langword="null" />, the default, stores
    /// text unchanged.
    /// </summary>
    public Func<string, string>? Formatter
    {
        get => GetValue(FormatterProperty);
        set => SetValue(FormatterProperty, value);
    }

    /// <summary>
    /// <see langword="true" /> to show <see cref="MaskChar" /> in every filled cell instead of the character.
    /// </summary>
    public bool IsMasked
    {
        get => GetValue(IsMaskedProperty);
        set => SetValue(IsMaskedProperty, value);
    }

    /// <summary>
    /// Character shown in filled cells while <see cref="IsMasked" /> is <see langword="true" />. The default is a
    /// bullet (U+2022).
    /// </summary>
    public char MaskChar
    {
        get => GetValue(MaskCharProperty);
        set => SetValue(MaskCharProperty, value);
    }

    /// <summary>
    /// <see langword="true" /> stops keyboard editing: typing, Backspace and Delete are ignored, while
    /// <see cref="Text" /> can still be assigned.
    /// </summary>
    public bool IsReadOnly
    {
        get => GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    /// <summary>
    /// Validation state painted on the cells: <see cref="InputControlStatus.Warning" /> or
    /// <see cref="InputControlStatus.Error" /> colors them accordingly, and <see cref="InputControlStatus.Default" />
    /// leaves them neutral. Data validation errors and an error reported by the form take precedence and force the
    /// error state; a warning reported by the form forces the warning state.
    /// </summary>
    public InputControlStatus Status
    {
        get => GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="EffectiveStatus" /> property.
    /// </summary>
    protected static readonly DirectProperty<AbstractOtpInput, InputControlStatus> EffectiveStatusProperty =
        AvaloniaProperty.RegisterDirect<AbstractOtpInput, InputControlStatus>(
            nameof(EffectiveStatus),
            o => o.EffectiveStatus);

    internal static readonly StyledProperty<FormValidateStatus> FormStatusProperty =
        InputControlState.FormStatusProperty.AddOwner<AbstractOtpInput>();

    /// <summary>
    /// Identifies the <see cref="ActiveIndex" /> property.
    /// </summary>
    protected static readonly DirectProperty<AbstractOtpInput, int> ActiveIndexProperty =
        AvaloniaProperty.RegisterDirect<AbstractOtpInput, int>(nameof(ActiveIndex), input => input.ActiveIndex);

    private InputControlStatus _effectiveStatus;

    /// <summary>
    /// The status the cells are painted with: <see cref="Status" /> combined with the validation status reported by
    /// the form and with data validation errors. It is read-only and is a direct property so that a theme can bind
    /// to it.
    /// </summary>
    protected InputControlStatus EffectiveStatus
    {
        get => _effectiveStatus;
        private set => SetAndRaise(EffectiveStatusProperty, ref _effectiveStatus, value);
    }

    internal FormValidateStatus FormStatus => GetValue(FormStatusProperty);

    private bool _isCompleted;
    private int _activeIndex;
    private EventHandler? _formValueChanged;

    /// <summary>
    /// Raised when <see cref="Text" /> reaches <see cref="Length" /> characters while it was not complete before.
    /// It is not raised again until the text is shorter once more. For the same change it is raised before
    /// <see cref="TextChanged" />.
    /// </summary>
    public event EventHandler<OtpLineEditCompletedEventArgs>? Completed;

    /// <summary>
    /// Raised after <see cref="Text" /> changed, whatever the cause: typing, deletion, assignment, normalization after
    /// <see cref="Length" />, <see cref="InputMode" /> or <see cref="Formatter" /> changed, or <see cref="Clear" />.
    /// </summary>
    public event EventHandler<TextChangedEventArgs>? TextChanged
    {
        add => AddHandler(TextChangedEvent, value);
        remove => RemoveHandler(TextChangedEvent, value);
    }

    /// <summary>
    /// Index of the cell that receives keyboard input. Assigned values are clamped to the range from 0 to the first
    /// empty cell. The cell is highlighted only while the control is enabled and has keyboard focus.
    /// </summary>
    protected int ActiveIndex
    {
        get => _activeIndex;
        set => SetAndRaise(ActiveIndexProperty, ref _activeIndex, Math.Clamp(value, 0, GetActiveIndexUpperBound()));
    }

    /// <summary>
    /// Maps a cell index counted from the start of <see cref="Text" /> to its position counted from the left edge.
    /// With right-to-left flow direction the positions are reversed.
    /// </summary>
    /// <param name="logicalIndex">
    /// The cell index counted from the start of <see cref="Text" />.
    /// </param>
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

    /// <summary>
    /// Sets <see cref="Text" /> to <see langword="null" />, which empties every cell.
    /// </summary>
    public void Clear()
    {
        SetCurrentValue(TextProperty, null);
    }

    /// <summary>
    /// Keeps the state in step with property changes. It normalizes <see cref="Text" /> after <see cref="Length" />,
    /// <see cref="InputMode" /> or <see cref="Formatter" /> changed; updates the active cell, the completion state and
    /// the <c>:filled</c> and <c>:empty</c> pseudo-classes after <see cref="Text" /> or <see cref="Length" /> changed;
    /// raises <see cref="Completed" /> and <see cref="TextChanged" />; recomputes <see cref="EffectiveStatus" />; and
    /// moves the active cell to the first empty cell when the control gains keyboard focus. An override must call the
    /// base method.
    /// </summary>
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

    /// <summary>
    /// Moves the active cell to the first empty cell when the control gains focus. An override must call the base
    /// method to keep that move.
    /// </summary>
    protected override void OnGotFocus(FocusChangedEventArgs e)
    {
        base.OnGotFocus(e);
        MoveActiveIndexToFirstEmptyCell();
    }

    /// <summary>
    /// Passes the text of the event to <see cref="TryHandleTextInput" /> when the event comes from this control and
    /// marks the event handled when the text is accepted; in every other case the event is passed to the base class.
    /// An override must call the base method, otherwise typing does nothing.
    /// </summary>
    protected override void OnTextInput(TextInputEventArgs e)
    {
        if (!ReferenceEquals(e.Source, this) || !TryHandleTextInput(e))
        {
            base.OnTextInput(e);
        }
    }

    /// <summary>
    /// Lets <see cref="TryHandleKeyDown" /> process the key when the event comes from this control; keys it does not
    /// handle are passed to the base class. An override must call the base method, otherwise the cells do not react
    /// to the keyboard.
    /// </summary>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (!ReferenceEquals(e.Source, this) || !TryHandleKeyDown(e))
        {
            base.OnKeyDown(e);
        }
    }

    /// <summary>
    /// Coerces a value assigned to <see cref="Length" />. The base implementation returns 6 for values of 0 or less
    /// and the value itself otherwise.
    /// </summary>
    /// <param name="length">
    /// The value assigned to <see cref="Length" />.
    /// </param>
    /// <returns>
    /// The number of cells to use.
    /// </returns>
    protected virtual int NormalizeLength(int length) => length > 0 ? length : 6;

    /// <summary>
    /// Writes <paramref name="text" /> into the cells starting at the active cell, replacing the characters there,
    /// and moves the active cell to the first empty cell.
    /// </summary>
    /// <param name="text">
    /// The text to insert.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when the text was accepted for insertion; <see langword="false" /> when
    /// <see cref="CanEdit" /> is <see langword="false" /> or <paramref name="text" /> is empty.
    /// </returns>
    protected bool InsertText(string text)
    {
        if (!CanEdit() || string.IsNullOrEmpty(text))
        {
            return false;
        }

        ReplaceTextFromActiveIndex(text);
        return true;
    }

    /// <summary>
    /// Removes the character of the active cell, or the last character when the active cell is empty, and leaves
    /// the active cell on the position of the removed character.
    /// </summary>
    /// <returns>
    /// <see langword="true" /> when the key was processed, also for empty text; <see langword="false" /> when
    /// <see cref="CanEdit" /> is <see langword="false" />.
    /// </returns>
    protected bool Backspace()
    {
        if (!CanEdit())
        {
            return false;
        }

        RemoveWithBackspace();
        return true;
    }

    /// <summary>
    /// Passes the text of the event to <see cref="InsertText" /> and marks the event handled when that accepts it.
    /// </summary>
    /// <param name="e">
    /// The text input event to process.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when the event was handled; <see langword="false" /> when it was left untouched.
    /// </returns>
    protected bool TryHandleTextInput(TextInputEventArgs e)
    {
        if (e.Text is not { } text || !InsertText(text))
        {
            return false;
        }

        e.Handled = true;
        return true;
    }

    /// <summary>
    /// Handles Backspace, Delete, Left, Right, Home and End for the active cell and marks the event handled. Left
    /// and Right follow the flow direction: with right-to-left flow they move to the next and the previous cell.
    /// </summary>
    /// <param name="e">
    /// The key event to process.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when the event was handled; <see langword="false" /> when the input cannot be edited
    /// (<see cref="CanEdit" />) or the key is none of these keys, in which case the event is left untouched.
    /// </returns>
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

    /// <summary>
    /// Returns whether keyboard editing is accepted: the control is effectively enabled and not read-only.
    /// </summary>
    /// <returns>
    /// <see langword="true" /> when text may be inserted or removed.
    /// </returns>
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

        // After a removal the active cell stays on the removed position. When that cell is now empty, the next
        // Backspace removes the character before it.
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
