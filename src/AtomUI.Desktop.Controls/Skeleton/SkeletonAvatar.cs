using AtomUI.Controls;
using AtomUI.Data;
using Avalonia;
using Avalonia.Controls.Primitives;

namespace AtomUI.Desktop.Controls;

public partial class SkeletonAvatar : AbstractSkeleton, ICustomizableSizeTypeAware
{
    #region 公共属性定义
    public static readonly StyledProperty<AvatarShape> ShapeProperty =
        AvaloniaProperty.Register<SkeletonAvatar, AvatarShape>(nameof(Shape), AvatarShape.Circle);

    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<SkeletonAvatar>();

    public static readonly StyledProperty<double> SizeProperty =
        AvaloniaProperty.Register<SkeletonAvatar, double>(nameof(Size), Double.NaN);

    public AvatarShape Shape
    {
        get => GetValue(ShapeProperty);
        set => SetValue(ShapeProperty, value);
    }

    public CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

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

    // 每次配置都替换持有的句柄，避免累积属性帧
    private IDisposable? _cornerRadiusValue;
    private IDisposable? _widthValue;
    private IDisposable? _heightValue;

    public SkeletonAvatar()
    {
    }

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
            // 不影响模板设置
            this.SetHeldValue(WidthProperty, Size, ref _widthValue);
            this.SetHeldValue(HeightProperty, Size, ref _heightValue);
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        ConfigureShape();
        ConfigureSize();
    }
}
