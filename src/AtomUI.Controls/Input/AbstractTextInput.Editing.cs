using System.Globalization;
using Avalonia;
using Avalonia.Controls.Presenters;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace AtomUI.Controls;

/// <summary>
/// The unit used to count and limit text.
/// </summary>
public enum TextLengthMode
{
    /// <summary>
    /// The length is counted in UTF-16 code units, the unit of the Avalonia text box.
    /// </summary>
    Utf16,
    /// <summary>
    /// The length is counted in text elements, so a combining sequence or an emoji counts as one.
    /// </summary>
    TextElement
}

public abstract partial class AbstractTextInput
{
    /// <summary>
    /// Identifies the <see cref="AbstractTextInput.TextLengthMode" /> property.
    /// </summary>
    public static readonly StyledProperty<TextLengthMode> TextLengthModeProperty =
        AvaloniaProperty.Register<AbstractTextInput, TextLengthMode>(nameof(TextLengthMode));

    /// <summary>
    /// Identifies the <see cref="IsClearButtonOnlyWhenFocused" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsClearButtonOnlyWhenFocusedProperty =
        AvaloniaProperty.Register<AbstractTextInput, bool>(nameof(IsClearButtonOnlyWhenFocused));

    /// <summary>
    /// Identifies the <see cref="IsComposing" /> property.
    /// </summary>
    public static readonly DirectProperty<AbstractTextInput, bool> IsComposingProperty =
        AvaloniaProperty.RegisterDirect<AbstractTextInput, bool>(nameof(IsComposing), input => input.IsComposing);

    /// <summary>
    /// Identifies the <see cref="EnterPressed" /> routed event.
    /// </summary>
    public static readonly RoutedEvent<RoutedEventArgs> EnterPressedEvent =
        RoutedEvent.Register<AbstractTextInput, RoutedEventArgs>(nameof(EnterPressed), RoutingStrategies.Bubble);

    /// <summary>
    /// Identifies the <see cref="Cleared" /> routed event.
    /// </summary>
    public static readonly RoutedEvent<RoutedEventArgs> ClearedEvent =
        RoutedEvent.Register<AbstractTextInput, RoutedEventArgs>(nameof(Cleared), RoutingStrategies.Bubble);

    private IDisposable? _nativeLengthOverride;
    private bool _isComposing;
    private bool _suppressSearchForEnter;
    private bool _isResettingComposition;
    private string? _coercionEchoInput;
    private string? _coercionEchoOutput;
    private bool _isCoercionEchoCleanupQueued;

    /// <summary>
    /// Selects the unit in which <see cref="MaxLength" /> and the counter of <see cref="IsShowCount" /> are measured:
    /// UTF-16 code units (the default) or text elements. Switching to <see cref="TextLengthMode.TextElement" /> cuts
    /// existing text that is longer than <see cref="MaxLength" />.
    /// </summary>
    public TextLengthMode TextLengthMode
    {
        get => GetValue(TextLengthModeProperty);
        set => SetValue(TextLengthModeProperty, value);
    }

    /// <summary>
    /// Gets or sets the length limit in the current <see cref="TextLengthMode" />. A value of 0 removes the limit.
    /// </summary>
    public new int MaxLength
    {
        get => _nativeLengthOverride is not null ? GetBaseValue(MaxLengthProperty).GetValueOrDefault() : base.MaxLength;
        set
        {
            base.MaxLength = value;
            ApplyTextElementLimit();
        }
    }

    /// <summary>
    /// <see langword="true" /> shows the clear button only while the input has keyboard focus. The default is
    /// <see langword="false" />, which shows it whenever the text can be cleared.
    /// </summary>
    public bool IsClearButtonOnlyWhenFocused
    {
        get => GetValue(IsClearButtonOnlyWhenFocusedProperty);
        set => SetValue(IsClearButtonOnlyWhenFocusedProperty, value);
    }

    /// <summary>
    /// Whether an input method composition is in progress, that is, the input shows uncommitted pre-edit text.
    /// While composing, the Enter key does not raise <see cref="EnterPressed" /> and text changes are not reported to
    /// the form system.
    /// </summary>
    public bool IsComposing => _isComposing;

    /// <summary>
    /// Raised when the Enter key is pressed in the input, the key event is not handled yet and no input method
    /// composition is in progress. Handle the event to consume the key, so that the text box base class does not
    /// receive it. When the event is not handled, <see cref="OnEnterPressedUnhandled" /> is called, and the key
    /// goes on to the base class unless that method marks the key event handled.
    /// </summary>
    public event EventHandler<RoutedEventArgs>? EnterPressed
    {
        add => AddHandler(EnterPressedEvent, value);
        remove => RemoveHandler(EnterPressedEvent, value);
    }

    /// <summary>
    /// Raised after the text was cleared through <see cref="Clear" />, which the clear button calls.
    /// </summary>
    public event EventHandler<RoutedEventArgs>? Cleared
    {
        add => AddHandler(ClearedEvent, value);
        remove => RemoveHandler(ClearedEvent, value);
    }

    /// <summary>
    /// Resets the active input method, clears the text and raises <see cref="Cleared" />.
    /// </summary>
    public new void Clear()
    {
        ResetComposition();
        base.Clear();
        RaiseEvent(new RoutedEventArgs(ClearedEvent, this));
    }

    /// <summary>
    /// Measures a text in the unit selected by <see cref="TextLengthMode" />.
    /// </summary>
    /// <param name="text">
    /// The text to measure; <see langword="null" /> counts as empty.
    /// </param>
    /// <returns>
    /// The length of the text.
    /// </returns>
    protected int GetTextLength(string? text) => TextLengthMode == TextLengthMode.TextElement
        ? new StringInfo(text ?? string.Empty).LengthInTextElements
        : text?.Length ?? 0;

    /// <summary>
    /// Called when the Enter key was pressed and the <see cref="EnterPressed" /> event was not handled. It runs
    /// before the text box base class processes the key; setting <c>Handled</c> on <paramref name="args" /> keeps the
    /// key from the base class.
    /// </summary>
    /// <param name="args">
    /// The key event of the Enter key.
    /// </param>
    protected virtual void OnEnterPressedUnhandled(KeyEventArgs args)
    {
    }

    /// <summary>
    /// Whether the Enter key press that is being processed must not start a search: an input method composition is in
    /// progress, or the <see cref="EnterPressed" /> event was handled. Search inputs read it when the key is released.
    /// </summary>
    protected bool ShouldSuppressSearchForEnter => _suppressSearchForEnter || IsComposing;

    /// <summary>
    /// Raises <see cref="EnterPressed" /> for an Enter key that is not handled yet and no composition is in progress,
    /// and passes every key that is still unhandled to the base class. An override must call the base method,
    /// otherwise no key reaches the text box.
    /// </summary>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            _suppressSearchForEnter = IsComposing;
            if (!e.Handled && !IsComposing)
            {
                var args = new RoutedEventArgs(EnterPressedEvent, this);
                RaiseEvent(args);
                if (!args.Handled)
                {
                    OnEnterPressedUnhandled(e);
                }

                e.Handled |= args.Handled;
                _suppressSearchForEnter = e.Handled;
            }
        }

        if (!e.Handled)
        {
            base.OnKeyDown(e);
        }
    }

    /// <summary>
    /// Cuts the text of an unhandled input event down to what fits under <see cref="MaxLength" /> when the length is
    /// measured in text elements, and passes the event to the base class. While a composition is being reset, the
    /// event is marked handled instead. An override must call the base method, otherwise typed text is not inserted.
    /// </summary>
    protected override void OnTextInput(TextInputEventArgs e)
    {
        if (_isResettingComposition)
        {
            e.Handled = true;
            return;
        }

        if (!e.Handled && TextLengthMode == TextLengthMode.TextElement && MaxLength > 0 && e.Text is { } text)
        {
            var permitted = LimitInsertion(text, MaxLength);
            if (permitted != text)
            {
                e.Text = permitted;
            }
        }

        base.OnTextInput(e);
    }

    /// <summary>
    /// Adjusts the text that is about to be stored. While an input method shows pre-edit text, the committed text is
    /// kept when the proposed value would drop the selection. When the length is measured in text elements and
    /// <see cref="MaxLength" /> is set, a value that is too long is cut down, keeping the text before and after the
    /// insertion. The base method is called with the adjusted value.
    /// </summary>
    /// <param name="value">
    /// The proposed text.
    /// </param>
    /// <returns>
    /// The text to store.
    /// </returns>
    protected override string? CoerceText(string? value)
    {
        if (!_isResettingComposition && !string.IsNullOrEmpty(_textPresenter?.PreeditText) &&
            Text is { } committed && SelectionStart != SelectionEnd && value == TextWithoutSelection(committed))
        {
            return committed;
        }

        if (!IsComposing && TextLengthMode == TextLengthMode.TextElement && MaxLength > 0 && value is not null)
        {
            if (value == _coercionEchoInput && Text == _coercionEchoOutput)
            {
                return _coercionEchoOutput;
            }

            var limited = LimitProposedText(value, MaxLength);
            if (limited != value)
            {
                TrackCoercionEcho(value, limited);
                return base.CoerceText(limited);
            }
        }

        return base.CoerceText(value);
    }

    private void TrackCoercionEcho(string proposed, string accepted)
    {
        // TextBox's two-way presenter can echo the original edit after Text accepted its truncated form.
        _coercionEchoInput = proposed;
        _coercionEchoOutput = accepted;
        if (_isCoercionEchoCleanupQueued)
        {
            return;
        }
        _isCoercionEchoCleanupQueued = true;
        Dispatcher.UIThread.Post(ClearCoercionEcho);
    }

    private void ClearCoercionEcho()
    {
        _coercionEchoInput = _coercionEchoOutput = null;
        _isCoercionEchoCleanupQueued = false;
    }

    private void ApplyTextElementLimit()
    {
        if (!IsComposing && TextLengthMode == TextLengthMode.TextElement && MaxLength > 0 && Text is { } value)
        {
            var limited = Truncate(value, MaxLength);
            if (limited != value)
            {
                SetCurrentValue(TextProperty, limited);
            }
        }
        HandleInputChanged(Text);
    }

    private void UpdateTextLengthMode()
    {
        if (TextLengthMode == TextLengthMode.TextElement)
        {
            // Avalonia's native insertion truncates UTF-16 before passing text to CoerceText. Use an
            // animated-priority override to keep its truncation off while preserving the consumer's base limit.
            _nativeLengthOverride ??= SetValue(MaxLengthProperty, 0, BindingPriority.Animation);
            ApplyTextElementLimit();
        }
        else
        {
            var previous = _nativeLengthOverride;
            _nativeLengthOverride = null;
            previous?.Dispose();
        }

        HandleInputChanged(Text);
    }

    private void UpdatePreeditState()
    {
        SetAndRaise(IsComposingProperty, ref _isComposing, !string.IsNullOrEmpty(_textPresenter?.PreeditText));
        ConfigurePlaceholderTextVisibility();
    }

    private void ResetComposition()
    {
        if (_textPresenter is null || string.IsNullOrEmpty(_textPresenter.PreeditText))
        {
            return;
        }

        _isResettingComposition = true;
        try
        {
            // Removing the active client through the public attached property requests a platform reset.
            // Restoring the previous value connects the same text control and keeps keyboard focus intact.
            using (SetValue(InputMethod.IsInputMethodEnabledProperty, false, BindingPriority.Animation))
            {
                _textPresenter.SetCurrentValue(TextPresenter.PreeditTextProperty, null);
            }
        }
        finally
        {
            _isResettingComposition = false;
        }
    }

    private string LimitInsertion(string insertion, int maximum)
    {
        var current = Text ?? string.Empty;
        var (start, end) = GetReplacementRange();
        return LimitInsertion(current.AsSpan(0, start), insertion, current.AsSpan(end), maximum);
    }

    private string LimitProposedText(string value, int maximum)
    {
        if (GetTextLength(value) <= maximum)
        {
            return value;
        }

        if (Text is { } committed)
        {
            // Native paste and SelectedText call the same private insertion path without a TextInput event.
            // Detect its proposed value here so truncating the insertion does not discard the existing suffix.
            var (start, end) = GetReplacementRange();
            var prefix = committed.AsSpan(0, start);
            var suffix = committed.AsSpan(end);
            if (value.Length >= prefix.Length + suffix.Length && value.AsSpan().StartsWith(prefix, StringComparison.Ordinal) &&
                value.AsSpan().EndsWith(suffix, StringComparison.Ordinal))
            {
                var insertion = value.Substring(prefix.Length, value.Length - prefix.Length - suffix.Length);
                return string.Concat(prefix, LimitInsertion(prefix, insertion, suffix, maximum), suffix);
            }
        }

        return Truncate(value, maximum);
    }

    private (int Start, int End) GetReplacementRange()
    {
        var start = Math.Min(SelectionStart, SelectionEnd);
        var end = Math.Max(SelectionStart, SelectionEnd);
        return start == end ? (CaretIndex, CaretIndex) : (start, end);
    }

    private string LimitInsertion(ReadOnlySpan<char> leading, string insertion, ReadOnlySpan<char> following, int maximum)
    {
        if (GetTextLength(string.Concat(leading, insertion, following)) <= maximum)
        {
            return insertion;
        }

        var starts = StringInfo.ParseCombiningCharacters(insertion);
        // A prefix having more clusters than the whole budget can only fit if its edges merge into
        // the surrounding graphemes; allow those two positions before testing actual combined text.
        var lastCandidate = Math.Min(starts.Length - 1, maximum <= int.MaxValue - 2 ? maximum + 2 : maximum);
        for (var index = lastCandidate; index > 0; index--)
        {
            var prefix = insertion.AsSpan(0, starts[index]);
            if (GetTextLength(string.Concat(leading, prefix, following)) <= maximum)
            {
                return insertion[..starts[index]];
            }
        }

        return string.Empty;
    }

    private string TextWithoutSelection(string text)
    {
        var start = Math.Min(SelectionStart, SelectionEnd);
        var end = Math.Max(SelectionStart, SelectionEnd);
        return start >= 0 && end <= text.Length ? text.Remove(start, end - start) : text;
    }

    private static string Truncate(string text, int maximum)
    {
        var offset = 0;
        while (offset < text.Length && maximum > 0)
        {
            offset += StringInfo.GetNextTextElementLength(text.AsSpan(offset));
            maximum--;
        }
        return offset == text.Length ? text : text[..offset];
    }
}
