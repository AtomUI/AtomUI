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

/// <summary>
/// Displays text with an optional action for expanding or collapsing overflow.
/// </summary>
public abstract class AbstractEllipsis : Control
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="Text" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<AbstractEllipsis, string?>(nameof(Text));

    /// <summary>
    /// Identifies the <see cref="Rows" /> property.
    /// </summary>
    public static readonly StyledProperty<int> RowsProperty =
        AvaloniaProperty.Register<AbstractEllipsis, int>(nameof(Rows), 1);

    /// <summary>
    /// Identifies the <see cref="Direction" /> property.
    /// </summary>
    public static readonly StyledProperty<EllipsisDirection> DirectionProperty =
        AvaloniaProperty.Register<AbstractEllipsis, EllipsisDirection>(nameof(Direction), EllipsisDirection.End);

    /// <summary>
    /// Identifies the <see cref="ExpandContent" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> ExpandContentProperty =
        AvaloniaProperty.Register<AbstractEllipsis, object?>(nameof(ExpandContent));

    /// <summary>
    /// Identifies the <see cref="CollapseContent" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> CollapseContentProperty =
        AvaloniaProperty.Register<AbstractEllipsis, object?>(nameof(CollapseContent));

    /// <summary>
    /// Identifies the <see cref="IsExpanded" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsExpandedProperty =
        AvaloniaProperty.Register<AbstractEllipsis, bool>(nameof(IsExpanded), defaultBindingMode: BindingMode.TwoWay);

    /// <summary>
    /// Identifies the <see cref="FontFamily" /> property.
    /// </summary>
    public static readonly StyledProperty<FontFamily> FontFamilyProperty =
        TextElement.FontFamilyProperty.AddOwner<AbstractEllipsis>();

    /// <summary>
    /// Identifies the <see cref="FontSize" /> property.
    /// </summary>
    public static readonly StyledProperty<double> FontSizeProperty =
        TextElement.FontSizeProperty.AddOwner<AbstractEllipsis>();

    /// <summary>
    /// Identifies the <see cref="FontStyle" /> property.
    /// </summary>
    public static readonly StyledProperty<FontStyle> FontStyleProperty =
        TextElement.FontStyleProperty.AddOwner<AbstractEllipsis>();

    /// <summary>
    /// Identifies the <see cref="FontWeight" /> property.
    /// </summary>
    public static readonly StyledProperty<FontWeight> FontWeightProperty =
        TextElement.FontWeightProperty.AddOwner<AbstractEllipsis>();

    /// <summary>
    /// Identifies the <see cref="FontStretch" /> property.
    /// </summary>
    public static readonly StyledProperty<FontStretch> FontStretchProperty =
        TextElement.FontStretchProperty.AddOwner<AbstractEllipsis>();

    /// <summary>
    /// Identifies the <see cref="FontFeatures" /> property.
    /// </summary>
    public static readonly StyledProperty<FontFeatureCollection?> FontFeaturesProperty =
        TextElement.FontFeaturesProperty.AddOwner<AbstractEllipsis>();

    /// <summary>
    /// Identifies the <see cref="LetterSpacing" /> property.
    /// </summary>
    public static readonly StyledProperty<double> LetterSpacingProperty =
        TextElement.LetterSpacingProperty.AddOwner<AbstractEllipsis>();

    /// <summary>
    /// Identifies the <see cref="Foreground" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> ForegroundProperty =
        TextElement.ForegroundProperty.AddOwner<AbstractEllipsis>();

    /// <summary>
    /// The text to lay out, truncate and draw. <see langword="null" /> or an empty string clears the layout,
    /// removes the action and makes the desired size zero.
    /// </summary>
    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>
    /// Maximum number of lines the text may occupy. Text that needs more lines within the available width
    /// overflows and is truncated with an ellipsis while <see cref="IsExpanded" /> is <see langword="false" />.
    /// The default is 1, values below 1 are treated as 1 and <c>int.MaxValue</c> removes the line limit.
    /// </summary>
    public int Rows
    {
        get => GetValue(RowsProperty);
        set => SetValue(RowsProperty, value);
    }

    /// <summary>
    /// Part of the text that the ellipsis replaces while the text is truncated. The default is
    /// <see cref="EllipsisDirection.End" />.
    /// </summary>
    public EllipsisDirection Direction
    {
        get => GetValue(DirectionProperty);
        set => SetValue(DirectionProperty, value);
    }

    /// <summary>
    /// Content of the action shown while the text overflows and <see cref="IsExpanded" /> is
    /// <see langword="false" />. <see langword="null" /> shows no action, and a truncated text is reduced to the
    /// ellipsis alone when the action is at least as wide as the available width.
    /// </summary>
    public object? ExpandContent
    {
        get => GetValue(ExpandContentProperty);
        set => SetValue(ExpandContentProperty, value);
    }

    /// <summary>
    /// Content of the action shown while the text overflows and <see cref="IsExpanded" /> is
    /// <see langword="true" />. <see langword="null" /> shows no action.
    /// </summary>
    public object? CollapseContent
    {
        get => GetValue(CollapseContentProperty);
        set => SetValue(CollapseContentProperty, value);
    }

    /// <summary>
    /// Whether text that overflows is shown in full instead of being truncated. While expanded, the text may use
    /// more lines than <see cref="Rows" /> and the action follows the last line, or takes a line of its own when
    /// it does not fit there. A tap on the action toggles this value without removing a binding and marks the
    /// original <c>Tapped</c> event as handled. It has no effect while the text fits, and bindings are two-way
    /// by default.
    /// </summary>
    public bool IsExpanded
    {
        get => GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    /// <summary>
    /// Font family of the text. Unset, the value is inherited from the parent controls. The action content
    /// inherits the same value.
    /// </summary>
    public FontFamily FontFamily
    {
        get => GetValue(FontFamilyProperty);
        set => SetValue(FontFamilyProperty, value);
    }

    /// <summary>
    /// Font size of the text in DIPs. Unset, the value is inherited from the parent controls. The action content
    /// inherits the same value.
    /// </summary>
    public double FontSize
    {
        get => GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    /// <summary>
    /// Font style of the text. Unset, the value is inherited from the parent controls. The action content
    /// inherits the same value.
    /// </summary>
    public FontStyle FontStyle
    {
        get => GetValue(FontStyleProperty);
        set => SetValue(FontStyleProperty, value);
    }

    /// <summary>
    /// Font weight of the text. Unset, the value is inherited from the parent controls. The action content
    /// inherits the same value.
    /// </summary>
    public FontWeight FontWeight
    {
        get => GetValue(FontWeightProperty);
        set => SetValue(FontWeightProperty, value);
    }

    /// <summary>
    /// Font stretch of the text. Unset, the value is inherited from the parent controls. The action content
    /// inherits the same value.
    /// </summary>
    public FontStretch FontStretch
    {
        get => GetValue(FontStretchProperty);
        set => SetValue(FontStretchProperty, value);
    }

    /// <summary>
    /// OpenType font features applied to the text. Unset, the value is inherited from the parent controls. The
    /// action content inherits the same value.
    /// </summary>
    public FontFeatureCollection? FontFeatures
    {
        get => GetValue(FontFeaturesProperty);
        set => SetValue(FontFeaturesProperty, value);
    }

    /// <summary>
    /// Letter spacing applied between the characters of the text. Unset, the value is inherited from the parent
    /// controls. The action content inherits the same value.
    /// </summary>
    public double LetterSpacing
    {
        get => GetValue(LetterSpacingProperty);
        set => SetValue(LetterSpacingProperty, value);
    }

    /// <summary>
    /// Brush of the text. The action presenter receives the brush returned by
    /// <see cref="ResolveActionForeground" />, which is this value by default. Unset, the value is inherited from
    /// the parent controls. A change repaints the already chosen text without re-running the truncation.
    /// </summary>
    public IBrush? Foreground
    {
        get => GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }

    #endregion

    #region 公共事件定义

    /// <summary>
    /// Identifies the <see cref="ExpandedChanged" /> routed event.
    /// </summary>
    public static readonly RoutedEvent<RoutedEventArgs> ExpandedChangedEvent =
        RoutedEvent.Register<AbstractEllipsis, RoutedEventArgs>(nameof(ExpandedChanged), RoutingStrategies.Bubble);

    /// <summary>
    /// Identifies the <see cref="ContentTapped" /> routed event.
    /// </summary>
    public static readonly RoutedEvent<TappedEventArgs> ContentTappedEvent =
        RoutedEvent.Register<AbstractEllipsis, TappedEventArgs>(nameof(ContentTapped), RoutingStrategies.Bubble);

    /// <summary>
    /// Raised after <see cref="IsExpanded" /> changed, including the change made by a tap on the action. The
    /// text is laid out again after the event. The event cannot be canceled.
    /// </summary>
    public event EventHandler<RoutedEventArgs>? ExpandedChanged
    {
        add => AddHandler(ExpandedChangedEvent, value);
        remove => RemoveHandler(ExpandedChangedEvent, value);
    }

    /// <summary>
    /// Raised when the control itself is tapped. A tap on the action does not raise it, because the action
    /// handles the original <c>Tapped</c> event first. The event carries its own argument instance, so handling
    /// it does not mark the original <c>Tapped</c> event as handled.
    /// </summary>
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

    /// <summary>
    /// Always <see langword="true" />: the control applies <see cref="Visual.FlowDirection" /> itself by passing it
    /// to the text layout, right-aligning the lines for right-to-left text and mirroring the action position, so
    /// Avalonia must not mirror the rendered content as well.
    /// </summary>
    protected override bool BypassFlowDirectionPolicies => true;

    /// <summary>
    /// Resolves the line height in DIPs. NaN uses the font metrics. Called once per measure; the value applies
    /// to every text layout.
    /// </summary>
    protected virtual double ResolveLineHeight() => double.NaN;

    /// <summary>
    /// Resolves the foreground applied to the action presenter. Called when the action presenter is created and
    /// whenever <see cref="Foreground" /> changes.
    /// </summary>
    protected virtual IBrush? ResolveActionForeground() => Foreground;

    /// <summary>
    /// Applies the brush returned by <see cref="ResolveActionForeground" /> to the action presenter without
    /// re-running the truncation. Does nothing while no action is shown.
    /// </summary>
    protected void RefreshActionForeground()
    {
        if (_actionPresenter is { } presenter)
        {
            presenter.SetCurrentValue(ContentPresenter.ForegroundProperty, ResolveActionForeground());
        }
    }

    /// <summary>
    /// Raises <see cref="ExpandedChanged" /> after <see cref="IsExpanded" /> changed and updates the action
    /// foreground after <see cref="Foreground" /> changed. Call the base member to keep both reactions.
    /// </summary>
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

    /// <summary>
    /// Releases the text layout and removes the action presenter on detach; the next measure rebuilds both.
    /// Call the base member to keep this release.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        SetActionContent(null);
        ClearLayout();
        InvalidateMeasure();
        base.OnDetachedFromVisualTree(e);
    }

    /// <summary>
    /// Lays out <see cref="Text" />, decides whether it overflows, creates or removes the action presenter and
    /// truncates the text while it is collapsed. The truncation is computed again only when the layout inputs
    /// or the width of the action changed.
    /// </summary>
    /// <param name="availableSize">
    /// The width is shared by the text and the action and drives wrapping and truncation; an infinite width
    /// never causes an overflow. The height is not used.
    /// </param>
    /// <returns>
    /// The size of the laid out text plus the action, or zero when <see cref="Text" /> is
    /// <see langword="null" /> or empty.
    /// </returns>
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

    /// <summary>
    /// Places the action presenter after the last text line, or on the line below it when the text is expanded
    /// and the action does not fit there; the position is mirrored for right-to-left text. The text itself is
    /// drawn in <see cref="Render(DrawingContext)" />. Returns the assigned size unchanged.
    /// </summary>
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

    /// <summary>
    /// Draws the laid out text, right-aligning the lines for right-to-left text, over a transparent rectangle
    /// that makes the whole control area hit-testable. Rebuilds the layout first when <see cref="Foreground" />
    /// changed after the layout was created.
    /// </summary>
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
