using System.Globalization;
using Avalonia;
using Avalonia.Controls.Presenters;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace AtomUI.Controls;

/// <summary>The unit used to count and limit text.</summary>
public enum TextLengthMode
{
    Utf16,
    TextElement
}

public abstract partial class AbstractTextInput
{
    public static readonly StyledProperty<TextLengthMode> TextLengthModeProperty =
        AvaloniaProperty.Register<AbstractTextInput, TextLengthMode>(nameof(TextLengthMode));

    public static readonly StyledProperty<bool> IsClearButtonOnlyWhenFocusedProperty =
        AvaloniaProperty.Register<AbstractTextInput, bool>(nameof(IsClearButtonOnlyWhenFocused));

    public static readonly DirectProperty<AbstractTextInput, bool> IsComposingProperty =
        AvaloniaProperty.RegisterDirect<AbstractTextInput, bool>(nameof(IsComposing), input => input.IsComposing);

    public static readonly RoutedEvent<RoutedEventArgs> EnterPressedEvent =
        RoutedEvent.Register<AbstractTextInput, RoutedEventArgs>(nameof(EnterPressed), RoutingStrategies.Bubble);

    public static readonly RoutedEvent<RoutedEventArgs> ClearedEvent =
        RoutedEvent.Register<AbstractTextInput, RoutedEventArgs>(nameof(Cleared), RoutingStrategies.Bubble);

    private IDisposable? _nativeLengthOverride;
    private bool _isComposing;
    private bool _suppressSearchForEnter;
    private bool _isResettingComposition;
    private string? _coercionEchoInput;
    private string? _coercionEchoOutput;
    private bool _isCoercionEchoCleanupQueued;

    public TextLengthMode TextLengthMode
    {
        get => GetValue(TextLengthModeProperty);
        set => SetValue(TextLengthModeProperty, value);
    }

    /// <summary>Gets or sets the length limit in the current <see cref="TextLengthMode"/>.</summary>
    public new int MaxLength
    {
        get => _nativeLengthOverride is not null ? GetBaseValue(MaxLengthProperty).GetValueOrDefault() : base.MaxLength;
        set
        {
            base.MaxLength = value;
            ApplyTextElementLimit();
        }
    }

    public bool IsClearButtonOnlyWhenFocused
    {
        get => GetValue(IsClearButtonOnlyWhenFocusedProperty);
        set => SetValue(IsClearButtonOnlyWhenFocusedProperty, value);
    }

    public bool IsComposing => _isComposing;

    public event EventHandler<RoutedEventArgs>? EnterPressed
    {
        add => AddHandler(EnterPressedEvent, value);
        remove => RemoveHandler(EnterPressedEvent, value);
    }

    public event EventHandler<RoutedEventArgs>? Cleared
    {
        add => AddHandler(ClearedEvent, value);
        remove => RemoveHandler(ClearedEvent, value);
    }

    /// <summary>Resets the active input method, clears the text and raises <see cref="Cleared"/>.</summary>
    public new void Clear()
    {
        ResetComposition();
        base.Clear();
        RaiseEvent(new RoutedEventArgs(ClearedEvent, this));
    }

    protected int GetTextLength(string? text) => TextLengthMode == TextLengthMode.TextElement
        ? new StringInfo(text ?? string.Empty).LengthInTextElements
        : text?.Length ?? 0;

    protected virtual void OnEnterPressedUnhandled(KeyEventArgs args)
    {
    }

    protected bool ShouldSuppressSearchForEnter => _suppressSearchForEnter || IsComposing;

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
