using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;

using AvaloniaTextBox = Avalonia.Controls.TextBox;

namespace AtomUI.Desktop.Controls;

[TemplatePart("PART_TextPresenter", typeof(InputTextPresenter))]
internal class OtpTextBox : AvaloniaTextBox
{
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

    private InputTextPresenter? _textPresenter;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _textPresenter?.HideCaret();
        _textPresenter = e.NameScope.Find<InputTextPresenter>("PART_TextPresenter");
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
        // 光标由 OtpLineEditCell 的 PART_Caret 自绘呈现，
        // 内嵌只读 TextBox 的内部 caret 不参与显示，避免双重光标。
        _textPresenter.HideCaret();
    }
}
