using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;

namespace AtomUI.Desktop.Controls;

internal class QuickJumpArgs
{
    public QuickJumpArgs(int pageNumber)
    {
        PageNumber = pageNumber;
    }
    public int PageNumber { get; set; }
}

internal class QuickJumperBar : TemplatedControl
{
    #region 公共属性定义
    
    public static readonly DirectProperty<QuickJumperBar, string?> JumpToTextProperty =
        AvaloniaProperty.RegisterDirect<QuickJumperBar,  string?>(nameof(JumpToText),
            o => o.JumpToText,
            (o, v) => o.JumpToText = v);
    
    public static readonly DirectProperty<QuickJumperBar, string?> PageTextProperty =
        AvaloniaProperty.RegisterDirect<QuickJumperBar,  string?>(nameof(PageText),
            o => o.PageText,
            (o, v) => o.PageText = v);
    
    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<QuickJumperBar>();
    
    private string? _jumpToText;

    public string? JumpToText
    {
        get => _jumpToText;
        set => SetAndRaise(JumpToTextProperty, ref _jumpToText, value);
    }
    
    private string? _pageText;

    public string? PageText
    {
        get => _pageText;
        set => SetAndRaise(PageTextProperty, ref _pageText, value);
    }
    
    public CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    #endregion

    #region 公共事件定义

    public event EventHandler<QuickJumpArgs>? JumpRequest;

    #endregion

    #region 内部属性定义

    // The pager's quick jumper inputs, bound by the pager that creates the bar.
    internal static readonly StyledProperty<double> QuickJumperSpacingProperty =
        AbstractPagination.QuickJumperSpacingProperty.AddOwner<QuickJumperBar>();

    internal static readonly StyledProperty<double> QuickJumperInputWidthProperty =
        AbstractPagination.QuickJumperInputWidthProperty.AddOwner<QuickJumperBar>();

    internal double QuickJumperSpacing
    {
        get => GetValue(QuickJumperSpacingProperty);
        set => SetValue(QuickJumperSpacingProperty, value);
    }

    internal double QuickJumperInputWidth
    {
        get => GetValue(QuickJumperInputWidthProperty);
        set => SetValue(QuickJumperInputWidthProperty, value);
    }

    #endregion
    
    private LineEdit? _lineEdit;
    
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        ClearLineEditPart();
        _lineEdit = e.NameScope.Find<LineEdit>("PART_PageLineEdit");

        if (_lineEdit != null)
        {
            _lineEdit.KeyUp -= HandleLineEditKeyUp;
            _lineEdit.KeyUp += HandleLineEditKeyUp;
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        ClearLineEditPart();
    }

    private void ClearLineEditPart()
    {
        if (_lineEdit is not null)
        {
            _lineEdit.KeyUp -= HandleLineEditKeyUp;
            _lineEdit = null;
        }
    }

    private void HandleLineEditKeyUp(object? sender, KeyEventArgs e)
    {
        if (sender is LineEdit lineEdit)
        {
            if (e.Key == Key.Enter)
            {
                if (lineEdit.Text is { } text && int.TryParse(text.AsSpan().Trim(), out var pageNumber))
                {
                    JumpRequest?.Invoke(this, new QuickJumpArgs(pageNumber));
                }
                lineEdit.Clear();
            }
        }
    }
}
