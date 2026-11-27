using System.Globalization;
using AtomUI.Media;
using AtomUI.Theme.Resources;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;
using Avalonia.Metadata;

namespace AtomUI.Controls.Commons;

using AvaloniaSeparator = Avalonia.Controls.Separator;

/// <summary>
/// Base class of a separator: a horizontal or vertical line with an optional title that splits it into two rails.
/// The template holds the title text block <c>PART_Title</c> and the rails <c>PART_RailStart</c> and
/// <c>PART_RailEnd</c>, two <see cref="SeparatorRail" /> that it binds to <see cref="LineColor" />,
/// <see cref="LineWidth" />, <see cref="Variant" /> and <see cref="Orientation" />.
/// </summary>
[PseudoClasses(SeparatorPseudoClass.HasTitleText)]
public abstract class AbstractSeparator : AvaloniaSeparator, ICustomizableSizeTypeAware
{
    private const double SEPARATOR_LINE_MIN_PROPORTION = 0.25;
    
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="Title" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<AbstractSeparator, string?>(nameof(Title));

    /// <summary>
    /// Identifies the <see cref="TitlePosition" /> property.
    /// </summary>
    public static readonly StyledProperty<SeparatorTitlePosition> TitlePositionProperty =
        AvaloniaProperty.Register<AbstractSeparator, SeparatorTitlePosition>(nameof(TitlePosition),
            SeparatorTitlePosition.Center);

    /// <summary>
    /// Identifies the <see cref="TitleColor" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> TitleColorProperty =
        AvaloniaProperty.Register<AbstractSeparator, IBrush?>(nameof(TitleColor));

    /// <summary>
    /// Identifies the <see cref="LineColor" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> LineColorProperty =
        AvaloniaProperty.Register<AbstractSeparator, IBrush?>(nameof(LineColor));

    /// <summary>
    /// Identifies the <see cref="Orientation" /> property.
    /// </summary>
    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<AbstractSeparator, Orientation>(nameof(Orientation));

    /// <summary>
    /// Identifies the <see cref="OrientationMargin" /> property.
    /// </summary>
    public static readonly StyledProperty<double> OrientationMarginProperty =
        AvaloniaProperty.Register<AbstractSeparator, double>(nameof(OrientationMargin), double.NaN);
    
    /// <summary>
    /// Identifies the <see cref="Variant" /> property.
    /// </summary>
    public static readonly StyledProperty<SeparatorVariant> VariantProperty =
        AvaloniaProperty.Register<AbstractSeparator, SeparatorVariant>(nameof(Variant), SeparatorVariant.Solid);

    /// <summary>
    /// Identifies the <see cref="LineWidth" /> property.
    /// </summary>
    public static readonly StyledProperty<double> LineWidthProperty =
        AvaloniaProperty.Register<AbstractSeparator, double>(nameof(LineWidth), 1);
    
    /// <summary>
    /// Identifies the <see cref="IsPlain" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsPlainProperty =
        AvaloniaProperty.Register<AbstractSeparator, bool>(nameof(IsPlain), false);
    
    /// <summary>
    /// Identifies the <see cref="SizeType" /> property.
    /// </summary>
    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<AbstractSeparator>();

    /// <summary>
    /// Text that breaks a horizontal separator line into two rails. Set it as the XAML content of the control.
    /// A vertical separator does not lay the title out. <see langword="null" /> or an empty string draws one
    /// continuous line and clears the <c>:has-title</c> pseudo-class.
    /// </summary>
    [Content]
    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>
    /// End of the line the title is placed at, or its center. Applies to a horizontal separator with a title only.
    /// <see cref="SeparatorTitlePosition.Center" /> by default.
    /// </summary>
    public SeparatorTitlePosition TitlePosition
    {
        get => GetValue(TitlePositionProperty);
        set => SetValue(TitlePositionProperty, value);
    }

    /// <summary>
    /// Brush that paints the title text. The default theme supplies its <c>ColorTextHeading</c> token; an explicit
    /// <see langword="null" /> leaves the title without a brush.
    /// </summary>
    public IBrush? TitleColor
    {
        get => GetValue(TitleColorProperty);
        set => SetValue(TitleColorProperty, value);
    }

    /// <summary>
    /// Brush that paints both rails of the line. The default theme supplies its <c>ColorSplit</c> token; an explicit
    /// <see langword="null" /> leaves the line unpainted.
    /// </summary>
    public IBrush? LineColor
    {
        get => GetValue(LineColorProperty);
        set => SetValue(LineColorProperty, value);
    }

    /// <summary>
    /// Direction of the line. <see cref="Avalonia.Layout.Orientation.Horizontal" />, the default, draws it across
    /// the control; <see cref="Avalonia.Layout.Orientation.Vertical" /> draws it from top to bottom, inset by a
    /// fifth of the height at each end, and ignores the title.
    /// </summary>
    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }
    
    /// <summary>
    /// Line style of both rails, <see cref="SeparatorVariant.Solid" /> by default. The theme binds the rails to it.
    /// </summary>
    public SeparatorVariant Variant
    {
        get => GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    /// <summary>
    /// Distance in DIPs between the title and the end it is placed at, with
    /// <see cref="SeparatorTitlePosition.Left" /> or <see cref="SeparatorTitlePosition.Right" />; the text padding of
    /// the theme is added between that distance and the title. It is capped at half of the width left around
    /// the title, and the rail between the title and that end is not drawn.
    /// <see cref="double.NaN" />, the default, places the title at the theme's default distance, a ratio of the
    /// width, and draws both rails.
    /// </summary>
    public double OrientationMargin
    {
        get => GetValue(OrientationMarginProperty);
        set => SetValue(OrientationMarginProperty, value);
    }
    
    /// <summary>
    /// Design thickness of the line in DIPs. The rails scale it by the render scaling before they draw, and it sets
    /// the measured thickness of a separator without a title. The default theme supplies the value from its
    /// <c>LineWidth</c> token.
    /// </summary>
    public double LineWidth
    {
        get => GetValue(LineWidthProperty);
        set => SetValue(LineWidthProperty, value);
    }
    
    /// <summary>
    /// <see langword="true" /> renders the title as plain body text. <see langword="false" />, the default, renders
    /// it as a heading with the large font size and the title font weight. The theme styles the title from this value.
    /// </summary>
    public bool IsPlain
    {
        get => GetValue(IsPlainProperty);
        set => SetValue(IsPlainProperty, value);
    }
    
    /// <summary>
    /// Size preset of a horizontal separator: the theme selects the margin above and below the line by it.
    /// <see cref="CustomizableSizeType.Small" />, <see cref="CustomizableSizeType.Middle" /> (the default) and
    /// <see cref="CustomizableSizeType.Large" /> each map to a block margin of the theme.
    /// <see cref="CustomizableSizeType.Custom" /> has no size style, so the block margin that the theme sets for a
    /// horizontal separator stays. A vertical separator ignores the value.
    /// </summary>
    public CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<double> TextPaddingInlineProperty =
        AvaloniaProperty.Register<AbstractSeparator, double>(
            nameof(TextPaddingInline));

    internal static readonly StyledProperty<double> OrientationMarginPercentProperty =
        AvaloniaProperty.Register<AbstractSeparator, double>(
            nameof(OrientationMarginPercent));

    internal static readonly StyledProperty<double> VerticalMarginInlineProperty =
        AvaloniaProperty.Register<AbstractSeparator, double>(
            nameof(VerticalMarginInline));

    internal static readonly StyledProperty<bool> UseSeparatorLayoutTokenResourcesProperty =
        AvaloniaProperty.Register<AbstractSeparator, bool>(nameof(UseSeparatorLayoutTokenResources));

    internal double TextPaddingInline
    {
        get => GetValue(TextPaddingInlineProperty);
        set => SetValue(TextPaddingInlineProperty, value);
    }

    internal double OrientationMarginPercent
    {
        get => GetValue(OrientationMarginPercentProperty);
        set => SetValue(OrientationMarginPercentProperty, value);
    }

    internal double VerticalMarginInline
    {
        get => GetValue(VerticalMarginInlineProperty);
        set => SetValue(VerticalMarginInlineProperty, value);
    }

    /// <summary>
    /// While <see langword="true" />, the separator observes the layout tokens of the Separator design and takes the
    /// text padding and the edge ratio of a titled separator and the vertical margin of a vertical separator from
    /// them; while <see langword="false" />, no tokens are observed. The theme sets it to <see langword="true" />.
    /// </summary>
    internal bool UseSeparatorLayoutTokenResources
    {
        get => GetValue(UseSeparatorLayoutTokenResourcesProperty);
        set => SetValue(UseSeparatorLayoutTokenResourcesProperty, value);
    }

    #endregion
    
    private TextBlock? _titleLabel;
    private SeparatorRail? _railStart;
    private SeparatorRail? _railEnd;
    private double _currentEdgeDistance;
    private OwnerTokenSet? _layoutTokens;
    private (double TextPadding, double EdgeRatio, double VerticalMargin) _layoutTokensValues;

    /// <summary>
    /// Creates the token set that the layout reads when the theme requests it: the padding around the title,
    /// the ratio that places the title while <see cref="OrientationMargin" /> is <see cref="double.NaN" />, and
    /// the margin that widens a vertical separator. Returns <see langword="null" /> by default, which reads no
    /// tokens. <paramref name="changed" /> is raised when a token value changes.
    /// It is called when the control joins a visual tree and when the request changes while the control is attached.
    /// The control owns the set it returns and disposes it when it is replaced and when the control is detached.
    /// </summary>
    private protected virtual OwnerTokenSet? CreateSeparatorLayoutTokenSet(Action changed) => null;

    private double EffectiveTextPadding =>
        IsSet(TextPaddingInlineProperty) ? TextPaddingInline : _layoutTokensValues.TextPadding;

    private double EffectiveEdgeRatio =>
        IsSet(OrientationMarginPercentProperty) ? OrientationMarginPercent : _layoutTokensValues.EdgeRatio;

    private double EffectiveVerticalMargin =>
        IsSet(VerticalMarginInlineProperty) ? VerticalMarginInline : _layoutTokensValues.VerticalMargin;

    static AbstractSeparator()
    {
        // Shared themes may set the layout properties directly.
        AffectsMeasure<AbstractSeparator>(OrientationProperty,
            LineWidthProperty,
            TitleProperty,
            VerticalMarginInlineProperty);
        AffectsArrange<AbstractSeparator>(TitlePositionProperty,
            OrientationMarginProperty,
            TextPaddingInlineProperty,
            OrientationMarginPercentProperty);
    }

    /// <summary>
    /// Updates the <c>:has-title</c> pseudo-class when <see cref="Title" /> changes, rebuilds the layout token
    /// subscription when the theme switches it on or off while the control is attached, and refreshes the layout
    /// metrics when <see cref="Orientation" /> or <see cref="Title" /> changes while a subscription is active.
    /// A derived override must call this base member to keep them up to date.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == TitleProperty)
        {
            UpdatePseudoClasses();
        }
        if (change.Property == UseSeparatorLayoutTokenResourcesProperty && this.IsAttachedToVisualTree())
        {
            UpdateLayoutTokenSubscription();
        }
        else if (_layoutTokens is not null && (change.Property == OrientationProperty || change.Property == TitleProperty))
        {
            RefreshLayoutMetrics();
        }
    }

    /// <summary>
    /// Builds the subscription to the layout tokens of the theme when the control joins a visual tree. The tokens
    /// provide the padding around the title, the default position of the title and the margin that widens a vertical
    /// separator; a theme that does not request them subscribes to nothing. A derived override must call this base
    /// member for the metrics to be read.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        UpdateLayoutTokenSubscription();
    }

    /// <summary>
    /// Releases the layout token subscription and drops the metrics read from it when the control leaves the visual
    /// tree. A derived override must call this base member to release the subscription.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _layoutTokens?.Dispose();
        _layoutTokens = null;
        _layoutTokensValues = default;
        base.OnDetachedFromVisualTree(e);
    }

    private void UpdateLayoutTokenSubscription()
    {
        _layoutTokens?.Dispose();
        _layoutTokens = UseSeparatorLayoutTokenResources ? CreateSeparatorLayoutTokenSet(RefreshLayoutMetrics) : null;
        RefreshLayoutMetrics();
    }

    private void RefreshLayoutMetrics()
    {
        var previous = (TextPadding: EffectiveTextPadding, EdgeRatio: EffectiveEdgeRatio, VerticalMargin: EffectiveVerticalMargin);
        var vertical = Orientation == Orientation.Vertical;
        var titled = !vertical && !string.IsNullOrEmpty(Title);
        _layoutTokensValues = (
            titled ? ReadLayoutToken(0, _layoutTokensValues.TextPadding) : 0,
            titled ? ReadLayoutToken(1, _layoutTokensValues.EdgeRatio) : 0,
            vertical ? ReadLayoutToken(2, _layoutTokensValues.VerticalMargin) : 0);

        if (vertical && !previous.VerticalMargin.Equals(EffectiveVerticalMargin))
        {
            InvalidateMeasure();
        }
        else if (titled && (!previous.TextPadding.Equals(EffectiveTextPadding) || !previous.EdgeRatio.Equals(EffectiveEdgeRatio)))
        {
            InvalidateArrange();
        }
    }

    private double ReadLayoutToken(int index, double previous)
    {
        var value = _layoutTokens?.GetValue(index);
        if (value is double metric)
        {
            return metric;
        }
        if (ReferenceEquals(value, BindingOperations.DoNothing))
        {
            return previous;
        }
        try
        {
            return value is IConvertible convertible ? convertible.ToDouble(CultureInfo.CurrentCulture) : 0;
        }
        catch (Exception error) when (error is FormatException or InvalidCastException or OverflowException)
        {
            return 0;
        }
    }

    /// <summary>
    /// Resolves the template parts <c>PART_Title</c>, <c>PART_RailStart</c> and <c>PART_RailEnd</c> and updates the
    /// <c>:has-title</c> pseudo-class. A missing part is left out: without the rails no line is arranged, without
    /// the title block no title is arranged. A derived override must call this base member.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _titleLabel = e.NameScope.Find<TextBlock>("PART_Title");
        _railStart  = e.NameScope.Find<SeparatorRail>("PART_RailStart");
        _railEnd    = e.NameScope.Find<SeparatorRail>("PART_RailEnd");
        UpdatePseudoClasses();
    }
    
    /// <summary>
    /// Stretches the separator along its line and fixes its thickness. A horizontal one is three times
    /// <see cref="LineWidth" /> high without a title and as high as the title with one; a vertical one is
    /// <see cref="LineWidth" />, or 1 DIP when that is smaller, plus the vertical margin wide and at least one em
    /// high. A derived override must call this base member to keep that sizing.
    /// </summary>
    protected override Size MeasureOverride(Size availableSize)
    {
        var size         = base.MeasureOverride(availableSize);
        var targetHeight = size.Height;
        var targetWidth  = size.Width;
        if (Orientation == Orientation.Horizontal)
        {
            if (Title is null || Title?.Length == 0)
            {
                targetHeight = LineWidth * 3;
            }

            if (!double.IsInfinity(availableSize.Width))
            {
                targetWidth = Math.Max(availableSize.Width, targetWidth);
            }
        }
        else
        {
            targetWidth  = Math.Max(1, LineWidth) + EffectiveVerticalMargin;
            targetHeight = FontUtils.ConvertEmToPixel(1, FontSize, TopLevel.GetTopLevel(this)?.RenderScaling ?? 1.0);
            if (!double.IsInfinity(availableSize.Height))
            {
                targetHeight = Math.Max(availableSize.Height, targetHeight);
            }
        }

        return new Size(targetWidth, targetHeight);
    }

    /// <summary>
    /// Arranges the rails and, on a horizontal separator with a visible title, the title, on top of the base
    /// arrangement. The rails leave out the title and its padding; a vertical separator insets its rail by a
    /// fifth of the height at each end. A derived override must call this base member to place them.
    /// </summary>
    protected override Size ArrangeOverride(Size finalSize)
    {
        var size = base.ArrangeOverride(finalSize);
        ArrangeRails(finalSize);
        if (Orientation == Orientation.Horizontal && _titleLabel?.IsVisible == true)
        {
            _titleLabel.Arrange(GetTitleRect(finalSize));
        }

        return size;
    }

    private void ArrangeRails(Size finalSize)
    {
        if (_railStart is null || _railEnd is null)
        {
            return;
        }

        if (Orientation == Orientation.Horizontal)
        {
            if (Title?.Length > 0)
            {
                var titleRect  = GetTitleRect(finalSize);
                var textPadding = GetTextPaddingInline();
                var drawStart  = TitlePosition != SeparatorTitlePosition.Left ||
                                 double.IsNaN(OrientationMargin);
                var drawEnd    = TitlePosition != SeparatorTitlePosition.Right ||
                                 double.IsNaN(OrientationMargin);

                var startWidth = drawStart
                    ? Math.Max(0, titleRect.Left - textPadding)
                    : 0;
                var endLeft   = titleRect.Right + textPadding;
                var endWidth  = drawEnd
                    ? Math.Max(0, finalSize.Width - endLeft)
                    : 0;

                _railStart.Arrange(new Rect(0, 0, startWidth, finalSize.Height));
                _railEnd.Arrange(new Rect(endLeft, 0, endWidth, finalSize.Height));
            }
            else
            {
                _railStart.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));
                _railEnd.Arrange(new Rect(0, 0, 0, 0));
            }
        }
        else
        {
            var inset      = finalSize.Height * 0.2;
            var railHeight = Math.Max(0, finalSize.Height - inset * 2);
            _railStart.Arrange(new Rect(0, inset, finalSize.Width, railHeight));
            _railEnd.Arrange(new Rect(0, 0, 0, 0));
        }
    }

    private double GetTextPaddingInline()
    {
        return FontSize * EffectiveTextPadding;
    }

    private Rect GetTitleRect(Size finalSize)
    {
        Rect titleRect = default;
        if (Orientation == Orientation.Horizontal && Title?.Length > 0)
        {
            // Minimum share of the width that the rails keep beside a long title. The value is a rough estimate.
            var lineMinWidth = finalSize.Width * SEPARATOR_LINE_MIN_PROPORTION;
            var titleWidth   = (_titleLabel?.DesiredSize.Width ?? 0) + 2;
            var remainWidth  = finalSize.Width - titleWidth - GetTextPaddingInline() * 2;
            if (lineMinWidth > remainWidth)
            {
                // The title leaves less than that share to the rails, so its width is capped.
                titleWidth = Math.Max(finalSize.Width - lineMinWidth, lineMinWidth);
            }

            // The cap above keeps the title within three quarters of the width.
            if (TitlePosition == SeparatorTitlePosition.Left)
            {
                if (!double.IsNaN(OrientationMargin))
                {
                    _currentEdgeDistance = Math.Min((finalSize.Width - titleWidth) / 2, OrientationMargin);
                }
                else
                {
                    _currentEdgeDistance = finalSize.Width * EffectiveEdgeRatio;
                }

                titleRect = new Rect(new Point(_currentEdgeDistance + GetTextPaddingInline(), 0),
                    new Size(titleWidth, finalSize.Height));
                var rightDelta = titleRect.Right - finalSize.Width;
                if (MathUtils.GreaterThan(rightDelta, 0))
                {
                    titleRect = titleRect.WithWidth(Math.Max(finalSize.Width - titleRect.Left, lineMinWidth));
                }
            }
            else if (TitlePosition == SeparatorTitlePosition.Right)
            {
                if (!double.IsNaN(OrientationMargin))
                {
                    _currentEdgeDistance = Math.Min((finalSize.Width - titleWidth) / 2, OrientationMargin);
                }
                else
                {
                    _currentEdgeDistance = finalSize.Width * EffectiveEdgeRatio;
                }

                titleRect = new Rect(
                    new Point(finalSize.Width - _currentEdgeDistance - titleWidth - GetTextPaddingInline() * 2, 0),
                    new Size(titleWidth, finalSize.Height));
                var leftDelta = titleRect.Left - 0;
                if (leftDelta < 0)
                {
                    titleRect = titleRect.WithX(0);
                }
            }
            else
            {
                // Centered: the remaining width is split evenly between the two rails.
                titleRect = new Rect(new Point((finalSize.Width - titleWidth) / 2, 0),
                    new Size(titleWidth, finalSize.Height));
            }
        }

        return titleRect;
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(SeparatorPseudoClass.HasTitleText, !string.IsNullOrEmpty(Title));
    }
}
