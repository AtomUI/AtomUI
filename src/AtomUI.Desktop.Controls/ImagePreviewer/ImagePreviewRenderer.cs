using AtomUI.Controls.Utils;
using AtomUI.Data;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

internal class ImagePreviewRenderer : Control
{
    #region 公共属性定义

    public static readonly StyledProperty<IImage?> SourceProperty =
        AvaloniaProperty.Register<ImagePreviewRenderer, IImage?>(nameof(Source));

    public static readonly StyledProperty<Stretch> StretchProperty =
        AvaloniaProperty.Register<ImagePreviewRenderer, Stretch>(nameof(Stretch), Stretch.Uniform);

    public IImage? Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    public Stretch Stretch
    {
        get => GetValue(StretchProperty);
        set => SetValue(StretchProperty, value);
    }

    /// <summary>
    /// 封面图片圆角。上游 image 元素自带 border-radius（示例中为 4px），AtomUI 的 image part
    /// 是纯渲染 Control，由本属性在图片上施加圆角裁剪几何。
    /// </summary>
    public CornerRadius CornerRadius
    {
        get => GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public Size SourceSize => Source?.Size ?? default;

    #endregion

    #region 内部属性定义

    // 本属性是 Border.CornerRadiusProperty 的 AddOwner，同一属性实例。XAML Semantic Style 必须以
    // 限定名 `Property="Border.CornerRadius"` 引用（经 public 声明类型 Border 的字段解析）；直接写
    // `CornerRadius` 会经 x:SetterTargetType 解析到本 internal 类型的字段，XAML 编译器生成的 ldsfld
    // 不做编译期可见性检查，internal 类型（即使字段声明为 public）会在运行时抛 FieldAccessException。
    internal static readonly StyledProperty<CornerRadius> CornerRadiusProperty =
        Border.CornerRadiusProperty.AddOwner<ImagePreviewRenderer>();

    #endregion

    private IDisposable? _stretchBindingDisposable;

    private Image? _image;

    static ImagePreviewRenderer()
    {
        AffectsMeasure<ImagePreviewRenderer>(SourceProperty);
        SourceProperty.Changed.AddClassHandler<ImagePreviewRenderer>((control, args) => control.HandleSourceChanged(args));
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == BoundsProperty || change.Property == CornerRadiusProperty)
        {
            UpdateImageClip();
        }
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var size = base.ArrangeOverride(finalSize);
        // 子 Image 的 Bounds 在 Arrange 后才确定，裁剪几何跟随其实际边界。
        UpdateImageClip();
        return size;
    }

    private void HandleSourceChanged(AvaloniaPropertyChangedEventArgs args)
    {
        _stretchBindingDisposable?.Dispose();
        _stretchBindingDisposable = null;
        if (_image is not null)
        {
            LogicalChildren.Remove(_image);
            VisualChildren.Remove(_image);
            _image = null;
        }

        var source = args.GetNewValue<IImage?>();
        if (source is null)
        {
            return;
        }
        _image = new Image { Source = source };
        ((ISetLogicalParent)_image).SetParent(this);
        // Image is created from Source at runtime, outside a ControlTemplate; ControlTheme cannot target it.
        // This renderer owns the binding and disposes it before replacing the child above.
        _stretchBindingDisposable = BindUtils.RelayBind(this, StretchProperty, _image, Image.StretchProperty);
        VisualChildren.Add(_image);
        LogicalChildren.Add(_image);
        UpdateImageClip();
    }

    // 与 DashedBorder.ClipContentToCornerRadius 同构：裁剪直接落在子 Image 的 Clip 几何上。
    // 不能在 Render override 里 PushGeometryClip 包着 _image.Render 画——Image 是 VisualChild，
    // 渲染器在父 Render 之后还会独立遍历 VisualChildren 再绘制一次无裁剪的 Image，覆盖裁剪结果。
    private void UpdateImageClip()
    {
        if (_image is null)
        {
            return;
        }

        var radius = CornerRadius;
        var size = _image.Bounds.Size;
        if (radius.IsUniform && radius.TopLeft == 0)
        {
            _image.Clip = null;
            return;
        }

        if (size.Width <= 0 || size.Height <= 0)
        {
            return;
        }

        // 与 DashedBorder 圆角裁剪同一 WinUI 关键点算法，保持全库圆角观感一致。
        var keypoints = RoundRectGeometryBuilder.CalculateRoundedCornersRectangleWinUI(
            new Rect(size),
            default,
            radius,
            BackgroundSizing.InnerBorderEdge);
        var geometry = new StreamGeometry();
        using (var ctx = geometry.Open())
        {
            RoundRectGeometryBuilder.DrawRoundedCornersRectangle(ctx, ref keypoints);
        }

        _image.Clip = geometry;
    }
}
