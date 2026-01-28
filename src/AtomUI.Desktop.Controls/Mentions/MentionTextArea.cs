using AtomUI.Desktop.Controls.Themes;
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

    private TextPresenter? _textPresenter;
    internal Mentions? Owner;
    private Rect? _currentTriggerBounds;
    private string? _currentTriggerText;
    private string? _currentPredicate;
    
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
        _textPresenter = e.NameScope.Find<TextPresenter>(TextAreaThemeConstants.TextPresenterPart);
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
        if (!string.IsNullOrEmpty(Text) && CaretIndex >= 1)
        {
            var  triggerFound = false;
            var  index        = CaretIndex;
            while (index > 0)
            {
                var ch = Text[index - 1];
                if (char.IsControl(ch) || char.IsWhiteSpace(ch))
                {
                    break;
                }
                if (TriggerPrefix != null && TriggerPrefix.Contains(ch.ToString()))
                {
                    _currentTriggerText = ch.ToString();
                    triggerFound        = true;
                    break;
                }
              
                index--;
            }
            var length = CaretIndex - index;
            if (triggerFound)
            {
                var triggerIndex = index - 1;
                var presenter    = this.GetTextPresenter();
                var textLayout   = presenter.TextLayout;
                _currentTriggerBounds = textLayout.HitTestTextPosition(triggerIndex);
                _currentPredicate     = Text.Substring(index, length);
                TriggerState          = true;
            }
            else
            {
                TriggerState = false;
            }
        }
        else if (CaretIndex == 0 || string.IsNullOrWhiteSpace(Text))
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
        if (string.IsNullOrWhiteSpace(Text))
        {
            SetCurrentValue(FilterValueProperty, null);
            return;
        }
        var triggerFound = false;
        var index        = CaretIndex;
        var triggerIndex = -1;
        while (index > 0)
        {
            var ch = Text[index - 1];
            if (char.IsControl(ch) || char.IsWhiteSpace(ch))
            {
                break;
            }
            if (TriggerPrefix != null && TriggerPrefix.Contains(ch.ToString()))
            {
                triggerFound = true;
                triggerIndex = index - 1;
                break;
            }
            index--;
        }

        if (triggerFound)
        {
            
            var valueIndex = triggerIndex + 1;
            if (valueIndex <= Text.Length)
            {
                SetCurrentValue(FilterValueProperty, Text.Substring(valueIndex, CaretIndex - valueIndex));
            }
        }
        else
        {
            SetCurrentValue(FilterValueProperty, null);
        }
    }
    
    internal Rect GetTextPresenterBounds()
    {
        if (_textPresenter != null)
        {
            var offset = _textPresenter.TranslatePoint(new Point(0, 0), this) ?? new Point(0, 0);
            return new Rect(offset.X, offset.Y, _textPresenter.DesiredSize.Width, _textPresenter.DesiredSize.Height);
        }
        return default;
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
        this.SnapshotUndoRedo();
        var  triggerIndex = -1;
        var  foundSplit   = false;
        var  foundSpace   = false;
        var  text         = Text ?? string.Empty;
        char triggerCh    = default;
        if (!string.IsNullOrEmpty(Text))
        {
            var currentIndex = CaretIndex;
            while (currentIndex > 0)
            {
                var ch = Text[currentIndex - 1];
                if (char.IsWhiteSpace(ch))
                {
                    break;
                }
                if (TriggerPrefix != null && TriggerPrefix.Contains(ch.ToString()))
                {
                    triggerCh    = ch;
                    triggerIndex = currentIndex - 1;
                    break;
                }
                currentIndex--;
            }
            if (triggerIndex != 0)
            {
                if ($"{Text[triggerIndex - 1]}" == split)
                {
                    foundSplit = true;
                }
                else if ($"{Text[triggerIndex - 1]}" == " ")
                {
                    foundSpace = true;
                }
            }
        }

        if (triggerIndex != -1)
        {
            SetCurrentValue(SelectionStartProperty, triggerIndex);
            SetCurrentValue(SelectionEndProperty, CaretIndex);
        }
        
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