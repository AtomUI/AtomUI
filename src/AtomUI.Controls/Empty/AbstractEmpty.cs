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

    public static readonly StyledProperty<object?> ImageProperty =
        AvaloniaProperty.Register<AbstractEmpty, object?>(nameof(Image));

    public static readonly StyledProperty<IDataTemplate?> ImageTemplateProperty =
        AvaloniaProperty.Register<AbstractEmpty, IDataTemplate?>(nameof(ImageTemplate));

    public static readonly StyledProperty<object?> TitleProperty =
        AvaloniaProperty.Register<AbstractEmpty, object?>(nameof(Title));

    public static readonly StyledProperty<IDataTemplate?> TitleTemplateProperty =
        AvaloniaProperty.Register<AbstractEmpty, IDataTemplate?>(nameof(TitleTemplate));

    public static readonly StyledProperty<object?> DescriptionProperty =
        AvaloniaProperty.Register<AbstractEmpty, object?>(nameof(Description));

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

    [DependsOn(nameof(ImageTemplate))]
    public object? Image
    {
        get => GetValue(ImageProperty);
        set => SetValue(ImageProperty, value);
    }

    public IDataTemplate? ImageTemplate
    {
        get => GetValue(ImageTemplateProperty);
        set => SetValue(ImageTemplateProperty, value);
    }

    [DependsOn(nameof(TitleTemplate))]
    public object? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public IDataTemplate? TitleTemplate
    {
        get => GetValue(TitleTemplateProperty);
        set => SetValue(TitleTemplateProperty, value);
    }

    public object? Description
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
        AffectsMeasure<AbstractEmpty>(ImageProperty,
            ImageTemplateProperty,
            TitleProperty,
            TitleTemplateProperty,
            PresetImageProperty,
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


    // Counts only values the app or a style set. Defaults and metadata overrides (a subclass default
    // preset) are not explicit and must not conflict with an app-set Image.
    private bool IsExplicitlySet(AvaloniaProperty property)
    {
        return IsSet(property) && GetValue(property) is not null;
    }

    private void CheckImageSource()
    {
        var imageSetCount = 0;
        if (IsExplicitlySet(ImageProperty))
        {
            imageSetCount++;
        }

        if (IsExplicitlySet(PresetImageProperty))
        {
            imageSetCount++;
        }

        if (IsExplicitlySet(ImagePathProperty))
        {
            imageSetCount++;
        }

        if (IsExplicitlySet(ImageSourceProperty))
        {
            imageSetCount++;
        }

        if (imageSetCount > 1)
        {
            throw new ApplicationException(
                "Image, ImagePath, ImageSource and PresetImage cannot be set at the same time.");
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (this.IsAttachedToVisualTree())
        {
            if (change.Property == ImageProperty ||
                change.Property == PresetImageProperty ||
                change.Property == ImagePathProperty ||
                change.Property == ImageSourceProperty)
            {
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

    private bool TryResolvePresetColors(out PresetImageColors colors)
    {
        if (BorderColor is ISolidColorBrush border &&
            BorderColorSecondary is ISolidColorBrush borderSecondary &&
            ShadowColor is ISolidColorBrush shadow &&
            ContentColor is ISolidColorBrush content &&
            BgColor is ISolidColorBrush background)
        {
            colors = new PresetImageColors(border.Color, borderSecondary.Color, shadow.Color, content.Color, background.Color);
            return true;
        }

        colors = default;
        return false;
    }

    private void SetupImage()
    {
        CheckImageSource();
        // Templates may omit the svg part (the image is then rendered through Image).
        if (_svg is null)
        {
            return;
        }

        string? imagePath = null;
        string? imageSource = null;
        if (Image is not null)
        {
            // Rendered by the template's image presenter; the svg stays empty.
        }
        else if (ImagePath is not null)
        {
            imagePath = ImagePath;
        }
        else if (ImageSource is not null)
        {
            imageSource = ImageSource;
        }
        else if (PresetImage is { } preset)
        {
            if (!TryResolvePresetColors(out var colors))
            {
                return;
            }

            imageSource = BuiltInImageBuilder.GetPresetImage(preset, colors);
        }

        if (imagePath is not null)
        {
            if (_svg.Source is not null)
            {
                _svg.Source = null;
            }
            if (_svg.Path != imagePath)
            {
                _svg.Path = imagePath;
            }
        }
        else
        {
            if (_svg.Path is not null)
            {
                _svg.Path = null;
            }
            if (_svg.Source != imageSource)
            {
                _svg.Source = imageSource;
            }
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _svg = e.NameScope.Find<Avalonia.Svg.Svg>("PART_SvgImage");
        SetupImage();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        SetupImage();
    }
}
