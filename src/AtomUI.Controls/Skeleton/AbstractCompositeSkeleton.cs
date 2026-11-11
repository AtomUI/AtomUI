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
/// A skeleton made of several blocks (avatar, title, paragraph) that stands in for its <see cref="Content"/> while
/// <see cref="IsLoading"/> is true. The placeholder blocks are built from <see cref="PlaceholderTemplate"/> only while
/// loading and dropped afterwards; the composite is the shimmer host of everything inside the placeholder, while its
/// content keeps its own shimmer.
/// </summary>
public abstract class AbstractCompositeSkeleton : AbstractSkeleton
{
    /// <summary>Panel of the control template the placeholder is inserted into, at index 0.</summary>
    internal const string PlaceholderHostPart = "PART_PlaceholderHost";

    #region 公共属性定义

    public static readonly StyledProperty<bool> IsLoadingProperty =
        AvaloniaProperty.Register<AbstractCompositeSkeleton, bool>(nameof(IsLoading));

    public static readonly StyledProperty<bool> IsShowAvatarProperty =
        AvaloniaProperty.Register<AbstractCompositeSkeleton, bool>(nameof(IsShowAvatar));

    public static readonly StyledProperty<bool> IsShowParagraphProperty =
        AvaloniaProperty.Register<AbstractCompositeSkeleton, bool>(nameof(IsShowParagraph), true);

    public static readonly StyledProperty<bool> IsShowTitleProperty =
        AvaloniaProperty.Register<AbstractCompositeSkeleton, bool>(nameof(IsShowTitle), true);

    public static readonly StyledProperty<bool> IsRoundProperty =
        AbstractSkeletonLine.IsRoundProperty.AddOwner<AbstractCompositeSkeleton>();

    public static readonly StyledProperty<Dimension> TitleWidthProperty =
        AvaloniaProperty.Register<AbstractCompositeSkeleton, Dimension>(nameof(TitleWidth), new Dimension(50, DimensionUnitType.Percentage));

    public static readonly StyledProperty<int> ParagraphRowsProperty =
        AvaloniaProperty.Register<AbstractCompositeSkeleton, int>(nameof(ParagraphRows), 2, validate: i => i >= 1);

    public static readonly StyledProperty<Dimension> ParagraphLastLineWidthProperty =
        AvaloniaProperty.Register<AbstractCompositeSkeleton, Dimension>(nameof(ParagraphLastLineWidth), new Dimension(61.0, DimensionUnitType.Percentage));

    public static readonly StyledProperty<List<Dimension>?> ParagraphLineWidthsProperty =
        AvaloniaProperty.Register<AbstractCompositeSkeleton, List<Dimension>?>(nameof(ParagraphLineWidths));

    public static readonly StyledProperty<AvatarShape> AvatarShapeProperty =
        AvaloniaProperty.Register<AbstractCompositeSkeleton, AvatarShape>(nameof(AvatarShape), AvatarShape.Circle);

    public static readonly StyledProperty<double> AvatarSizeProperty =
        AvaloniaProperty.Register<AbstractCompositeSkeleton, double>(nameof(AvatarSize), Double.NaN);

    public static readonly StyledProperty<object?> ContentProperty =
        ContentControl.ContentProperty.AddOwner<AbstractCompositeSkeleton>();

    public static readonly StyledProperty<IDataTemplate?> ContentTemplateProperty =
        ContentControl.ContentTemplateProperty.AddOwner<AbstractCompositeSkeleton>();

    public static readonly StyledProperty<HorizontalAlignment> HorizontalContentAlignmentProperty =
        ContentControl.HorizontalContentAlignmentProperty.AddOwner<AbstractCompositeSkeleton>();

    public static readonly StyledProperty<VerticalAlignment> VerticalContentAlignmentProperty =
        ContentControl.VerticalContentAlignmentProperty.AddOwner<AbstractCompositeSkeleton>();

    public bool IsLoading
    {
        get => GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    public bool IsShowAvatar
    {
        get => GetValue(IsShowAvatarProperty);
        set => SetValue(IsShowAvatarProperty, value);
    }

    public bool IsShowParagraph
    {
        get => GetValue(IsShowParagraphProperty);
        set => SetValue(IsShowParagraphProperty, value);
    }

    public bool IsShowTitle
    {
        get => GetValue(IsShowTitleProperty);
        set => SetValue(IsShowTitleProperty, value);
    }

    public bool IsRound
    {
        get => GetValue(IsRoundProperty);
        set => SetValue(IsRoundProperty, value);
    }

    public Dimension TitleWidth
    {
        get => GetValue(TitleWidthProperty);
        set => SetValue(TitleWidthProperty, value);
    }

    public int ParagraphRows
    {
        get => GetValue(ParagraphRowsProperty);
        set => SetValue(ParagraphRowsProperty, value);
    }

    public Dimension ParagraphLastLineWidth
    {
        get => GetValue(ParagraphLastLineWidthProperty);
        set => SetValue(ParagraphLastLineWidthProperty, value);
    }

    public List<Dimension>? ParagraphLineWidths
    {
        get => GetValue(ParagraphLineWidthsProperty);
        set => SetValue(ParagraphLineWidthsProperty, value);
    }

    public AvatarShape AvatarShape
    {
        get => GetValue(AvatarShapeProperty);
        set => SetValue(AvatarShapeProperty, value);
    }

    public double AvatarSize
    {
        get => GetValue(AvatarSizeProperty);
        set => SetValue(AvatarSizeProperty, value);
    }

    [Content]
    [DependsOn(nameof(ContentTemplate))]
    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    public IDataTemplate? ContentTemplate
    {
        get => GetValue(ContentTemplateProperty);
        set => SetValue(ContentTemplateProperty, value);
    }

    public HorizontalAlignment HorizontalContentAlignment
    {
        get => GetValue(HorizontalContentAlignmentProperty);
        set => SetValue(HorizontalContentAlignmentProperty, value);
    }

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

    internal bool IsContentVisible
    {
        get => _isContentVisible;
        set => SetAndRaise(IsContentVisibleProperty, ref _isContentVisible, value);
    }

    /// <summary>Template of the placeholder shown while loading; built on demand with this skeleton as templated
    /// parent, so template bindings, <c>/template/</c> styles and template part names work inside it.</summary>
    internal static readonly StyledProperty<IControlTemplate?> PlaceholderTemplateProperty =
        AvaloniaProperty.Register<AbstractCompositeSkeleton, IControlTemplate?>(nameof(PlaceholderTemplate));

    internal IControlTemplate? PlaceholderTemplate
    {
        get => GetValue(PlaceholderTemplateProperty);
        set => SetValue(PlaceholderTemplateProperty, value);
    }

    #endregion

    private Panel? _placeholderHost;
    private Control? _placeholder;
    private INameScope? _placeholderNameScope;

    /// <summary>The placeholder built from <see cref="PlaceholderTemplate"/>; null while not loading.</summary>
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

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        StopShimmer();
        RemovePlaceholder();
        _placeholderHost = e.NameScope.Find<Panel>(PlaceholderHostPart);
        BuildPlaceholder();
        // The base resolves the shimmer layer, which already sees the new placeholder.
        base.OnApplyTemplate(e);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        UpdatePlaceholder(false);
    }

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
