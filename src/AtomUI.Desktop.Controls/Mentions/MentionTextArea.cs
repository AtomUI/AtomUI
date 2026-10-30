using AtomUI.Desktop.Controls.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;

namespace AtomUI.Desktop.Controls;

internal class MentionTextArea : TextArea
{
    public static readonly StyledProperty<IList<string>?> TriggerPrefixProperty =
        AvaloniaProperty.Register<MentionTextArea, IList<string>?>(nameof(TriggerPrefix));
    
    public static readonly StyledProperty<string?> FilterValueProperty =
        AvaloniaProperty.Register<MentionTextArea, string?>(nameof(FilterValue));
           
    public static readonly StyledProperty<bool> IsDropDownOpenProperty =
        AbstractAutoComplete.IsDropDownOpenProperty.AddOwner<MentionTextArea>();

    public IList<string>? TriggerPrefix
    {
        get => GetValue(TriggerPrefixProperty);
        set => SetValue(TriggerPrefixProperty, value);
    }
    
    public string? FilterValue
    {
        get => GetValue(FilterValueProperty);
        set => SetValue(FilterValueProperty, value);
    }

    public bool IsDropDownOpen
    {
        get => GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }
    
    #region 公共事件定义

    public event EventHandler<ShowMentionCandidateRequestEventArgs>? CandidateOpenRequest;
    public event EventHandler<EventArgs>? CandidateCloseRequest;

    #endregion

    #region 内部属性定义

    internal static readonly DirectProperty<MentionTextArea, bool> TriggerStateProperty =
        AvaloniaProperty.RegisterDirect<MentionTextArea, bool>(
            nameof(TriggerState),
            o => o.TriggerState,
            (o, v) => o.TriggerState = v);
    
    private bool _triggerState;
    
    internal bool TriggerState
    {
        get => _triggerState;
        set => SetAndRaise(TriggerStateProperty, ref _triggerState, value);
    }

    #endregion

    protected override Type StyleKeyOverride => typeof(TextArea);

    private TextPresenter? _textPresenter;
    internal Mentions? Owner;
    private Rect? _currentTriggerBounds;
    private string? _currentTriggerText;
    private string? _currentPredicate;

    private readonly record struct MentionTriggerState(
        int TriggerIndex,
        string TriggerText,
        string Predicate,
        Rect TriggerBounds);
    
    static MentionTextArea()
    {
        LinesProperty.OverrideDefaultValue<MentionTextArea>(1);
        CaretIndexProperty.Changed.AddClassHandler<MentionTextArea>((textArea, args) => textArea.HandleCaretIndexChanged());
    }

    protected override void OnTextInput(TextInputEventArgs e)
    {
        base.OnTextInput(e);
        CheckTriggerState();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _textPresenter = e.NameScope.Find<TextPresenter>("PART_TextPresenter");
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == TextProperty ||
            change.Property == CaretIndexProperty)
        {
            CheckTriggerState();
        }
    }

    private void CheckTriggerState()
    {
        if (TryGetCurrentTriggerState(out var triggerState))
        {
            _currentTriggerBounds = triggerState.TriggerBounds;
            _currentPredicate     = triggerState.Predicate;
            _currentTriggerText   = triggerState.TriggerText;
            TriggerState          = true;
        }
        else
        {
            TriggerState = false;
        }
        
        if (TriggerState)
        {
            if (!IsDropDownOpen)
            {
                if (_currentTriggerBounds != null && _currentPredicate != null && _currentTriggerText != null)
                {
                    CandidateOpenRequest?.Invoke(this, new ShowMentionCandidateRequestEventArgs(_currentTriggerBounds.Value, _currentPredicate, _currentTriggerText));
                }
            }
        }
        else
        {
            if (IsDropDownOpen)
            {
                CandidateCloseRequest?.Invoke(this, EventArgs.Empty);
            }
            _currentPredicate     = null;
            _currentTriggerBounds = null;
            _currentTriggerText   = null;
        }
    }

    private void HandleCaretIndexChanged()
    {
        if (TryGetCurrentTriggerState(out var triggerState))
        {
            SetCurrentValue(FilterValueProperty, triggerState.Predicate);
        }
        else
        {
            SetCurrentValue(FilterValueProperty, null);
        }
    }
    
    internal Rect GetTriggerBounds(Rect triggerBounds, Control? relativeControl = null)
    {
        if (relativeControl != null && _textPresenter != null)
        {
            var offset = _textPresenter.TranslatePoint(triggerBounds.Position, relativeControl) ?? triggerBounds.Position;
            return new Rect(offset, triggerBounds.Size);
        }
        return triggerBounds;
    }

    private bool TryGetTriggerPrefix(char ch, out string triggerText)
    {
        var triggerPrefix = TriggerPrefix;
        if (triggerPrefix != null)
        {
            for (var i = 0; i < triggerPrefix.Count; i++)
            {
                var prefix = triggerPrefix[i];
                if (prefix?.Length == 1 && prefix[0] == ch)
                {
                    triggerText = prefix;
                    return true;
                }
            }
        }

        triggerText = string.Empty;
        return false;
    }

    private bool TryGetCurrentTriggerState(out MentionTriggerState state)
    {
        state = default;
        var text = Text;
        if (string.IsNullOrEmpty(text) || CaretIndex < 1 || CaretIndex > text.Length)
        {
            return false;
        }

        var index = CaretIndex;
        while (index > 0)
        {
            var ch = text[index - 1];
            if (char.IsControl(ch) || char.IsWhiteSpace(ch))
            {
                break;
            }
            if (TryGetTriggerPrefix(ch, out var triggerText))
            {
                var triggerIndex  = index - 1;
                var presenter = _textPresenter ??
                    throw new InvalidOperationException("PART_TextPresenter is not available for mention geometry.");
                var textLayout    = presenter.TextLayout;
                var triggerBounds = textLayout.HitTestTextPosition(triggerIndex);
                var predicate     = text.Substring(index, CaretIndex - index);
                state = new MentionTriggerState(triggerIndex, triggerText, predicate, triggerBounds);
                return true;
            }

            index--;
        }

        return false;
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        Owner?.NotifyTextAreaPointerPressed(e);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        Owner?.NotifyTextAreaPointerReleased(e);
    }

    internal void InsertMentionOption(string value, string? split)
    {
        if (!TryGetCurrentTriggerState(out var triggerState))
        {
            return;
        }

        this.SnapshotUndoRedo();
        var  triggerIndex = triggerState.TriggerIndex;
        var  foundSplit   = false;
        var  foundSpace   = false;
        var  text         = Text ?? string.Empty;
        char triggerCh    = text[triggerIndex];
        if (triggerIndex > 0)
        {
            var previousCh = text[triggerIndex - 1];
            if (split is { Length: 1 } && previousCh == split[0])
            {
                foundSplit = true;
            }
            else if (previousCh == ' ')
            {
                foundSpace = true;
            }
        }

        SetCurrentValue(SelectionStartProperty, triggerIndex);
        SetCurrentValue(SelectionEndProperty, CaretIndex);
        
        if (!string.IsNullOrWhiteSpace(split))
        {
            if (foundSplit)
            {
                value = triggerCh + value + split;
            }
            else
            {
                value = split + triggerCh + value + split;
            }
        }
        else
        {
            if (foundSpace)
            {
                value = triggerCh + value + " ";
            }
            else
            {
                if (triggerIndex > 0)
                {
                    value = " " + triggerCh + value + " ";
                }
                else
                {
                    value = triggerCh + value + " ";
                }
            }
        }
        this.HandleTextInput(value);
    }
    
    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Return)
        {
            if (IsDropDownOpen)
            {
                return;
            }
        }
        base.OnKeyDown(e);
    }
}
