using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;

using AvaloniaTextBox = Avalonia.Controls.TextBox;

namespace AtomUI.Desktop.Controls;

[TemplatePart("PART_TextPresenter", typeof(TextPresenter))]
internal class OtpTextBox : AvaloniaTextBox
{
    #region 内部属性定义

    internal static readonly StyledProperty<string?> SourcePlaceholderTextProperty =
        AvaloniaProperty.Register<OtpTextBox, string?>(nameof(SourcePlaceholderText));

    internal static readonly StyledProperty<bool> IsCellActiveProperty =
        AvaloniaProperty.Register<OtpTextBox, bool>(nameof(IsCellActive));

    internal string? SourcePlaceholderText
    {
        get => GetValue(SourcePlaceholderTextProperty);
        set => SetValue(SourcePlaceholderTextProperty, value);
    }

    internal bool IsCellActive
    {
        get => GetValue(IsCellActiveProperty);
        set => SetValue(IsCellActiveProperty, value);
    }

    #endregion

    private TextPresenter? _textPresenter;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _textPresenter?.HideCaret();
        _textPresenter = e.NameScope.Find<TextPresenter>("PART_TextPresenter");
        ConfigureCaretHost();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        _textPresenter?.HideCaret();
        _textPresenter = null;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == TextProperty ||
            change.Property == SourcePlaceholderTextProperty ||
            change.Property == IsCellActiveProperty ||
            change.Property == IsEffectivelyEnabledProperty)
        {
            ConfigureCaretHost();
        }
    }

    private void ConfigureCaretHost()
    {
        var displayText = Text;
        var caretIndex  = displayText?.Length ?? 0;
        SetCurrentValue(PlaceholderTextProperty,
            IsCellActive && string.IsNullOrEmpty(displayText) ? null : SourcePlaceholderText);
        CaretIndex      = caretIndex;
        SelectionStart  = caretIndex;
        SelectionEnd    = caretIndex;

        if (_textPresenter is null)
        {
            return;
        }

        _textPresenter.SetCurrentValue(TextPresenter.TextProperty, displayText);
        _textPresenter.CaretIndex              = caretIndex;
        _textPresenter.SelectionStart          = caretIndex;
        _textPresenter.SelectionEnd            = caretIndex;
        _textPresenter.ShowSelectionHighlight  = IsCellActive;
        // The caret is drawn by the PART_Caret part of the OtpLineEditCell that hosts this text box.
        // The built-in caret of this read-only text box stays hidden so that no second caret appears.
        _textPresenter.HideCaret();
    }
}
