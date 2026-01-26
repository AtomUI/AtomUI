using AtomUI.Desktop.Controls.Themes;
using AtomUI.Desktop.Controls.Utils;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls;

internal class MentionTextArea : TextArea
{
    public static readonly StyledProperty<IList<string>?> TriggerPrefixProperty =
        AvaloniaProperty.Register<MentionTextArea, IList<string>?>(nameof(TriggerPrefix));
    
    public IList<string>? TriggerPrefix
    {
        get => GetValue(TriggerPrefixProperty);
        set => SetValue(TriggerPrefixProperty, value);
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
    private int _lastTriggerCached = -1;
    
    static MentionTextArea()
    {
        LinesProperty.OverrideDefaultValue<MentionTextArea>(1);
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
        if (change.Property == TextProperty)
        {
            if (change.Property == TextProperty)
            {
                _lastTriggerCached = HashCode.Combine(Text);
            }
            CheckTriggerState();
        }
        else if (change.Property == CaretIndexProperty)
        {
            var cached = HashCode.Combine(Text);
            if (cached == _lastTriggerCached)
            {
                CheckTriggerState();
            }
        }
    }

    private void CheckTriggerState()
    {
        if (!string.IsNullOrEmpty(Text) && CaretIndex >= 1)
        {
            var triggerFound = false;
            var index        = CaretIndex;
            char triggerCh    = default;
            while (index > 0)
            {
                var ch = Text[index - 1];
                if (TriggerPrefix != null && TriggerPrefix.Contains(ch.ToString()))
                {
                    triggerCh    = ch;
                    triggerFound = true;
                    break;
                }
                if (char.IsControl(ch) || char.IsWhiteSpace(ch))
                {
                    break;
                }
                index--;
            }
            var length = CaretIndex - index;
            if (triggerFound)
            {
                var triggerIndex  = index - 1;
                var presenter     = this.GetTextPresenter();
                var textLayout    = presenter.TextLayout;
                var triggerBounds = textLayout.HitTestTextPosition(triggerIndex);
                var predicate     = Text.Substring(index, length);
                if (!TriggerState)
                {
                    TriggerState = true;
                    CandidateOpenRequest?.Invoke(this, new ShowMentionCandidateRequestEventArgs(triggerBounds, predicate, triggerCh.ToString()));
                }
            }
            else
            {
                if (TriggerState)
                {
                    TriggerState = false;
                    CandidateCloseRequest?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        else if (CaretIndex == 0 || string.IsNullOrWhiteSpace(Text))
        {
            if (TriggerState)
            {
                TriggerState = false;
                CandidateCloseRequest?.Invoke(this, EventArgs.Empty);
            }
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
        var  text         = Text ?? string.Empty;
        char triggerCh    = default; 
        if (!string.IsNullOrEmpty(Text))
        {
            var currentIndex = CaretIndex;
            while (currentIndex > 0)
            {
                var ch = Text[currentIndex - 1];
                if (TriggerPrefix != null && TriggerPrefix.Contains(ch.ToString()))
                {
                    triggerCh    = ch;
                    triggerIndex = currentIndex - 1;
                    break;
                }
                currentIndex--;
            }
            if (triggerIndex != 0 && $"{Text[triggerIndex - 1]}" == split)
            {
                foundSplit = true;
            }
        }

        if (triggerIndex != -1)
        {
            var sb = StringBuilderCache.Acquire(text.Length);
            sb.Append(text);
            sb.Remove(triggerIndex, CaretIndex - triggerIndex);
            SetCurrentValue(TextProperty, StringBuilderCache.GetStringAndRelease(sb));
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
            value = triggerCh + value;
        }
        this.HandleTextInput(value);
    }
}