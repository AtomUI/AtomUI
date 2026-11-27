using AtomUI.Media.TextFormatting;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Avalonia.Utilities;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// A text block whose text the user can select with the pointer or with the select-all shortcut, and copy with
/// the platform's copy shortcut. The selection is painted with <see cref="SelectionBrush" />.
/// </summary>
public class SelectableTextBlock : TextBlock
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="SelectionStart" /> property.
    /// </summary>
    public static readonly StyledProperty<int> SelectionStartProperty =
        Avalonia.Controls.TextBox.SelectionStartProperty.AddOwner<SelectableTextBlock>();

    /// <summary>
    /// Identifies the <see cref="SelectionEnd" /> property.
    /// </summary>
    public static readonly StyledProperty<int> SelectionEndProperty =
        Avalonia.Controls.TextBox.SelectionEndProperty.AddOwner<SelectableTextBlock>();

    /// <summary>
    /// Identifies the <see cref="SelectedText" /> property.
    /// </summary>
    public static readonly DirectProperty<SelectableTextBlock, string> SelectedTextProperty =
        AvaloniaProperty.RegisterDirect<SelectableTextBlock, string>(
            nameof(SelectedText),
            o => o.SelectedText);

    /// <summary>
    /// Identifies the <see cref="SelectionBrush" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> SelectionBrushProperty =
        Avalonia.Controls.TextBox.SelectionBrushProperty.AddOwner<SelectableTextBlock>();

    /// <summary>
    /// Identifies the <see cref="SelectionForegroundBrush" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> SelectionForegroundBrushProperty =
        Avalonia.Controls.TextBox.SelectionForegroundBrushProperty.AddOwner<SelectableTextBlock>();

    /// <summary>
    /// Identifies the <see cref="CanCopy" /> property.
    /// </summary>
    public static readonly DirectProperty<SelectableTextBlock, bool> CanCopyProperty =
        Avalonia.Controls.TextBox.CanCopyProperty.AddOwner<SelectableTextBlock>(o => o.CanCopy);

    /// <summary>
    /// Identifies the <see cref="CopyingToClipboard" /> routed event.
    /// </summary>
    public static readonly RoutedEvent<RoutedEventArgs> CopyingToClipboardEvent =
        RoutedEvent.Register<SelectableTextBlock, RoutedEventArgs>(
            nameof(CopyingToClipboard), RoutingStrategies.Bubble);

    static SelectableTextBlock()
    {
        FocusableProperty.OverrideDefaultValue(typeof(SelectableTextBlock), true);
        AffectsRender<SelectableTextBlock>(SelectionStartProperty, SelectionEndProperty, SelectionBrushProperty);
    }

    /// <summary>
    /// Raised before the selected text is written to the clipboard, when a copy finds a non-empty selection.
    /// Handling the bubbling event prevents the clipboard write.
    /// </summary>
    public event EventHandler<RoutedEventArgs>? CopyingToClipboard
    {
        add => AddHandler(CopyingToClipboardEvent, value);
        remove => RemoveHandler(CopyingToClipboardEvent, value);
    }

    /// <summary>
    /// Gets or sets the brush that highlights selected text.
    /// </summary>
    public IBrush? SelectionBrush
    {
        get => GetValue(SelectionBrushProperty);
        set => SetValue(SelectionBrushProperty, value);
    }

    /// <summary>
    /// Gets or sets the brush of the selected text. While it is <see langword="null" />, the selected text
    /// keeps the <c>Foreground</c> brush.
    /// </summary>
    public IBrush? SelectionForegroundBrush
    {
        get => GetValue(SelectionForegroundBrushProperty);
        set => SetValue(SelectionForegroundBrushProperty, value);
    }

    /// <summary>
    /// Gets or sets a character index for the beginning of the current selection.
    /// </summary>
    public int SelectionStart
    {
        get => GetValue(SelectionStartProperty);
        set => SetValue(SelectionStartProperty, value);
    }

    /// <summary>
    /// Gets or sets a character index for the end of the current selection.
    /// </summary>
    public int SelectionEnd
    {
        get => GetValue(SelectionEndProperty);
        set => SetValue(SelectionEndProperty, value);
    }

    /// <summary>
    /// Gets the content of the current selection.
    /// </summary>
    public string SelectedText
    {
        get => GetSelection();
    }

    /// <summary>
    /// Gets whether <see cref="CopyAsync" /> can copy, which is the case when the current selection is not empty.
    /// </summary>
    public bool CanCopy
    {
        get => _canCopy;
        private set => SetAndRaise(CanCopyProperty, ref _canCopy, value);
    }

    #endregion

    private bool _canCopy;
    private int _wordSelectionStart = -1;

    /// <summary>
    /// Copies the current selection to the clipboard. Nothing is copied while <see cref="CanCopy" /> is
    /// <see langword="false" />, while the selection is empty, or when a handler of
    /// <see cref="CopyingToClipboard" /> marks the event as handled. The returned task completes when the
    /// clipboard write has finished; an exception thrown on the way is caught and written to the debug output,
    /// so the task still completes normally.
    /// </summary>
    public async Task CopyAsync()
    {
        try
        {
            if (!_canCopy)
            {
                return;
            }

            var text = GetSelection();

            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            var eventArgs = new RoutedEventArgs(CopyingToClipboardEvent);

            RaiseEvent(eventArgs);

            if (!eventArgs.Handled)
            {
                if (!this.IsAttachedToVisualTree())
                {
                    return;
                }

                var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;

                if (clipboard != null)
                {
                    await clipboard.SetTextAsync(text);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in CopyAsync: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Selects the whole text: the text of the inlines when they are set, otherwise <c>Text</c>.
    /// </summary>
    public void SelectAll()
    {
        var text = Inlines is { Count: > 0 } ? Inlines?.Text : Text;

        SetCurrentValue(SelectionStartProperty, 0);
        SetCurrentValue(SelectionEndProperty, text?.Length ?? 0);
    }

    /// <summary>
    /// Clears the current selection
    /// </summary>
    public void ClearSelection()
    {
        SetCurrentValue(SelectionEndProperty, SelectionStart);
    }

    /// <summary>
    /// Calls the base member and then updates <see cref="CanCopy" /> to match the current selection.
    /// </summary>
    protected override void OnGotFocus(FocusChangedEventArgs e)
    {
        base.OnGotFocus(e);

        UpdateCommandStates();
    }

    /// <summary>
    /// Clears the selection when the control loses focus while its context menu and its context flyout are
    /// closed, so that the selection survives while one of them is open. Calls the base member first and then
    /// updates <see cref="CanCopy" />.
    /// </summary>
    protected override void OnLostFocus(FocusChangedEventArgs e)
    {
        base.OnLostFocus(e);

        if ((ContextFlyout == null || !ContextFlyout.IsOpen) &&
            (ContextMenu == null || !ContextMenu.IsOpen))
        {
            ClearSelection();
        }

        UpdateCommandStates();
    }

    /// <summary>
    /// Creates the text layout without calling the base member. The text from <see cref="SelectionStart" /> to
    /// <see cref="SelectionEnd" /> is drawn in <see cref="SelectionForegroundBrush" /> when the range is not
    /// empty and the brush is set; all other text uses the text properties of the control.
    /// </summary>
    protected override TextLayout CreateTextLayout(string? text)
    {
        var typeface = new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);

        var defaultProperties = new GenericTextRunProperties(
            typeface,
            FontSize,
            TextDecorations,
            Foreground,
            fontFeatures: FontFeatures);

        var paragraphProperties = new GenericTextParagraphProperties(FlowDirection, TextAlignment, true, false,
            defaultProperties, TextWrapping, LineHeight, 0, LetterSpacing);

        List<ValueSpan<TextRunProperties>>? textStyleOverrides = null;
        var selectionStart = SelectionStart;
        var selectionEnd = SelectionEnd;
        var start = Math.Min(selectionStart, selectionEnd);
        var length = Math.Max(selectionStart, selectionEnd) - start;

        if (length > 0 && SelectionForegroundBrush != null)
        {
            textStyleOverrides = new List<ValueSpan<TextRunProperties>>
            {
                new ValueSpan<TextRunProperties>(start, length,
                    new GenericTextRunProperties(typeface, FontSize,
                        TextDecorations,
                        foregroundBrush: SelectionForegroundBrush,
                        fontFeatures: FontFeatures))
            };
        }

        ITextSource textSource;

        if (_textRuns != null)
        {
            textSource = new InlinesTextSource(_textRuns, textStyleOverrides);
        }
        else
        {
            textSource = new FormattedTextSource(text ?? "", defaultProperties, textStyleOverrides);
        }

        var maxSize = this.GetMaxSizeFromConstraint();

        return new TextLayout(
            textSource,
            paragraphProperties,
            TextTrimming,
            maxSize.Width,
            maxSize.Height,
            MaxLines);
    }

    /// <summary>
    /// Paints the selection highlight with <see cref="SelectionBrush" /> behind the text and then calls the base
    /// member to render the text. Nothing is painted when the selection is empty or the brush is
    /// <see langword="null" />.
    /// </summary>
    protected override void RenderTextLayout(DrawingContext context, Point origin)
    {
        var selectionStart = SelectionStart;
        var selectionEnd   = SelectionEnd;
        var selectionBrush = SelectionBrush;

        if (selectionStart != selectionEnd && selectionBrush != null)
        {
            var start  = Math.Min(selectionStart, selectionEnd);
            var length = Math.Max(selectionStart, selectionEnd) - start;

            var rects = TextLayout.HitTestTextRange(start, length);

            using (context.PushTransform(Matrix.CreateTranslation(origin)))
            {
                foreach (var rect in rects)
                {
                    context.FillRectangle(selectionBrush, PixelRect.FromRect(rect, 1).ToRect(1));
                }
            }
        }

        base.RenderTextLayout(context, origin);
    }

    /// <summary>
    /// Calls the base member and then handles the platform's copy shortcut, which starts
    /// <see cref="CopyAsync" />, and its select-all shortcut, which calls <see cref="SelectAll" />.
    /// The key event ends up handled only when one of the two shortcuts matched.
    /// </summary>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        var handled   = false;
        var keymap    = Application.Current!.PlatformSettings!.HotkeyConfiguration;

        if (MatchesAnyGesture(keymap.Copy, e))
        {
            _ = CopyAsync();
            handled = true;
        }
        else if (MatchesAnyGesture(keymap.SelectAll, e))
        {
            SelectAll();
            handled = true;
        }

        e.Handled = handled;
    }

    private static bool MatchesAnyGesture(IEnumerable<KeyGesture> gestures, KeyEventArgs e)
    {
        foreach (var gesture in gestures)
        {
            if (gesture.Matches(e))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Calls the base member and then keeps the derived state in step with the change: a new
    /// <see cref="SelectionStart" /> or <see cref="SelectionEnd" /> raises a change of
    /// <see cref="SelectedText" />, updates <see cref="CanCopy" /> and invalidates the text layout; a new
    /// <see cref="SelectionForegroundBrush" /> invalidates the text layout.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == SelectionStartProperty ||
            change.Property == SelectionEndProperty)
        {
            RaisePropertyChanged(SelectedTextProperty, "", "");
            UpdateCommandStates();
            InvalidateTextLayout();
        }

        if (change.Property == SelectionForegroundBrushProperty)
        {
            InvalidateTextLayout();
        }
    }

    /// <summary>
    /// Sets the selection at the pressed text position for a press of the left button: a single click collapses
    /// the selection to that position, or extends it from the selection start while Shift is held; a double click
    /// selects the word under the pointer; a triple click selects all text. Every press, whatever its button,
    /// captures the pointer and marks the event as handled. The base member is called first.
    /// </summary>
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        var text      = Inlines is { Count: > 0 } ? Inlines?.Text : Text;
        var clickInfo = e.GetCurrentPoint(this);

        if (text != null && clickInfo.Properties.IsLeftButtonPressed)
        {
            var padding = Padding;

            var point = e.GetPosition(this) - new Point(padding.Left, padding.Top);

            var clickToSelect = (e.KeyModifiers & KeyModifiers.Shift) == KeyModifiers.Shift;

            var oldIndex = SelectionStart;

            var hit   = TextLayout.HitTestPoint(point);
            var index = hit.TextPosition;

            switch (e.ClickCount)
            {
                case 1:
                    if (clickToSelect)
                    {
                        if (_wordSelectionStart >= 0)
                        {
                            var previousWord = StringUtils.PreviousWord(text, index);

                            if (index > _wordSelectionStart)
                            {
                                SetCurrentValue(SelectionEndProperty, StringUtils.NextWord(text, index));
                            }

                            if (index < _wordSelectionStart || previousWord == _wordSelectionStart)
                            {
                                SetCurrentValue(SelectionStartProperty, previousWord);
                            }
                        }
                        else
                        {
                            SetCurrentValue(SelectionStartProperty, Math.Min(oldIndex, index));
                            SetCurrentValue(SelectionEndProperty, Math.Max(oldIndex, index));
                        }
                    }
                    else
                    {
                        SetCurrentValue(SelectionStartProperty, index);
                        SetCurrentValue(SelectionEndProperty, index);
                        _wordSelectionStart = -1;
                    }

                    break;
                case 2:
                    if (!StringUtils.IsStartOfWord(text, index))
                    {
                        SetCurrentValue(SelectionStartProperty, StringUtils.PreviousWord(text, index));
                    }

                    _wordSelectionStart = SelectionStart;

                    if (!StringUtils.IsEndOfWord(text, index))
                    {
                        SetCurrentValue(SelectionEndProperty, StringUtils.NextWord(text, index));
                    }

                    break;
                case 3:
                    _wordSelectionStart = -1;

                    SelectAll();
                    break;
            }
        }

        e.Pointer.Capture(this);
        e.Handled = true;
    }

    /// <summary>
    /// Moves the selection end to the text position under the pointer, or to the word boundary there after a
    /// double click, while this control holds the pointer and the left button is pressed. Calls the base member
    /// first.
    /// </summary>
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        // selection should not change during pointer move if the user right clicks
        if (e.Pointer.Captured == this && e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            var text    = Inlines is { Count: > 0 } ? Inlines?.Text : Text;
            var padding = Padding;

            var point = e.GetPosition(this) - new Point(padding.Left, padding.Top);

            point = new Point(
                Math.Clamp(point.X, 0, Math.Max(TextLayout.WidthIncludingTrailingWhitespace, 0)),
                Math.Clamp(point.Y, 0, Math.Max(TextLayout.Height, 0)));

            var hit          = TextLayout.HitTestPoint(point);
            var textPosition = hit.TextPosition;

            if (text != null && _wordSelectionStart >= 0)
            {
                var distance = textPosition - _wordSelectionStart;

                if (distance <= 0)
                {
                    SetCurrentValue(SelectionStartProperty, StringUtils.PreviousWord(text, textPosition));
                }

                if (distance >= 0)
                {
                    if (SelectionStart != _wordSelectionStart)
                    {
                        SetCurrentValue(SelectionStartProperty, _wordSelectionStart);
                    }

                    SetCurrentValue(SelectionEndProperty, StringUtils.NextWord(text, textPosition));
                }
            }
            else
            {
                SetCurrentValue(SelectionEndProperty, textPosition);
            }
        }
    }

    /// <summary>
    /// Releases the pointer capture after the base member handled the event. When the right button was pressed,
    /// it first collapses the selection to the text position under the pointer unless that position lies inside
    /// the current selection. A release of a pointer that this control does not capture does nothing more than
    /// the base member.
    /// </summary>
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        if (e.Pointer.Captured != this)
        {
            return;
        }

        if (e.InitialPressMouseButton == MouseButton.Right)
        {
            var padding = Padding;

            var point = e.GetPosition(this) - new Point(padding.Left, padding.Top);

            var hit = TextLayout.HitTestPoint(point);

            var caretIndex = hit.TextPosition;

            // see if mouse clicked inside current selection
            // if it did not, we change the selection to where the user clicked
            var firstSelection = Math.Min(SelectionStart, SelectionEnd);
            var lastSelection  = Math.Max(SelectionStart, SelectionEnd);
            var didClickInSelection = SelectionStart != SelectionEnd &&
                                      caretIndex >= firstSelection && caretIndex <= lastSelection;
            if (!didClickInSelection)
            {
                SetCurrentValue(SelectionStartProperty, caretIndex);
                SetCurrentValue(SelectionEndProperty, caretIndex);
            }
        }

        e.Pointer.Capture(null);
    }

    private void UpdateCommandStates()
    {
        var text = GetSelection();

        CanCopy = !string.IsNullOrEmpty(text);
    }

    private string GetSelection()
    {
        var text = Inlines is { Count: > 0 } ? Inlines?.Text : Text;

        var textLength = text?.Length ?? 0;

        if (textLength == 0)
        {
            return "";
        }

        var selectionStart = SelectionStart;
        var selectionEnd   = SelectionEnd;
        var start          = Math.Min(selectionStart, selectionEnd);
        var end            = Math.Max(selectionStart, selectionEnd);

        if (start == end || textLength < end)
        {
            return "";
        }

        var length = Math.Max(0, end - start);

        var selectedText = text!.Substring(start, length);

        return selectedText;
    }
}
