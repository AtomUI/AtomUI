using AtomUI.Desktop.Controls.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;

namespace AtomUI.Desktop.Controls;

public partial class TextArea : AbstractTextInput
{
    #region 公共属性定义
    public static readonly StyledProperty<int> LinesProperty =
        AvaloniaProperty.Register<TextArea, int>(nameof(Lines), 2);
    
    public static readonly StyledProperty<bool> IsAutoSizeProperty =
        AvaloniaProperty.Register<TextArea, bool>(nameof(IsAutoSize), false);
    
    public static readonly StyledProperty<bool> IsResizableProperty =
        AvaloniaProperty.Register<TextArea, bool>(nameof(IsResizable));

    public static readonly StyledProperty<IDataTemplate?> InnerLeftContentTemplateProperty =
        AvaloniaProperty.Register<TextArea, IDataTemplate?>(nameof(InnerLeftContentTemplate));

    public static readonly StyledProperty<IDataTemplate?> InnerRightContentTemplateProperty =
        AvaloniaProperty.Register<TextArea, IDataTemplate?>(nameof(InnerRightContentTemplate));
    
    public int Lines
    {
        get => GetValue(LinesProperty);
        set
        {
            ValidateLinesValue(value);
            SetValue(LinesProperty, value);
        }
    }
    
    public bool IsAutoSize
    {
        get => GetValue(IsAutoSizeProperty);
        set => SetValue(IsAutoSizeProperty, value);
    }
    
    public bool IsResizable
    {
        get => GetValue(IsResizableProperty);
        set => SetValue(IsResizableProperty, value);
    }

    public IDataTemplate? InnerLeftContentTemplate
    {
        get => GetValue(InnerLeftContentTemplateProperty);
        set => SetValue(InnerLeftContentTemplateProperty, value);
    }

    public IDataTemplate? InnerRightContentTemplate
    {
        get => GetValue(InnerRightContentTemplateProperty);
        set => SetValue(InnerRightContentTemplateProperty, value);
    }

    #endregion

    #region 内部属性定义

    #endregion

    private ScrollViewer? _scrollViewer;
    private TextAreaDecoratedBox? _textAreaDecoratedBox;
    private ResizeHandle? _resizeHandle;
    private double? _originHeight; // 拖动改变高度的初始值
    private double _minResizeHeight; // 拖动改变高度时允许的最小 TextArea.Height
    private double _maxResizeHeight; // 拖动改变高度时允许的最大 TextArea.Height

    static TextArea()
    {
        AffectsMeasure<TextArea>(IsAutoSizeProperty, LinesProperty);
    }
    
    public TextArea()
    {
    }

    protected override bool AllowsClearForMultilineInput => true;

    protected override AvaloniaProperty? InnerRightContentTemplatePropertyForBinding =>
        InnerRightContentTemplateProperty;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (_textAreaDecoratedBox is not null)
        {
            _textAreaDecoratedBox.Owner = null;
        }

        _textAreaDecoratedBox = e.NameScope.Find<TextAreaDecoratedBox>(AddOnDecoratedBox.AddOnDecoratedBoxPart);
        if (_textAreaDecoratedBox is not null)
        {
            _textAreaDecoratedBox.Owner = this;
        }

        _resizeHandle = e.NameScope.Find<ResizeHandle>("PART_ResizeHandle");
        if (_resizeHandle != null)
        {
            _resizeHandle.Owner = this;
        }

    }

    internal void NotifyScrollViewerCreated(ScrollViewer scrollViewer)
    {
        _scrollViewer = scrollViewer;
        this.SetScrollViewer(scrollViewer);
        NotifyTextViewportCreated(scrollViewer);
    }

    private void ValidateLinesValue(int lines)
    {
        var maxLines = int.MaxValue;
        if (MaxLines > 0)
        {
            maxLines = MaxLines;
        }

        if (lines > maxLines || lines < MinLines)
        {
            throw new ArgumentOutOfRangeException($"Lines must be between {MinLines} and {MaxLines}");
        }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var size           = base.MeasureOverride(availableSize);
        var viewportLayout = _textAreaDecoratedBox?.ScrollViewerLayout;
        if (viewportLayout is null)
        {
            return size;
        }

        if (IsAutoSize || Lines <= 0 || !double.IsNaN(Height))
        {
            viewportLayout.ClearValue(MinHeightProperty);
            return size;
        }

        // Lines only decides the natural height. The scroll viewer's own Min/MaxHeight stay owned by the
        // MinLines/MaxLines measure of the base TextBox, so a taller arranged slot (Grid star row, Flex grow)
        // still stretches the viewport instead of centering a Lines-high band inside the frame.
        var linesHeight = CalculateScrollViewerHeight(Math.Max(Lines, MinLines));
        if (viewportLayout.MinHeight != linesHeight)
        {
            viewportLayout.SetCurrentValue(MinHeightProperty, linesHeight);
            size = base.MeasureOverride(availableSize);
        }

        // Content taller than Lines scrolls inside the viewport and must not grow the desired height.
        var naturalHeight = size.Height - viewportLayout.DesiredSize.Height + linesHeight;
        if (naturalHeight < size.Height)
        {
            size = base.MeasureOverride(availableSize.WithHeight(Math.Min(availableSize.Height, naturalHeight)));
        }
        return size;
    }

    private double CalculateScrollViewerHeight(int lines)
    {
        var fontSize = FontSize;
        var typeface = new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);
        var textRunProperties = new GenericTextRunProperties(typeface, fontSize, fontFeatures: FontFeatures);
        var paragraphProperties = new GenericTextParagraphProperties(
            default, default, true, false, textRunProperties, default, LineHeight, 0, 0);
        using var textLayout = new TextLayout(new LineTextSource(lines), paragraphProperties);
        var verticalSpace = this.GetVerticalSpaceBetweenScrollViewerAndPresenter();
        return Math.Ceiling(textLayout.Height + verticalSpace);
    }
    
    private class LineTextSource : ITextSource
    {
        private readonly int _lines;

        public LineTextSource(int lines)
        {
            _lines = lines;
        }

        public TextRun? GetTextRun(int textSourceIndex)
        {
            if (textSourceIndex >= _lines)
            {
                return null;
            }
            return new TextEndOfLine(1);
        }
    }
    
    internal void NotifyAboutToResize()
    {
        if (!IsResizable)
        {
            _originHeight = null;
            return;
        }

        _originHeight = Bounds.Height;
        SetCurrentValue(HeightProperty, Bounds.Height);
        if (_scrollViewer != null)
        {
            var overhead = Math.Max(0, Bounds.Height - _scrollViewer.Bounds.Height);
            var minScrollHeight = MinLines > 0 ? CalculateScrollViewerHeight(MinLines) : 0;
            _minResizeHeight = overhead + minScrollHeight;
            _maxResizeHeight = MaxLines > 0
                ? overhead + CalculateScrollViewerHeight(MaxLines)
                : double.PositiveInfinity;
        }
        else
        {
            _minResizeHeight = 0;
            _maxResizeHeight = double.PositiveInfinity;
        }
    }

    internal void NotifyResizing(Point delta)
    {
        if (IsResizable && _originHeight != null)
        {
            var height = _originHeight.Value + delta.Y;
            height = Math.Max(_minResizeHeight, Math.Min(height, _maxResizeHeight));
            SetCurrentValue(HeightProperty, height);
        }
    }
    
    internal void NotifyResizeCompleted()
    {
        _originHeight = null;
    }
    
}
