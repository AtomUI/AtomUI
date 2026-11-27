using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Metadata;
using Avalonia.VisualTree;

namespace AtomUI.Controls.Commons;

/// <summary>
/// A skeleton made of several blocks (avatar, title, paragraph) that stands in for its <see cref="Content" /> while
/// <see cref="IsLoading" /> is true. The placeholder blocks are built from <see cref="PlaceholderTemplate" /> only while
/// loading and dropped afterwards; the composite is the shimmer host of everything inside the placeholder, while its
/// content keeps its own shimmer.
/// </summary>
public abstract class AbstractCompositeSkeleton : AbstractSkeleton
{
    /// <summary>
    /// Panel of the control template the placeholder is inserted into, at index 0.
    /// </summary>
    internal const string PlaceholderHostPart = "PART_PlaceholderHost";

    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="IsLoading" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsLoadingProperty =
        AvaloniaProperty.Register<AbstractCompositeSkeleton, bool>(nameof(IsLoading));

    /// <summary>
    /// Identifies the <see cref="IsShowAvatar" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsShowAvatarProperty =
        AvaloniaProperty.Register<AbstractCompositeSkeleton, bool>(nameof(IsShowAvatar));

    /// <summary>
    /// Identifies the <see cref="IsShowParagraph" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsShowParagraphProperty =
        AvaloniaProperty.Register<AbstractCompositeSkeleton, bool>(nameof(IsShowParagraph), true);

    /// <summary>
    /// Identifies the <see cref="IsShowTitle" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsShowTitleProperty =
        AvaloniaProperty.Register<AbstractCompositeSkeleton, bool>(nameof(IsShowTitle), true);

    /// <summary>
    /// Identifies the <see cref="IsRound" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsRoundProperty =
        AbstractSkeletonLine.IsRoundProperty.AddOwner<AbstractCompositeSkeleton>();

    /// <summary>
    /// Identifies the <see cref="TitleWidth" /> property.
    /// </summary>
    public static readonly StyledProperty<Dimension> TitleWidthProperty =
        AvaloniaProperty.Register<AbstractCompositeSkeleton, Dimension>(nameof(TitleWidth), new Dimension(50, DimensionUnitType.Percentage));

    /// <summary>
    /// Identifies the <see cref="ParagraphRows" /> property.
    /// </summary>
    public static readonly StyledProperty<int> ParagraphRowsProperty =
        AvaloniaProperty.Register<AbstractCompositeSkeleton, int>(nameof(ParagraphRows), 2, validate: i => i >= 1);

    /// <summary>
    /// Identifies the <see cref="ParagraphLastLineWidth" /> property.
    /// </summary>
    public static readonly StyledProperty<Dimension> ParagraphLastLineWidthProperty =
        AvaloniaProperty.Register<AbstractCompositeSkeleton, Dimension>(nameof(ParagraphLastLineWidth), new Dimension(61.0, DimensionUnitType.Percentage));

    /// <summary>
    /// Identifies the <see cref="ParagraphLineWidths" /> property.
    /// </summary>
    public static readonly StyledProperty<List<Dimension>?> ParagraphLineWidthsProperty =
        AvaloniaProperty.Register<AbstractCompositeSkeleton, List<Dimension>?>(nameof(ParagraphLineWidths));

    /// <summary>
    /// Identifies the <see cref="AvatarShape" /> property.
    /// </summary>
    public static readonly StyledProperty<AvatarShape> AvatarShapeProperty =
        AvaloniaProperty.Register<AbstractCompositeSkeleton, AvatarShape>(nameof(AvatarShape), AvatarShape.Circle);

    /// <summary>
    /// Identifies the <see cref="AvatarSize" /> property.
    /// </summary>
    public static readonly StyledProperty<double> AvatarSizeProperty =
        AvaloniaProperty.Register<AbstractCompositeSkeleton, double>(nameof(AvatarSize), Double.NaN);

    /// <summary>
    /// Identifies the <see cref="Content" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> ContentProperty =
        ContentControl.ContentProperty.AddOwner<AbstractCompositeSkeleton>();

    /// <summary>
    /// Identifies the <see cref="ContentTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> ContentTemplateProperty =
        ContentControl.ContentTemplateProperty.AddOwner<AbstractCompositeSkeleton>();

    /// <summary>
    /// Identifies the <see cref="HorizontalContentAlignment" /> property.
    /// </summary>
    public static readonly StyledProperty<HorizontalAlignment> HorizontalContentAlignmentProperty =
        ContentControl.HorizontalContentAlignmentProperty.AddOwner<AbstractCompositeSkeleton>();

    /// <summary>
    /// Identifies the <see cref="VerticalContentAlignment" /> property.
    /// </summary>
    public static readonly StyledProperty<VerticalAlignment> VerticalContentAlignmentProperty =
        ContentControl.VerticalContentAlignmentProperty.AddOwner<AbstractCompositeSkeleton>();

    /// <summary>
    /// Shows the placeholder built from the theme in place of <see cref="Content" /> while true, and the content
    /// itself while false. The placeholder exists only once the control is attached and its template is applied,
    /// and it shimmers only while <see cref="AbstractSkeleton.IsActive" /> is also true.
    /// </summary>
    public bool IsLoading
    {
        get => GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    /// <summary>
    /// Shows the avatar placeholder beside the title and paragraph. The default is false.
    /// </summary>
    public bool IsShowAvatar
    {
        get => GetValue(IsShowAvatarProperty);
        set => SetValue(IsShowAvatarProperty, value);
    }

    /// <summary>
    /// Shows the paragraph placeholder below the title. The default is true.
    /// </summary>
    public bool IsShowParagraph
    {
        get => GetValue(IsShowParagraphProperty);
        set => SetValue(IsShowParagraphProperty, value);
    }

    /// <summary>
    /// Shows the title placeholder above the paragraph. The default is true.
    /// </summary>
    public bool IsShowTitle
    {
        get => GetValue(IsShowTitleProperty);
        set => SetValue(IsShowTitleProperty, value);
    }

    /// <summary>
    /// Corner style of the title and paragraph lines of the placeholder: true gives them rounded ends, false gives
    /// them the block corner radius.
    /// </summary>
    public bool IsRound
    {
        get => GetValue(IsRoundProperty);
        set => SetValue(IsRoundProperty, value);
    }

    /// <summary>
    /// Width of the title line: a percentage is taken of the width offered to the title, a pixel value is an exact
    /// width in DIPs, and the title never grows beyond the offered width. The default is 50%. The <c>Skeleton</c>
    /// theme sets 38% while the avatar is hidden and the paragraph is shown; a value set on the control wins.
    /// </summary>
    public Dimension TitleWidth
    {
        get => GetValue(TitleWidthProperty);
        set => SetValue(TitleWidthProperty, value);
    }

    /// <summary>
    /// Number of lines of the paragraph placeholder; values below 1 are rejected. The default is 2. The
    /// <c>Skeleton</c> theme raises it to 3 while the avatar is hidden and the title is shown.
    /// </summary>
    public int ParagraphRows
    {
        get => GetValue(ParagraphRowsProperty);
        set => SetValue(ParagraphRowsProperty, value);
    }

    /// <summary>
    /// Width of the last line of the paragraph placeholder, as a percentage of the width offered to the line or as
    /// an exact width in DIPs; the line never grows beyond the offered width. It is used only while
    /// <see cref="ParagraphLineWidths" /> has no entry for the last row.
    /// </summary>
    public Dimension ParagraphLastLineWidth
    {
        get => GetValue(ParagraphLastLineWidthProperty);
        set => SetValue(ParagraphLastLineWidthProperty, value);
    }

    /// <summary>
    /// Widths of the single lines of the paragraph placeholder in top-down order; the line of row <c>i</c> takes
    /// entry <c>i</c> when the list has one. Any other line falls back to <see cref="ParagraphLastLineWidth" />
    /// when it is the last row and to the full width otherwise; <see langword="null" /> does so for every line.
    /// Each entry is a percentage of the width offered to the line or an exact width in DIPs.
    /// </summary>
    public List<Dimension>? ParagraphLineWidths
    {
        get => GetValue(ParagraphLineWidthsProperty);
        set => SetValue(ParagraphLineWidthsProperty, value);
    }

    /// <summary>
    /// Shape of the avatar placeholder: <c>Circle</c> is fully round, <c>Square</c> has the corner radius of its
    /// size. The default is <c>Circle</c>. The <c>Skeleton</c> theme sets <c>Square</c> while the title is shown
    /// without the paragraph; a value set on the control wins.
    /// </summary>
    public AvatarShape AvatarShape
    {
        get => GetValue(AvatarShapeProperty);
        set => SetValue(AvatarShapeProperty, value);
    }

    /// <summary>
    /// Width and height of the avatar placeholder in DIPs. <c>NaN</c>, the default, leaves the size to the avatar
    /// size type of the concrete control.
    /// </summary>
    public double AvatarSize
    {
        get => GetValue(AvatarSizeProperty);
        set => SetValue(AvatarSizeProperty, value);
    }

    /// <summary>
    /// The real content of the control: shown while <see cref="IsLoading" /> is false and hidden while the
    /// placeholder is shown. A control used as content becomes a logical child of this control.
    /// </summary>
    [Content]
    [DependsOn(nameof(ContentTemplate))]
    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    /// <summary>
    /// Template that builds <see cref="Content" /> when the content is not a control. While <see cref="IsLoading" />
    /// is true the content is hidden, so the template has no visible effect.
    /// </summary>
    public IDataTemplate? ContentTemplate
    {
        get => GetValue(ContentTemplateProperty);
        set => SetValue(ContentTemplateProperty, value);
    }

    /// <summary>
    /// Horizontal alignment of <see cref="Content" /> inside the control. It has no effect on the placeholder,
    /// which always fills the control.
    /// </summary>
    public HorizontalAlignment HorizontalContentAlignment
    {
        get => GetValue(HorizontalContentAlignmentProperty);
        set => SetValue(HorizontalContentAlignmentProperty, value);
    }

    /// <summary>
    /// Vertical alignment of <see cref="Content" /> inside the control. It has no effect on the placeholder,
    /// which always fills the control.
    /// </summary>
    public VerticalAlignment VerticalContentAlignment
    {
        get => GetValue(VerticalContentAlignmentProperty);
        set => SetValue(VerticalContentAlignmentProperty, value);
    }

    #endregion

    #region 内部属性定义

    internal static readonly DirectProperty<AbstractCompositeSkeleton, bool> IsContentVisibleProperty =
        AvaloniaProperty.RegisterDirect<AbstractCompositeSkeleton, bool>(
            nameof(IsContentVisible),
            o => o.IsContentVisible,
            (o, v) => o.IsContentVisible = v);

    private bool _isContentVisible;

    /// <summary>
    /// Whether the content of the skeleton is shown: the opposite of <see cref="IsLoading" />, kept in step with it
    /// by the skeleton. The theme binds the <c>IsVisible</c> of the content presenter to it.
    /// </summary>
    internal bool IsContentVisible
    {
        get => _isContentVisible;
        set => SetAndRaise(IsContentVisibleProperty, ref _isContentVisible, value);
    }

    /// <summary>
    /// Identifies the <see cref="PlaceholderTemplate" /> property.
    /// </summary>
    internal static readonly StyledProperty<IControlTemplate?> PlaceholderTemplateProperty =
        AvaloniaProperty.Register<AbstractCompositeSkeleton, IControlTemplate?>(nameof(PlaceholderTemplate));

    /// <summary>
    /// Template of the placeholder that is shown in place of the content while <see cref="IsLoading" /> is
    /// <see langword="true" />; built on demand with this skeleton as templated parent, so template bindings,
    /// <c>/template/</c> styles and template part names work inside it. <see langword="null" /> shows no placeholder.
    /// The theme of the composite skeleton supplies the template.
    /// </summary>
    internal IControlTemplate? PlaceholderTemplate
    {
        get => GetValue(PlaceholderTemplateProperty);
        set => SetValue(PlaceholderTemplateProperty, value);
    }

    #endregion

    private Panel? _placeholderHost;
    private Control? _placeholder;
    private INameScope? _placeholderNameScope;

    /// <summary>
    /// The placeholder built from <see cref="PlaceholderTemplate" />; null while not loading.
    /// </summary>
    internal Control? Placeholder => _placeholder;

    internal override bool CanRunShimmer => IsLoading;

    internal override bool IsShimmerReady => base.IsShimmerReady && (_placeholder is not null || !ShouldHavePlaceholder());

    static AbstractCompositeSkeleton()
    {
        ContentProperty.Changed.AddClassHandler<AbstractCompositeSkeleton>((x, e) => x.HandleContentChanged(e));
    }

    protected AbstractCompositeSkeleton()
    {
        UpdateContentVisibility();
    }

    /// <summary>
    /// Hides or shows the content when <see cref="IsLoading" />, <see cref="Content" /> or
    /// <see cref="ContentTemplate" /> changes, builds or drops the placeholder when <see cref="IsLoading" />
    /// changes, and rebuilds it when the placeholder template changes. Overrides must call the base member.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsLoadingProperty ||
            change.Property == ContentProperty ||
            change.Property == ContentTemplateProperty)
        {
            UpdateContentVisibility();
        }

        if (change.Property == IsLoadingProperty)
        {
            UpdatePlaceholder(false);
        }
        else if (change.Property == PlaceholderTemplateProperty)
        {
            UpdatePlaceholder(true);
        }
    }

    /// <summary>
    /// Drops the placeholder of the previous template, locates the placeholder host of the new one and builds the
    /// placeholder again. Call the base member last: it resolves the shimmer layer, which must already see the new
    /// placeholder.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        StopShimmer();
        RemovePlaceholder();
        _placeholderHost = e.NameScope.Find<Panel>(PlaceholderHostPart);
        BuildPlaceholder();
        // The base resolves the shimmer layer, which already sees the new placeholder.
        base.OnApplyTemplate(e);
    }

    /// <summary>
    /// Builds the placeholder once the control is attached, if it is loading. Overrides must call the base member,
    /// which decides whether the control is a shimmer host.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        UpdatePlaceholder(false);
    }

    /// <summary>
    /// Removes the placeholder while the control is detached, so that it is rebuilt when the control attaches
    /// again. Overrides must call the base member, which stops the shimmer.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        if (_placeholder is not null)
        {
            RemovePlaceholder();
            RefreshShimmerLayer();
        }
    }

    internal override Control ResolveShimmerLayer()
    {
        return _placeholderNameScope?.Find<Control>(ShimmerLayerPart) ?? base.ResolveShimmerLayer();
    }

    // Builds or drops the placeholder to match IsLoading; rebuild replaces an existing one (template change).
    private void UpdatePlaceholder(bool rebuild)
    {
        var shouldExist = ShouldHavePlaceholder();
        if (shouldExist == (_placeholder is not null) && !(rebuild && shouldExist))
        {
            return;
        }
        // Stop first so a running shimmer never sees half of the blocks go.
        StopShimmer();
        RemovePlaceholder();
        BuildPlaceholder();
        RefreshShimmerLayer();
    }

    private bool ShouldHavePlaceholder()
    {
        return IsLoading && _placeholderHost is not null && PlaceholderTemplate is not null &&
               this.IsAttachedToVisualTree();
    }

    private void BuildPlaceholder()
    {
        if (_placeholder is not null || !ShouldHavePlaceholder() ||
            PlaceholderTemplate?.Build(this) is not { } result)
        {
            return;
        }
        var placeholder = result.Result;
        TemplatedControlUtils.ApplyTemplatedParent(placeholder, this);
        _placeholder          = placeholder;
        _placeholderNameScope = result.NameScope;
        // Set before insertion: the blocks find their host through Placeholder as they attach.
        _placeholderHost!.Children.Insert(0, placeholder);
    }

    private void RemovePlaceholder()
    {
        if (_placeholder is not { } placeholder)
        {
            return;
        }
        _placeholder          = null;
        _placeholderNameScope = null;
        if (placeholder.GetVisualParent() is Panel host)
        {
            host.Children.Remove(placeholder);
        }
    }

    private void UpdateContentVisibility()
    {
        IsContentVisible = !IsLoading;
    }

    private void HandleContentChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.OldValue is ILogical oldChild)
        {
            LogicalChildren.Remove(oldChild);
        }

        if (e.NewValue is ILogical newChild)
        {
            LogicalChildren.Add(newChild);
        }
    }
}
