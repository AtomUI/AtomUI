using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Presenters;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Avalonia.VisualTree;

namespace AtomUI.Controls.Commons;

/// <summary>Displays text with an optional action for expanding or collapsing overflow.</summary>
public abstract class AbstractEllipsis : Control
{
    #region 公共属性定义

    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<AbstractEllipsis, string?>(nameof(Text));

    public static readonly StyledProperty<int> RowsProperty =
        AvaloniaProperty.Register<AbstractEllipsis, int>(nameof(Rows), 1);

    public static readonly StyledProperty<EllipsisDirection> DirectionProperty =
        AvaloniaProperty.Register<AbstractEllipsis, EllipsisDirection>(nameof(Direction), EllipsisDirection.End);

    public static readonly StyledProperty<object?> ExpandContentProperty =
        AvaloniaProperty.Register<AbstractEllipsis, object?>(nameof(ExpandContent));

    public static readonly StyledProperty<object?> CollapseContentProperty =
        AvaloniaProperty.Register<AbstractEllipsis, object?>(nameof(CollapseContent));

    public static readonly StyledProperty<bool> IsExpandedProperty =
        AvaloniaProperty.Register<AbstractEllipsis, bool>(nameof(IsExpanded), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<FontFamily> FontFamilyProperty =
        TextElement.FontFamilyProperty.AddOwner<AbstractEllipsis>();

    public static readonly StyledProperty<double> FontSizeProperty =
        TextElement.FontSizeProperty.AddOwner<AbstractEllipsis>();

    public static readonly StyledProperty<FontStyle> FontStyleProperty =
        TextElement.FontStyleProperty.AddOwner<AbstractEllipsis>();

    public static readonly StyledProperty<FontWeight> FontWeightProperty =
        TextElement.FontWeightProperty.AddOwner<AbstractEllipsis>();

    public static readonly StyledProperty<FontStretch> FontStretchProperty =
        TextElement.FontStretchProperty.AddOwner<AbstractEllipsis>();

    public static readonly StyledProperty<FontFeatureCollection?> FontFeaturesProperty =
        TextElement.FontFeaturesProperty.AddOwner<AbstractEllipsis>();

    public static readonly StyledProperty<double> LetterSpacingProperty =
        TextElement.LetterSpacingProperty.AddOwner<AbstractEllipsis>();

    public static readonly StyledProperty<IBrush?> ForegroundProperty =
        TextElement.ForegroundProperty.AddOwner<AbstractEllipsis>();

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public int Rows
    {
        get => GetValue(RowsProperty);
        set => SetValue(RowsProperty, value);
    }

    public EllipsisDirection Direction
    {
        get => GetValue(DirectionProperty);
        set => SetValue(DirectionProperty, value);
    }

    public object? ExpandContent
    {
        get => GetValue(ExpandContentProperty);
        set => SetValue(ExpandContentProperty, value);
    }

    public object? CollapseContent
    {
        get => GetValue(CollapseContentProperty);
        set => SetValue(CollapseContentProperty, value);
    }

    public bool IsExpanded
    {
        get => GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    public FontFamily FontFamily
    {
        get => GetValue(FontFamilyProperty);
        set => SetValue(FontFamilyProperty, value);
    }

    public double FontSize
    {
        get => GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    public FontStyle FontStyle
    {
        get => GetValue(FontStyleProperty);
        set => SetValue(FontStyleProperty, value);
    }

    public FontWeight FontWeight
    {
        get => GetValue(FontWeightProperty);
        set => SetValue(FontWeightProperty, value);
    }

    public FontStretch FontStretch
    {
        get => GetValue(FontStretchProperty);
        set => SetValue(FontStretchProperty, value);
    }

    public FontFeatureCollection? FontFeatures
    {
        get => GetValue(FontFeaturesProperty);
        set => SetValue(FontFeaturesProperty, value);
    }

    public double LetterSpacing
    {
        get => GetValue(LetterSpacingProperty);
        set => SetValue(LetterSpacingProperty, value);
    }

    public IBrush? Foreground
    {
        get => GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }

    #endregion

    #region 公共事件定义

    public static readonly RoutedEvent<RoutedEventArgs> ExpandedChangedEvent =
        RoutedEvent.Register<AbstractEllipsis, RoutedEventArgs>(nameof(ExpandedChanged), RoutingStrategies.Bubble);

    public static readonly RoutedEvent<TappedEventArgs> ContentTappedEvent =
        RoutedEvent.Register<AbstractEllipsis, TappedEventArgs>(nameof(ContentTapped), RoutingStrategies.Bubble);

    public event EventHandler<RoutedEventArgs>? ExpandedChanged
    {
        add => AddHandler(ExpandedChangedEvent, value);
        remove => RemoveHandler(ExpandedChangedEvent, value);
    }

    public event EventHandler<TappedEventArgs>? ContentTapped
    {
        add => AddHandler(ContentTappedEvent, value);
        remove => RemoveHandler(ContentTappedEvent, value);
    }

    #endregion

    #region 内部属性定义

    internal string? DisplayedText { get; private set; }

    internal int TextLineCount => _textLayout?.TextLines.Count ?? 0;

    #endregion

    private TextLayout? _textLayout;
    private LayoutInputs? _inputs;
    private IBrush? _layoutForeground;
    private ContentPresenter? _actionPresenter;
    private double _actionWidth;
    private bool _overflows;
    private bool _actionOnNewLine;
    private string? _textWithBoundaries;
    private int[]? _boundaries;

    static AbstractEllipsis()
    {
        AffectsMeasure<AbstractEllipsis>(TextProperty, RowsProperty, DirectionProperty, ExpandContentProperty,
            CollapseContentProperty, IsExpandedProperty, FontFamilyProperty, FontSizeProperty, FontStyleProperty,
            FontWeightProperty, FontStretchProperty, FontFeaturesProperty, LetterSpacingProperty, FlowDirectionProperty);
        AffectsRender<AbstractEllipsis>(ForegroundProperty);
        TappedEvent.AddClassHandler<AbstractEllipsis>(static (control, e) => control.HandleContentTap(e));
    }

    protected override bool BypassFlowDirectionPolicies => true;

    /// <summary>Resolves the line height in DIPs. NaN uses the font metrics.</summary>
    protected virtual double ResolveLineHeight() => double.NaN;

    /// <summary>Resolves the foreground applied to the operation presenter.</summary>
    protected virtual IBrush? ResolveActionForeground() => Foreground;

    /// <summary>Applies the current operation foreground without invalidating truncation.</summary>
    protected void RefreshActionForeground()
    {
        if (_actionPresenter is { } presenter)
        {
            presenter.Foreground = ResolveActionForeground();
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsExpandedProperty)
        {
            RaiseEvent(new RoutedEventArgs(ExpandedChangedEvent));
        }
        else if (change.Property == ForegroundProperty)
        {
            RefreshActionForeground();
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        SetActionContent(null);
        ClearLayout();
        InvalidateMeasure();
        base.OnDetachedFromVisualTree(e);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        if (string.IsNullOrEmpty(Text))
        {
            SetActionContent(null);
            ClearLayout();
            return default;
        }

        var inputs = new LayoutInputs(Text, availableSize.Width, Math.Max(1, Rows), Direction, IsExpanded,
            new Typeface(FontFamily, FontStyle, FontWeight, FontStretch), FontSize, FontFeatures, LetterSpacing,
            FlowDirection, ResolveLineHeight());
        var sameInputs = _inputs == inputs;
        TextLayout? fullLayout = null;
        if (!sameInputs)
        {
            fullLayout = CreateLayout(inputs.Text, inputs, Foreground);
            _overflows = !double.IsPositiveInfinity(inputs.Width) &&
                         (fullLayout.TextLines.Count > inputs.Rows || fullLayout.Width > inputs.Width ||
                          inputs.Width == 0);
        }

        SetActionContent(_overflows ? inputs.IsExpanded ? CollapseContent : ExpandContent : null);
        _actionPresenter?.Measure(Size.Infinity);
        var actionSize = _actionPresenter?.DesiredSize ?? default;
        if (sameInputs && _actionWidth == actionSize.Width)
        {
            return GetDesiredSize(actionSize);
        }

        var text = inputs.Text;
        var layout = fullLayout ?? CreateLayout(text, inputs, Foreground);
        if (_overflows && !inputs.IsExpanded)
        {
            layout.Dispose();
            (text, layout) = FitText(inputs, actionSize.Width);
        }

        _actionOnNewLine = _actionPresenter is not null && inputs.IsExpanded &&
                           layout.TextLines[^1].WidthIncludingTrailingWhitespace + actionSize.Width > inputs.Width;
        _inputs = inputs;
        _actionWidth = actionSize.Width;
        ReplaceLayout(text, layout, Foreground);
        return GetDesiredSize(actionSize);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        if (_actionPresenter is { } presenter && _textLayout is { } layout)
        {
            var line = layout.TextLines[^1];
            var action = presenter.DesiredSize;
            var lineHeight = _actionOnNewLine ? GetNextLineHeight(layout) : line.Height;
            var x = _actionOnNewLine ? 0 : line.WidthIncludingTrailingWhitespace;
            var y = _actionOnNewLine ? layout.Height : layout.Height - line.Height;
            if (FlowDirection == FlowDirection.RightToLeft)
            {
                x = Math.Max(0, finalSize.Width - x - action.Width);
            }

            presenter.Arrange(new Rect(new Point(x, y + Math.Max(0, (lineHeight - action.Height) / 2)), action));
        }

        return finalSize;
    }

    public override void Render(DrawingContext context)
    {
        if (_textLayout is null || _inputs is not { } inputs || DisplayedText is not { } text)
        {
            return;
        }

        var foreground = Foreground;
        if (!ReferenceEquals(foreground, _layoutForeground))
        {
            // Avalonia stores the foreground with the text runs. Rebuild only the chosen text.
            _textLayout.Dispose();
            _textLayout = CreateLayout(text, inputs, foreground);
            _layoutForeground = foreground;
        }

        var layout = _textLayout!;
        context.DrawRectangle(Brushes.Transparent, null, new Rect(Bounds.Size));
        if (inputs.FlowDirection == FlowDirection.LeftToRight)
        {
            layout.Draw(context, default);
            return;
        }

        var y = 0d;
        for (var index = 0; index < layout.TextLines.Count; index++)
        {
            var line = layout.TextLines[index];
            var x = Math.Max(0, Bounds.Width - line.WidthIncludingTrailingWhitespace);
            if (index == layout.TextLines.Count - 1 && !_actionOnNewLine)
            {
                x = Math.Max(_actionWidth, x);
            }

            line.Draw(context, new Point(x, y));
            y += line.Height;
        }
    }

    private void SetActionContent(object? content)
    {
        if (content is null)
        {
            if (_actionPresenter is { } current)
            {
                current.Tapped -= HandleActionTap;
                current.Content = null;
                VisualChildren.Remove(current);
                LogicalChildren.Remove(current);
                _actionPresenter = null;
            }

            return;
        }

        if (_actionPresenter is null)
        {
            // Visibility and content are known at measurement time, so the presenter is created
            // and attached only after the text has been measured and found to overflow.
            var presenter = new ContentPresenter { Background = Brushes.Transparent };
            _actionPresenter = presenter;
            RefreshActionForeground();
            presenter.Tapped += HandleActionTap;
            LogicalChildren.Add(presenter);
            VisualChildren.Add(presenter);
        }

        if (!Equals(_actionPresenter.Content, content))
        {
            _actionPresenter.Content = content;
            if (!((ILogical)_actionPresenter).IsAttachedToLogicalTree)
            {
                _actionPresenter.UpdateChild();
            }
        }
    }

    private void HandleActionTap(object? sender, TappedEventArgs e)
    {
        e.Handled = true;
        SetCurrentValue(IsExpandedProperty, !IsExpanded);
    }

    private void HandleContentTap(TappedEventArgs e)
    {
        // Keep the original Tapped route and Handled state independent of ContentTapped.
        var positionSource = (e.Source as Visual ?? this).GetPresentationSource()?.RootVisual;
        var pointer = new PointerEventArgs(null, e.Source, e.Pointer, positionSource, e.GetPosition(null),
            e.Timestamp, PointerPointProperties.None, e.KeyModifiers);
        RaiseEvent(new TappedEventArgs(ContentTappedEvent, pointer));
    }

    private Size GetDesiredSize(Size action)
    {
        var layout = _textLayout!;
        var width = layout.WidthIncludingTrailingWhitespace;
        var height = layout.Height;
        if (_actionPresenter is not null)
        {
            var last = layout.TextLines[^1];
            if (_actionOnNewLine)
            {
                width = Math.Max(width, action.Width);
                height += Math.Max(GetNextLineHeight(layout), action.Height);
            }
            else
            {
                width = Math.Max(width, last.WidthIncludingTrailingWhitespace + action.Width);
                height += Math.Max(0, action.Height - last.Height);
            }
        }

        return new Size(width, height);
    }

    private static double GetNextLineHeight(TextLayout layout) =>
        double.IsNaN(layout.LineHeight) ? layout.TextLines[^1].Height : layout.LineHeight;

    private (string Text, TextLayout Layout) FitText(LayoutInputs inputs, double actionWidth)
    {
        const string marker = "…";
        var foreground = Foreground;
        if (actionWidth >= inputs.Width)
        {
            return (marker, CreateLayout(marker, inputs, foreground));
        }

        if (_textWithBoundaries != inputs.Text)
        {
            _textWithBoundaries = inputs.Text;
            _boundaries = StringInfo.ParseCombiningCharacters(inputs.Text);
        }

        var boundaries = _boundaries!;
        var count = boundaries.Length;
        var best = 0;
        var chosen = (Text: marker, Layout: (TextLayout?)null);
        var high = inputs.Direction == EllipsisDirection.Middle ? count / 2 : count;
        var right = Probe(high);
        if (right.Rows > inputs.Rows)
        {
            high = FindLast(1, high - 1, retain => Probe(retain).Rows <= inputs.Rows);
            right = Probe(high);
        }

        while (high > best)
        {
            var row = right.Rows;
            // Wrapping can make a previously rejected candidate fit when its last line starts
            // farther into the text. Search the longest row band first, then step below it.
            var low = row == 1 ? 1 : FindLast(best + 1, high - 1, retain => Probe(retain).Rows < row) + 1;
            if (low < high && high > best)
            {
                FitBand(low, Probe(low), high, right);
            }

            high = low - 1;
            if (high > best)
            {
                right = Probe(high);
            }
        }

        return (chosen.Text, chosen.Layout ?? CreateLayout(marker, inputs, foreground));

        void FitBand(int low, Candidate left, int high, Candidate right)
        {
            if (high <= best)
            {
                return;
            }

            if (left.LastBreak == right.LastBreak || left.DistanceFromEnd == right.DistanceFromEnd)
            {
                // A stable last-line start lets us bisect the remaining width. Shaping can
                // adjust glyph advances at the cut, so it need not be strictly monotonic.
                if (left.Fits)
                {
                    FindLast(best + 1, high - 1, retain => Probe(retain).Fits);
                }

                return;
            }

            var middle = low + (high - low) / 2;
            if (middle + 1 < high)
            {
                FitBand(middle + 1, Probe(middle + 1), high, right);
            }

            if (middle > low && middle > best)
            {
                FitBand(low, left, middle, Probe(middle));
            }
        }

        Candidate Probe(int retain)
        {
            var text = GetCandidate(retain);
            var layout = CreateLayout(text, inputs, foreground, ProbeLineLimit(inputs.Rows));
            var last = layout.TextLines[^1];
            var fits = layout.TextLines.Count <= inputs.Rows && layout.Width <= inputs.Width &&
                       last.WidthIncludingTrailingWhitespace + actionWidth <= inputs.Width;
            var probe = new Candidate(layout.TextLines.Count, last.FirstTextSourceIndex,
                text.Length - last.FirstTextSourceIndex, fits);
            if (fits && retain > best)
            {
                chosen.Layout?.Dispose();
                chosen = (text, layout);
                best = retain;
            }
            else
            {
                layout.Dispose();
            }

            return probe;
        }

        string GetCandidate(int retain)
        {
            var prefixEnd = retain == count ? inputs.Text.Length : boundaries[retain];
            var suffixStart = retain == 0 ? inputs.Text.Length : boundaries[count - retain];
            return inputs.Direction switch
            {
                EllipsisDirection.Start => string.Concat(marker, inputs.Text.AsSpan(suffixStart)),
                EllipsisDirection.Middle => string.Concat(inputs.Text.AsSpan(0, prefixEnd), marker,
                    inputs.Text.AsSpan(suffixStart)),
                _ => string.Concat(inputs.Text.AsSpan(0, prefixEnd), marker)
            };
        }
    }

    private static int FindLast(int low, int high, Func<int, bool> predicate)
    {
        var best = low - 1;
        while (low <= high)
        {
            var index = low + (high - low) / 2;
            if (predicate(index))
            {
                best = index;
                low = index + 1;
            }
            else
            {
                high = index - 1;
            }
        }

        return best;
    }

    private readonly record struct Candidate(int Rows, int LastBreak, int DistanceFromEnd, bool Fits);

    private static int ProbeLineLimit(int rows) => rows == int.MaxValue ? 0 : rows + 1;

    private static TextLayout CreateLayout(string text, LayoutInputs inputs, IBrush? foreground, int maxLines = 0) =>
        new(text, inputs.Typeface, inputs.FontSize, foreground, textWrapping: TextWrapping.Wrap,
            flowDirection: inputs.FlowDirection, maxWidth: Math.Max(0.000001, inputs.Width), lineHeight: inputs.LineHeight,
            letterSpacing: inputs.LetterSpacing, maxLines: maxLines, fontFeatures: inputs.FontFeatures);

    private void ReplaceLayout(string text, TextLayout layout, IBrush? foreground)
    {
        _textLayout?.Dispose();
        _textLayout = layout;
        _layoutForeground = foreground;
        DisplayedText = text;
        InvalidateVisual();
    }

    private void ClearLayout()
    {
        _textLayout?.Dispose();
        _textLayout = null;
        _inputs = null;
        _layoutForeground = null;
        _textWithBoundaries = null;
        _boundaries = null;
        DisplayedText = null;
    }

    private readonly record struct LayoutInputs(string Text, double Width, int Rows, EllipsisDirection Direction,
        bool IsExpanded, Typeface Typeface, double FontSize, FontFeatureCollection? FontFeatures, double LetterSpacing,
        FlowDirection FlowDirection, double LineHeight);
}
