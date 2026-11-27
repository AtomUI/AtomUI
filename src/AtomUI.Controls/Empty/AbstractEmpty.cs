using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace AtomUI.Controls.Commons;

/// <summary>
/// Base class of empty-state indicators, which show an illustration, a description and an optional footer.
/// The illustration comes from <see cref="Image" />, <see cref="ImagePath" />, <see cref="ImageSource" /> or
/// <see cref="PresetImage" />. Setting more than one of them to a non-null value throws an
/// <see cref="ApplicationException" /> when the illustration is next updated: when the template is applied,
/// when the control is attached to a visual tree, or when one of them changes while it is attached. Default
/// values do not count.
/// </summary>
/// <remarks>
/// A template shows <see cref="ImagePath" />, <see cref="ImageSource" /> and <see cref="PresetImage" /> through
/// an <c>Avalonia.Svg.Svg</c> element named <c>PART_SvgImage</c>, so a template without that element cannot show
/// them. <see cref="Image" /> is presented by the template itself.
/// </remarks>
[TemplatePart("PART_SvgImage", typeof(Avalonia.Svg.Svg))]
public abstract class AbstractEmpty : TemplatedControl, ISizeTypeAware
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="PresetImage" /> property.
    /// </summary>
    public static readonly StyledProperty<PresetEmptyImage?> PresetImageProperty =
        AvaloniaProperty.Register<AbstractEmpty, PresetEmptyImage?>(nameof(PresetImage));

    /// <summary>
    /// Identifies the <see cref="ImagePath" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> ImagePathProperty =
        AvaloniaProperty.Register<AbstractEmpty, string?>(nameof(ImagePath));

    /// <summary>
    /// Identifies the <see cref="ImageSource" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> ImageSourceProperty =
        AvaloniaProperty.Register<AbstractEmpty, string?>(nameof(ImageSource));

    /// <summary>
    /// Identifies the <see cref="Image" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> ImageProperty =
        AvaloniaProperty.Register<AbstractEmpty, object?>(nameof(Image));

    /// <summary>
    /// Identifies the <see cref="ImageTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> ImageTemplateProperty =
        AvaloniaProperty.Register<AbstractEmpty, IDataTemplate?>(nameof(ImageTemplate));

    /// <summary>
    /// Identifies the <see cref="Title" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> TitleProperty =
        AvaloniaProperty.Register<AbstractEmpty, object?>(nameof(Title));

    /// <summary>
    /// Identifies the <see cref="TitleTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> TitleTemplateProperty =
        AvaloniaProperty.Register<AbstractEmpty, IDataTemplate?>(nameof(TitleTemplate));

    /// <summary>
    /// Identifies the <see cref="Description" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> DescriptionProperty =
        AvaloniaProperty.Register<AbstractEmpty, object?>(nameof(Description));

    /// <summary>
    /// Identifies the <see cref="SizeType" /> property.
    /// </summary>
    public static readonly StyledProperty<SizeType> SizeTypeProperty =
        SizeTypeControlProperty.SizeTypeProperty.AddOwner<AbstractEmpty>();

    /// <summary>
    /// Identifies the <see cref="IsDescriptionVisible" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsDescriptionVisibleProperty =
        AvaloniaProperty.Register<AbstractEmpty, bool>(nameof(IsDescriptionVisible), true);

    /// <summary>
    /// Identifies the <see cref="Footer" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> FooterProperty =
        AvaloniaProperty.Register<AbstractEmpty, object?>(nameof(Footer));

    /// <summary>
    /// Identifies the <see cref="FooterTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> FooterTemplateProperty =
        AvaloniaProperty.Register<AbstractEmpty, IDataTemplate?>(nameof(FooterTemplate));

    /// <summary>
    /// Identifies the <see cref="StrokeDashArray" /> property.
    /// </summary>
    public static readonly StyledProperty<IReadOnlyList<double>?> StrokeDashArrayProperty =
        AvaloniaProperty.Register<AbstractEmpty, IReadOnlyList<double>?>(nameof(StrokeDashArray));

    /// <summary>
    /// The built-in illustration, drawn in the colors of the theme palette. It cannot be combined with
    /// <see cref="Image" />, <see cref="ImagePath" /> or <see cref="ImageSource" />.
    /// <see langword="null" />, the default, adds no illustration.
    /// </summary>
    /// <remarks>
    /// While one of its colors is unresolved, the illustration shown before is kept.
    /// </remarks>
    public PresetEmptyImage? PresetImage
    {
        get => GetValue(PresetImageProperty);
        set => SetValue(PresetImageProperty, value);
    }

    /// <summary>
    /// The location of an SVG file that is used as the illustration. It is assigned to <c>Path</c> of the template
    /// part <c>PART_SvgImage</c>. It cannot be combined with <see cref="Image" />, <see cref="ImageSource" /> or
    /// <see cref="PresetImage" />. <see langword="null" />, the default, adds no illustration.
    /// </summary>
    public string? ImagePath
    {
        get => GetValue(ImagePathProperty);
        set => SetValue(ImagePathProperty, value);
    }

    /// <summary>
    /// SVG markup that is used as the illustration. It is assigned to <c>Source</c> of the template part
    /// <c>PART_SvgImage</c>. It cannot be combined with <see cref="Image" />, <see cref="ImagePath" /> or
    /// <see cref="PresetImage" />. <see langword="null" />, the default, adds no illustration.
    /// </summary>
    public string? ImageSource
    {
        get => GetValue(ImageSourceProperty);
        set => SetValue(ImageSourceProperty, value);
    }

    /// <summary>
    /// Custom content that replaces the illustration. The theme template presents it with
    /// <see cref="ImageTemplate" />. It cannot be combined with <see cref="ImagePath" />,
    /// <see cref="ImageSource" /> or <see cref="PresetImage" />. When it is <see langword="null" />, the default,
    /// the illustration comes from those three.
    /// </summary>
    [DependsOn(nameof(ImageTemplate))]
    public object? Image
    {
        get => GetValue(ImageProperty);
        set => SetValue(ImageProperty, value);
    }

    /// <summary>
    /// The template used to present <see cref="Image" />. <see langword="null" />, the default, leaves the choice
    /// to the image presenter of the theme template.
    /// </summary>
    public IDataTemplate? ImageTemplate
    {
        get => GetValue(ImageTemplateProperty);
        set => SetValue(ImageTemplateProperty, value);
    }

    /// <summary>
    /// The title of the empty state, for a theme template that shows a title region. The template of the desktop
    /// <c>Empty</c> control has no title region and does not show it.
    /// </summary>
    [DependsOn(nameof(TitleTemplate))]
    public object? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>
    /// The template used to present <see cref="Title" />. <see langword="null" />, the default, leaves the choice
    /// to the title presenter of the theme template. The template of the desktop <c>Empty</c> control has no title
    /// presenter, so this property has no effect there.
    /// </summary>
    public IDataTemplate? TitleTemplate
    {
        get => GetValue(TitleTemplateProperty);
        set => SetValue(TitleTemplateProperty, value);
    }

    /// <summary>
    /// The content of the description region under the illustration. In the desktop <c>Empty</c> control a string
    /// is shown as text, and any other object is presented in the same region.
    /// </summary>
    /// <remarks>
    /// The desktop theme assigns the localized "No data" text when no value is set. A <see langword="null" />
    /// description is shown as an empty text line that keeps the height of the region; use
    /// <see cref="IsDescriptionVisible" /> to remove the region from the layout.
    /// </remarks>
    public object? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    /// <summary>
    /// The size preset. The desktop theme derives the height of the illustration and the margin around the
    /// description region from it. The default is <c>Middle</c>.
    /// </summary>
    public SizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    /// <summary>
    /// Whether the description region is laid out. <see langword="true" />, the default, shows the description;
    /// <see langword="false" /> removes the region from the layout.
    /// </summary>
    public bool IsDescriptionVisible
    {
        get => GetValue(IsDescriptionVisibleProperty);
        set => SetValue(IsDescriptionVisibleProperty, value);
    }

    /// <summary>
    /// The content of the optional action region under the description. The region is shown only while this is
    /// not <see langword="null" />, which is the default.
    /// </summary>
    [DependsOn(nameof(FooterTemplate))]
    public object? Footer
    {
        get => GetValue(FooterProperty);
        set => SetValue(FooterProperty, value);
    }

    /// <summary>
    /// The template used to present <see cref="Footer" />. <see langword="null" />, the default, leaves the choice
    /// to the footer presenter of the theme template.
    /// </summary>
    public IDataTemplate? FooterTemplate
    {
        get => GetValue(FooterTemplateProperty);
        set => SetValue(FooterTemplateProperty, value);
    }

    /// <summary>
    /// The dash pattern of the border around the content. <see langword="null" />, the default, and an empty list
    /// draw a solid border.
    /// </summary>
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

    /// <summary>
    /// Rebuilds the illustration while the control is attached to a visual tree: when <see cref="Image" />,
    /// <see cref="ImagePath" />, <see cref="ImageSource" /> or <see cref="PresetImage" /> changes, and when one of
    /// the brushes that color the preset illustration changes. Overrides must call the base member first.
    /// </summary>
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

    /// <summary>
    /// Resolves the colors that a preset illustration is drawn with. This class reads them from the brushes
    /// <c>BorderColor</c>, <c>BorderColorSecondary</c>, <c>ShadowColor</c>, <c>ContentColor</c> and
    /// <c>BgColor</c>, which must all be solid brushes. A derived class can override it to read them elsewhere.
    /// </summary>
    /// <param name="colors">
    /// The resolved colors when this method returns <see langword="true" />; <see langword="default" /> otherwise.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when all colors were resolved. When it returns <see langword="false" />, the
    /// illustration that is shown is left unchanged.
    /// </returns>
    private protected virtual bool TryResolvePresetColors(out PresetImageColors colors)
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

    /// <summary>
    /// Rebuilds the preset illustration, for a derived class to call when the colors it resolves have changed.
    /// It does nothing unless the control is attached to a visual tree, <see cref="PresetImage" /> is set and
    /// <see cref="Image" />, <see cref="ImagePath" /> and <see cref="ImageSource" /> are all
    /// <see langword="null" />.
    /// </summary>
    private protected void RefreshPresetImage()
    {
        if (this.IsAttachedToVisualTree() && Image is null && ImagePath is null && ImageSource is null && PresetImage.HasValue)
        {
            SetupImage();
        }
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

    /// <summary>
    /// Called when the control template is applied: it looks up the template part <c>PART_SvgImage</c> and
    /// rebuilds the illustration. Overrides must call the base member first.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _svg = e.NameScope.Find<Avalonia.Svg.Svg>("PART_SvgImage");
        SetupImage();
    }

    /// <summary>
    /// Rebuilds the illustration when the control is attached to a visual tree, because changes of its sources are
    /// not applied while the control is detached. Overrides must call the base member first.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        SetupImage();
    }
}
