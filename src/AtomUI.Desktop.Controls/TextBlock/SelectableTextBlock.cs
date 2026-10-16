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

public class SelectableTextBlock : TextBlock
{
    #region 公共属性定义

    public static readonly StyledProperty<int> SelectionStartProperty =
        Avalonia.Controls.TextBox.SelectionStartProperty.AddOwner<SelectableTextBlock>();

    public static readonly StyledProperty<int> SelectionEndProperty =
        Avalonia.Controls.TextBox.SelectionEndProperty.AddOwner<SelectableTextBlock>();

    public static readonly DirectProperty<SelectableTextBlock, string> SelectedTextProperty =
        AvaloniaProperty.RegisterDirect<SelectableTextBlock, string>(
            nameof(SelectedText),
            o => o.SelectedText);

    public static readonly StyledProperty<IBrush?> SelectionBrushProperty =
        Avalonia.Controls.TextBox.SelectionBrushProperty.AddOwner<SelectableTextBlock>();

    public static readonly StyledProperty<IBrush?> SelectionForegroundBrushProperty =
        Avalonia.Controls.TextBox.SelectionForegroundBrushProperty.AddOwner<SelectableTextBlock>();

    public static readonly DirectProperty<SelectableTextBlock, bool> CanCopyProperty =
        Avalonia.Controls.TextBox.CanCopyProperty.AddOwner<SelectableTextBlock>(o => o.CanCopy);

    public static readonly RoutedEvent<RoutedEventArgs> CopyingToClipboardEvent =
        RoutedEvent.Register<SelectableTextBlock, RoutedEventArgs>(
            nameof(CopyingToClipboard), RoutingStrategies.Bubble);

    static SelectableTextBlock()
    {
        FocusableProperty.OverrideDefaultValue(typeof(SelectableTextBlock), true);
        AffectsRender<SelectableTextBlock>(SelectionStartProperty, SelectionEndProperty, SelectionBrushProperty);
    }

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
    /// Gets or sets a brush that is used for the foreground of selected text
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
    /// Property for determining if the Copy command can be executed.
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
    /// Copies the current selection to the Clipboard.
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
    /// Select all text in the TextBox
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

    protected override void OnGotFocus(FocusChangedEventArgs e)
    {
        base.OnGotFocus(e);

        UpdateCommandStates();
    }

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
