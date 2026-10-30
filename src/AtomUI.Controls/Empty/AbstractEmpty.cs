using AtomUI.Media;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace AtomUI.Controls.Commons;

[TemplatePart("PART_SvgImage", typeof(Avalonia.Svg.Svg))]
public abstract class AbstractEmpty : TemplatedControl, ISizeTypeAware
{
    #region 公共属性定义

    public static readonly StyledProperty<PresetEmptyImage?> PresetImageProperty =
        AvaloniaProperty.Register<AbstractEmpty, PresetEmptyImage?>(nameof(PresetImage));

    public static readonly StyledProperty<string?> ImagePathProperty =
        AvaloniaProperty.Register<AbstractEmpty, string?>(nameof(ImagePath));

    public static readonly StyledProperty<string?> ImageSourceProperty =
        AvaloniaProperty.Register<AbstractEmpty, string?>(nameof(ImageSource));

    public static readonly StyledProperty<string?> DescriptionProperty =
        AvaloniaProperty.Register<AbstractEmpty, string?>(nameof(Description));

    public static readonly StyledProperty<SizeType> SizeTypeProperty =
        SizeTypeControlProperty.SizeTypeProperty.AddOwner<AbstractEmpty>();

    public static readonly StyledProperty<bool> IsDescriptionVisibleProperty =
        AvaloniaProperty.Register<AbstractEmpty, bool>(nameof(IsDescriptionVisible), true);

    public static readonly StyledProperty<object?> FooterProperty =
        AvaloniaProperty.Register<AbstractEmpty, object?>(nameof(Footer));

    public static readonly StyledProperty<IDataTemplate?> FooterTemplateProperty =
        AvaloniaProperty.Register<AbstractEmpty, IDataTemplate?>(nameof(FooterTemplate));

    public static readonly StyledProperty<IReadOnlyList<double>?> StrokeDashArrayProperty =
        AvaloniaProperty.Register<AbstractEmpty, IReadOnlyList<double>?>(nameof(StrokeDashArray));

    public PresetEmptyImage? PresetImage
    {
        get => GetValue(PresetImageProperty);
        set => SetValue(PresetImageProperty, value);
    }

    public string? ImagePath
    {
        get => GetValue(ImagePathProperty);
        set => SetValue(ImagePathProperty, value);
    }

    public string? ImageSource
    {
        get => GetValue(ImageSourceProperty);
        set => SetValue(ImageSourceProperty, value);
    }

    public string? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public SizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    public bool IsDescriptionVisible
    {
        get => GetValue(IsDescriptionVisibleProperty);
        set => SetValue(IsDescriptionVisibleProperty, value);
    }

    [DependsOn(nameof(FooterTemplate))]
    public object? Footer
    {
        get => GetValue(FooterProperty);
        set => SetValue(FooterProperty, value);
    }

    public IDataTemplate? FooterTemplate
    {
        get => GetValue(FooterTemplateProperty);
        set => SetValue(FooterTemplateProperty, value);
    }

    public IReadOnlyList<double>? StrokeDashArray
    {
        get => GetValue(StrokeDashArrayProperty);
        set => SetValue(StrokeDashArrayProperty, value);
    }

    #endregion

    #region 内部属性定义
    
    internal static readonly StyledProperty<IBrush?> BorderColorProperty =
        AvaloniaProperty.Register<AbstractEmpty, IBrush?>(nameof(BorderColor));
    
    internal static readonly StyledProperty<IBrush?> BorderColorSecondaryProperty =
        AvaloniaProperty.Register<AbstractEmpty, IBrush?>(nameof(BorderColorSecondary));
    
    internal static readonly StyledProperty<IBrush?> ShadowColorProperty =
        AvaloniaProperty.Register<AbstractEmpty, IBrush?>(nameof(ShadowColor));
    
    internal static readonly StyledProperty<IBrush?> ContentColorProperty =
        AvaloniaProperty.Register<AbstractEmpty, IBrush?>(nameof(ContentColor));
    
    internal static readonly StyledProperty<IBrush?> BgColorProperty =
        AvaloniaProperty.Register<AbstractEmpty, IBrush?>(nameof(BgColor));
    
    internal IBrush? BorderColor
    {
        get => GetValue(BorderColorProperty);
        set => SetValue(BorderColorProperty, value);
    }
    
    internal IBrush? BorderColorSecondary
    {
        get => GetValue(BorderColorSecondaryProperty);
        set => SetValue(BorderColorSecondaryProperty, value);
    }
    
    internal IBrush? ShadowColor
    {
        get => GetValue(ShadowColorProperty);
        set => SetValue(ShadowColorProperty, value);
    }

    internal IBrush? ContentColor
    {
        get => GetValue(ContentColorProperty);
        set => SetValue(ContentColorProperty, value);
    }
    
    internal IBrush? BgColor
    {
        get => GetValue(BgColorProperty);
        set => SetValue(BgColorProperty, value);
    }

    #endregion
    
    private Avalonia.Svg.Svg? _svg;

    static AbstractEmpty()
    {
        AffectsMeasure<AbstractEmpty>(PresetImageProperty,
            ImagePathProperty,
            ImageSourceProperty,
            DescriptionProperty,
            IsDescriptionVisibleProperty,
            FooterProperty,
            FooterTemplateProperty,
            BorderColorProperty,
            BorderColorSecondaryProperty,
            ShadowColorProperty,
            ContentColorProperty,
            BgColorProperty);
    }


    private void CheckImageSource()
    {
        var imageSetCount = 0;
        if (PresetImage is not null)
        {
            imageSetCount++;
        }

        if (ImagePath is not null)
        {
            imageSetCount++;
        }

        if (ImageSource is not null)
        {
            imageSetCount++;
        }

        if (imageSetCount > 1)
        {
            throw new ApplicationException(
                "ImagePath, ImageSource and PresetImage cannot be set at the same time.");
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (this.IsAttachedToVisualTree())
        {
            if (change.Property == PresetImageProperty ||
                change.Property == ImagePathProperty ||
                change.Property == ImageSourceProperty)
            {
                CheckImageSource();
                SetupImage();
            }
            else if (change.Property == BorderColorProperty ||
                change.Property == BorderColorSecondaryProperty ||
                change.Property == ShadowColorProperty ||
                change.Property == ContentColorProperty ||
                change.Property == BgColorProperty)
            {
                SetupImage();
            }
        }
    }

    private void SetupImage()
    {
        if (_svg is null)
        {
            return;
        }

        if (PresetImage is not null)
        {
            _svg.Path   = null;
            _svg.Source = null;
            if (BorderColor != null && ShadowColor != null && ContentColor != null && BgColor != null && BorderColorSecondary != null)
            {
                var colorBgContainer = ((ISolidColorBrush)BgColor!).Color;
                var borderColor      = ColorUtils.OnBackground(((ISolidColorBrush)BorderColor).Color, colorBgContainer);
                var borderColorSecondary =
                    ColorUtils.OnBackground(((ISolidColorBrush)BorderColorSecondary).Color, colorBgContainer);
                var shadowColor  = ColorUtils.OnBackground(((ISolidColorBrush)ShadowColor).Color, colorBgContainer);
                var contentColor = ColorUtils.OnBackground(((ISolidColorBrush)ContentColor).Color, colorBgContainer);
                if (PresetImage.Value == PresetEmptyImage.Default)
                {
                    _svg.Source = BuiltInImageBuilder.BuildDefaultImage(shadowColor, borderColor, borderColorSecondary);
                }
                else
                {
                    _svg.Source = BuiltInImageBuilder.BuildSimpleImage(contentColor, borderColor, shadowColor);
                }
            }
        }
        else if (ImageSource is not null)
        {
            _svg.Path   = null;
            _svg.Source = ImageSource;
        }
        else if (ImagePath is not null)
        {
            _svg.Source = null;
            _svg.Path = ImagePath;
        }
        else
        {
            _svg.Source = null;
            _svg.Path   = null;
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _svg = e.NameScope.Find<Avalonia.Svg.Svg>("PART_SvgImage");
        CheckImageSource();
        SetupImage();
    }
}
