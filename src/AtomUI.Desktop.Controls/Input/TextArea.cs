using AtomUI.Desktop.Controls.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Multi-line text input. Its height follows <see cref="Lines" />, or the content while
/// <see cref="IsAutoSize" /> is set, and <see cref="IsResizable" /> adds a grip for dragging the height.
/// </summary>
public partial class TextArea : AbstractTextInput
{
    #region 公共属性定义
    /// <summary>
    /// Identifies the <see cref="Lines" /> property.
    /// </summary>
    public static readonly StyledProperty<int> LinesProperty =
        AvaloniaProperty.Register<TextArea, int>(nameof(Lines), 2);
    
    /// <summary>
    /// Identifies the <see cref="IsAutoSize" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsAutoSizeProperty =
        AvaloniaProperty.Register<TextArea, bool>(nameof(IsAutoSize), false);
    
    /// <summary>
    /// Identifies the <see cref="IsResizable" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsResizableProperty =
        AvaloniaProperty.Register<TextArea, bool>(nameof(IsResizable));

    /// <summary>
    /// Identifies the <see cref="InnerLeftContentTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> InnerLeftContentTemplateProperty =
        AvaloniaProperty.Register<TextArea, IDataTemplate?>(nameof(InnerLeftContentTemplate));

    /// <summary>
    /// Identifies the <see cref="InnerRightContentTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> InnerRightContentTemplateProperty =
        AvaloniaProperty.Register<TextArea, IDataTemplate?>(nameof(InnerRightContentTemplate));
    
    /// <summary>
    /// Number of text lines the text area is tall enough to show without scrolling; it sets the natural height.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The assigned value is below <c>MinLines</c> or above a positive <c>MaxLines</c>.
    /// </exception>
    /// <remarks>
    /// It has no effect while <see cref="IsAutoSize" /> is <see langword="true" />, while <c>Height</c> is set
    /// explicitly or while it is not positive. A value below <c>MinLines</c> counts as <c>MinLines</c>.
    /// </remarks>
    public int Lines
    {
        get => GetValue(LinesProperty);
        set
        {
            ValidateLinesValue(value);
            SetValue(LinesProperty, value);
        }
    }
    
    /// <summary>
    /// Lets the text area take its height from its content instead of <see cref="Lines" />.
    /// </summary>
    public bool IsAutoSize
    {
        get => GetValue(IsAutoSizeProperty);
        set => SetValue(IsAutoSizeProperty, value);
    }
    
    /// <summary>
    /// When <see langword="true" />, shows a resize grip that changes <c>Height</c> while it is dragged.
    /// The default, <see langword="false" />, hides the grip.
    /// </summary>
    /// <remarks>
    /// The dragged height stays between the heights of <c>MinLines</c> and <c>MaxLines</c> lines.
    /// </remarks>
    public bool IsResizable
    {
        get => GetValue(IsResizableProperty);
        set => SetValue(IsResizableProperty, value);
    }

    /// <summary>
    /// Template of the add-on shown inside the text area frame, before the text.
    /// </summary>
    /// <remarks>
    /// A <see langword="null" /> template shows no add-on.
    /// </remarks>
    public IDataTemplate? InnerLeftContentTemplate
    {
        get => GetValue(InnerLeftContentTemplateProperty);
        set => SetValue(InnerLeftContentTemplateProperty, value);
    }

    /// <summary>
    /// Template of the add-on shown inside the text area frame, after the text.
    /// </summary>
    /// <remarks>
    /// A <see langword="null" /> template shows no add-on.
    /// </remarks>
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
    private double? _originHeight; // Height when the resize drag started; null while no drag is in progress
    private double _minResizeHeight; // Smallest Height allowed while the resize grip is dragged
    private double _maxResizeHeight; // Largest Height allowed while the resize grip is dragged

    static TextArea()
    {
        AffectsMeasure<TextArea>(IsAutoSizeProperty, LinesProperty);
    }
    
    public TextArea()
    {
    }

    /// <summary>
    /// Always <see langword="true" />, so the base class also offers the clear button for this multi-line input.
    /// </summary>
    /// <remarks>
    /// The button still appears only while <c>IsAllowClear</c> is set and the text can be cleared.
    /// </remarks>
    protected override bool AllowsClearForMultilineInput => true;

    /// <summary>
    /// Returns <see cref="InnerRightContentTemplateProperty" />, the property that carries
    /// <see cref="InnerRightContentTemplate" />, where the base class returns <see langword="null" />.
    /// </summary>
    protected override AvaloniaProperty? InnerRightContentTemplatePropertyForBinding =>
        InnerRightContentTemplateProperty;

    /// <summary>
    /// Called when the control template is applied. After the base implementation it makes this text area the owner
    /// of the decorated box and the resize grip of the new template, and releases the previous ones.
    /// </summary>
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

    /// <summary>
    /// Measures the text area and keeps its natural height at <see cref="Lines" /> lines of text, so content with
    /// more lines scrolls inside the viewport instead of growing the control.
    /// </summary>
    /// <remarks>
    /// <see cref="Lines" /> is skipped while <see cref="IsAutoSize" /> is <see langword="true" />, while
    /// <c>Height</c> is set explicitly or while <see cref="Lines" /> is not positive; then the viewport's minimum
    /// height is cleared. The base implementation is called first and again when that minimum height changes.
    /// </remarks>
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
