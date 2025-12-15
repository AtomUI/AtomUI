using AtomUI.Controls.Utils;
using AtomUI.Data;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

using SvgControl = Avalonia.Svg.Svg;

namespace AtomUI.Desktop.Controls;

internal class ImagePreviewRenderer : Control
{
    #region 公告属性定义

    public static readonly StyledProperty<PreviewImageSource?> SourceProperty =
        AvaloniaProperty.Register<ImagePreviewRenderer, PreviewImageSource?>(nameof(Source));
    
    public static readonly StyledProperty<Stretch> StretchProperty =
        AvaloniaProperty.Register<ImagePreviewRenderer, Stretch>(nameof (Stretch), Stretch.Uniform);
    
    public PreviewImageSource? Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }
    
    public Stretch Stretch
    {
        get => GetValue(Image.StretchProperty);
        set => SetValue(Image.StretchProperty, value);
    }

    public Size SourceSize => GetSourceOriginSize();
    
    #endregion
    
    private IDisposable? _stretchBindingDisposable;
    private Control? _sourceControl;

    static ImagePreviewRenderer()
    {
        AffectsMeasure<ImagePreviewRenderer>(SourceProperty);
        SourceProperty.Changed.AddClassHandler<ImagePreviewRenderer>((x, e) => x.HandleSourceChanged(e));
    }
    
    private void HandleSourceChanged(AvaloniaPropertyChangedEventArgs e)
    {
        var oldSource = (PreviewImageSource?)e.OldValue;
        var newSource = (PreviewImageSource?)e.NewValue;

        if (oldSource != null)
        {
            _stretchBindingDisposable?.Dispose();
            LogicalChildren.Clear();
            VisualChildren.Clear();
            _sourceControl = null;
        }

        if (newSource != null)
        {
            _stretchBindingDisposable?.Dispose();
            if (newSource.IsSvg)
            {
                _sourceControl = new SvgControl(new Uri("https://atomui.net"))
                {
                    Source = newSource.SvgContent
                };
                _stretchBindingDisposable = BindUtils.RelayBind(this, StretchProperty, _sourceControl, Avalonia.Svg.Svg.StretchProperty);
            }
            else
            {
                _sourceControl = new Image
                {
                    Source = newSource.Bitmap
                };
                ((ISetLogicalParent)_sourceControl).SetParent(this);
                _stretchBindingDisposable = BindUtils.RelayBind(this, StretchProperty, _sourceControl, Image.StretchProperty);
            }
            VisualChildren.Add(_sourceControl);
            LogicalChildren.Add(_sourceControl);
        }
    }

    private Size GetSourceOriginSize()
    {
        Size size = default;
        if (_sourceControl is Image image)
        {
            size = image.Source?.Size ?? default;
        }
        else if (_sourceControl is SvgControl svgControl)
        {
            var picture = svgControl.GetSKPicture();
            if (picture != null)
            {
                size = new Size(picture.CullRect.Width, picture.CullRect.Height);
            }
        }
        return size;
    }
}