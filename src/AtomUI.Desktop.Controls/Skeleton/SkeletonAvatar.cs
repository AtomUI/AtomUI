using AtomUI.Controls;
using AtomUI.Data;
using Avalonia;
using Avalonia.Controls.Primitives;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// A round or square block that stands in for an avatar. It is also the avatar block of a <see cref="Skeleton" />.
/// </summary>
public partial class SkeletonAvatar : AbstractSkeleton, ICustomizableSizeTypeAware
{
    #region 公共属性定义
    /// <summary>
    /// Identifies the <see cref="Shape" /> property.
    /// </summary>
    public static readonly StyledProperty<AvatarShape> ShapeProperty =
        AvaloniaProperty.Register<SkeletonAvatar, AvatarShape>(nameof(Shape), AvatarShape.Circle);

    /// <summary>
    /// Identifies the <see cref="SizeType" /> property.
    /// </summary>
    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<SkeletonAvatar>();

    /// <summary>
    /// Identifies the <see cref="Size" /> property.
    /// </summary>
    public static readonly StyledProperty<double> SizeProperty =
        AvaloniaProperty.Register<SkeletonAvatar, double>(nameof(Size), Double.NaN);

    /// <summary>
    /// Outline of the avatar. <c>Circle</c>, the default, is fully round. <c>Square</c> has the corner radius of its
    /// size: <c>BorderRadiusLG</c>, <c>BorderRadius</c> or <c>BorderRadiusSM</c> by <see cref="SizeType" />, or
    /// <c>BorderRadius</c> while <see cref="Size" /> is set.
    /// </summary>
    public AvatarShape Shape
    {
        get => GetValue(ShapeProperty);
        set => SetValue(ShapeProperty, value);
    }

    /// <summary>
    /// Size step of the avatar: <c>Large</c>, <c>Middle</c> (the default) or <c>Small</c>. The theme takes its width
    /// and height from the matching control height while <see cref="Size" /> is <see langword="NaN" />.
    /// </summary>
    public CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    /// <summary>
    /// Width and height of the avatar in DIPs. The default, <see langword="NaN" />, leaves the size to
    /// <see cref="SizeType" />. Setting <see langword="NaN" /> after a numeric value does not release the width and
    /// height that were applied from it.
    /// </summary>
    public double Size
    {
        get => GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    #endregion

    #region 内部属性定义

    internal static readonly DirectProperty<SkeletonAvatar, bool> IsCustomSizeProperty =
        AvaloniaProperty.RegisterDirect<SkeletonAvatar, bool>(
            nameof(IsCustomSize),
            o => o.IsCustomSize,
            (o, v) => o.IsCustomSize = v);

    private bool _isCustomSize;

    internal bool IsCustomSize
    {
        get => _isCustomSize;
        set => SetAndRaise(IsCustomSizeProperty, ref _isCustomSize, value);
    }

    #endregion

    // Every configure pass replaces the held handles, so no property frames pile up.
    private IDisposable? _cornerRadiusValue;
    private IDisposable? _widthValue;
    private IDisposable? _heightValue;

    public SkeletonAvatar()
    {
    }

    /// <summary>
    /// Applies the width and height of <see cref="Size" /> when <see cref="SizeType" /> or <see cref="Size" /> changes,
    /// and the corner radius of <see cref="Shape" /> when <see cref="Shape" />, <see cref="SizeType" /> or
    /// <see cref="Size" /> changes. Overrides must call the base member.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == SizeTypeProperty || change.Property == SizeProperty)
        {
            IsCustomSize = !double.IsNaN(Size);
            ConfigureSize();
        }
        if (change.Property == ShapeProperty ||
            change.Property == SizeTypeProperty ||
            change.Property == SizeProperty)
        {
            ConfigureShape();
        }
    }

    private void ConfigureShape()
    {
        if (Shape == AvatarShape.Circle)
        {
            var radius = double.IsNaN(Width) ? Height : Width;
            this.SetHeldValue(CornerRadiusProperty, new CornerRadius(radius), ref _cornerRadiusValue);
        }
    }

    private void ConfigureSize()
    {
        if (!double.IsNaN(Size))
        {
            // A held value does not affect the setting made by the template.
            this.SetHeldValue(WidthProperty, Size, ref _widthValue);
            this.SetHeldValue(HeightProperty, Size, ref _heightValue);
        }
    }

    /// <summary>
    /// Applies the corner radius of <see cref="Shape" /> and the width and height of <see cref="Size" /> once the
    /// template is applied. Overrides must call the base member.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        ConfigureShape();
        ConfigureSize();
    }
}
