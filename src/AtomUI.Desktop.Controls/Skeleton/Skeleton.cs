using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Metadata;

namespace AtomUI.Desktop.Controls;

public partial class Skeleton : AbstractSkeleton
{
    #region 公共属性定义

    public static readonly StyledProperty<bool> IsLoadingProperty =
        AvaloniaProperty.Register<Skeleton, bool>(nameof(IsLoading));

    public static readonly StyledProperty<bool> IsShowAvatarProperty =
        AvaloniaProperty.Register<Skeleton, bool>(nameof(IsShowAvatar));

    public static readonly StyledProperty<bool> IsShowParagraphProperty =
        AvaloniaProperty.Register<Skeleton, bool>(nameof(IsShowParagraph), true);

    public static readonly StyledProperty<bool> IsShowTitleProperty =
        AvaloniaProperty.Register<Skeleton, bool>(nameof(IsShowTitle), true);

    public static readonly StyledProperty<bool> IsRoundProperty =
        SkeletonLine.IsRoundProperty.AddOwner<Skeleton>();

    public static readonly StyledProperty<Dimension> TitleWidthProperty =
        AvaloniaProperty.Register<Skeleton, Dimension>(nameof(TitleWidth), new Dimension(50, DimensionUnitType.Percentage));

    public static readonly StyledProperty<int> ParagraphRowsProperty =
        AvaloniaProperty.Register<Skeleton, int>(nameof(ParagraphRows), 2, validate: i => i >= 1);

    public static readonly StyledProperty<Dimension> ParagraphLastLineWidthProperty =
        AvaloniaProperty.Register<Skeleton, Dimension>(nameof(ParagraphLastLineWidth), new Dimension(61.0, DimensionUnitType.Percentage));

    public static readonly StyledProperty<List<Dimension>?> ParagraphLineWidthsProperty =
        AvaloniaProperty.Register<Skeleton, List<Dimension>?>(nameof(ParagraphLineWidths));

    public static readonly StyledProperty<AvatarShape> AvatarShapeProperty =
        AvaloniaProperty.Register<Skeleton, AvatarShape>(nameof(AvatarShape), AvatarShape.Circle);

    public static readonly StyledProperty<CustomizableSizeType> AvatarSizeTypeProperty =
        AvaloniaProperty.Register<Skeleton, CustomizableSizeType>(nameof(AvatarSizeType), CustomizableSizeType.Middle);

    public static readonly StyledProperty<double> AvatarSizeProperty =
        AvaloniaProperty.Register<Skeleton, double>(nameof(AvatarSize), Double.NaN);

    public static readonly StyledProperty<object?> ContentProperty =
        ContentControl.ContentProperty.AddOwner<Skeleton>();

    public static readonly StyledProperty<IDataTemplate?> ContentTemplateProperty =
        ContentControl.ContentTemplateProperty.AddOwner<Skeleton>();

    public static readonly StyledProperty<HorizontalAlignment> HorizontalContentAlignmentProperty =
        ContentControl.HorizontalContentAlignmentProperty.AddOwner<Skeleton>();

    public static readonly StyledProperty<VerticalAlignment> VerticalContentAlignmentProperty =
        ContentControl.VerticalContentAlignmentProperty.AddOwner<Skeleton>();

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

    public CustomizableSizeType AvatarSizeType
    {
        get => GetValue(AvatarSizeTypeProperty);
        set => SetValue(AvatarSizeTypeProperty, value);
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

    internal static readonly DirectProperty<Skeleton, bool> IsContentVisibleProperty =
        AvaloniaProperty.RegisterDirect<Skeleton, bool>(
            nameof(IsContentVisible),
            o => o.IsContentVisible,
            (o, v) => o.IsContentVisible = v);

    private bool _isContentVisible;

    internal bool IsContentVisible
    {
        get => _isContentVisible;
        set => SetAndRaise(IsContentVisibleProperty, ref _isContentVisible, value);
    }

    #endregion

    static Skeleton()
    {
        ContentProperty.Changed.AddClassHandler<Skeleton>((x, e) => x.HandleContentChanged(e));
    }

    public Skeleton()
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
