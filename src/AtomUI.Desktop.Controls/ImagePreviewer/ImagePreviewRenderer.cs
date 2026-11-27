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
    /// Corner radius of the cover image. The antd image element brings its own border-radius (4px in the
    /// examples); the AtomUI image part is a pure rendering control, so this property applies the rounded
    /// clip geometry to the image.
    /// </summary>
    public CornerRadius CornerRadius
    {
        get => GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public Size SourceSize => Source?.Size ?? default;

    #endregion

    #region 内部属性定义

    // This field is an AddOwner of Border.CornerRadiusProperty: the same property instance. A XAML
    // semantic style must reference it by its qualified name, `Property="Border.CornerRadius"`, which
    // resolves through the field of the public declaring type Border. A bare `CornerRadius` is resolved
    // by x:SetterTargetType to the field of this internal type. The ldsfld emitted by the XAML compiler
    // gets no compile-time visibility check, so an internal type (even with a public field) throws
    // FieldAccessException at runtime.
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
        // The bounds of the child Image are known only after Arrange; the clip geometry follows them.
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

    // Same construction as DashedBorder.ClipContentToCornerRadius: the clip is applied directly to the
    // Clip geometry of the child Image. Pushing a geometry clip around _image.Render inside the Render
    // override does not work: the Image is a VisualChild, and after the parent's Render the renderer
    // walks the VisualChildren again and draws the Image once more without the clip.
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

        // Same WinUI key-point algorithm as the rounded clip of DashedBorder, so rounded corners look the
        // same across the library.
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
