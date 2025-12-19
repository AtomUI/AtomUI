using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Avalonia.Metadata;

namespace AtomUI.Desktop.Controls;

public class ImagePreviewer : AbstractImagePreviewer
{
    #region 公共属性定义
    public static readonly StyledProperty<object?> CoverIndicatorContentProperty =
        AvaloniaProperty.Register<ImagePreviewer, object?>(nameof(CoverIndicatorContent));
    
    public static readonly StyledProperty<IDataTemplate?> CoverIndicatorContentTemplateProperty =
        AvaloniaProperty.Register<ImagePreviewer, IDataTemplate?>(nameof(CoverIndicatorContentTemplate));
    
    public static readonly StyledProperty<string?> CoverImageSrcProperty =
        AvaloniaProperty.Register<ImagePreviewer, string?>(nameof(CoverImageSrc));
    
    public string? CoverImageSrc
    {
        get => GetValue(CoverImageSrcProperty);
        set => SetValue(CoverImageSrcProperty, value);
    }
    
    [DependsOn(nameof(CoverIndicatorContentTemplate))]
    public object? CoverIndicatorContent
    {
        get => GetValue(CoverIndicatorContentProperty);
        set => SetValue(CoverIndicatorContentProperty, value);
    }
    
    public IDataTemplate? CoverIndicatorContentTemplate
    {
        get => GetValue(CoverIndicatorContentTemplateProperty);
        set => SetValue(CoverIndicatorContentTemplateProperty, value);
    }
    #endregion
    
    #region 内部属性定义
    internal static readonly DirectProperty<ImagePreviewer, PreviewImageSource?> EffectiveCoverImageProperty =
        AvaloniaProperty.RegisterDirect<ImagePreviewer, PreviewImageSource?>(
            nameof(EffectiveCoverImage),
            o => o.EffectiveCoverImage,
            (o, v) => o.EffectiveCoverImage = v);
    
    private PreviewImageSource? _effectiveCoverImage;

    internal PreviewImageSource? EffectiveCoverImage
    {
        get => _effectiveCoverImage;
        set => SetAndRaise(EffectiveCoverImageProperty, ref _effectiveCoverImage, value);
    }
    #endregion
    
    static ImagePreviewer()
    {
        AffectsRender<ImagePreviewer>(CoverImageSrcProperty);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == CoverImageSrcProperty)
        {
            if (!string.IsNullOrEmpty(CoverImageSrc))
            {
                SetCurrentValue(EffectiveCoverImageProperty, LoadImageSource(CoverImageSrc));
            }
            else
            {
                SetCurrentValue(EffectiveCoverImageProperty, null);
            }
        }
        
        else if (change.Property == EffectiveSourcesProperty)
        {
            if (EffectiveSources?.Count > 0)
            {
                if (EffectiveCoverImage == null)
                {
                    SetCurrentValue(EffectiveCoverImageProperty, EffectiveSources.First());
                }
            }
        }
    }
    
    protected override void OnLoaded(RoutedEventArgs args)
    {
        base.OnLoaded(args);
        if (EffectiveCoverImage == null && EffectiveSources?.Count > 0)
        {
            SetCurrentValue(EffectiveCoverImageProperty, EffectiveSources.First());
        }
    }
}
