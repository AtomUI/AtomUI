using AtomUI.Media;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using Avalonia.Media;
using Avalonia.VisualTree;
using Avalonia.Threading;

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
    private WeakReference<Control>? _pendingTemplateFocus;
    private int _templateGeneration;

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
        if (change.Property == TemplateProperty)
        {
            var focused = TopLevel.GetTopLevel(this)?.FocusManager?.GetFocusedElement() as Control;
            if (focused is not null)
            {
                _pendingTemplateFocus = this.IsVisualAncestorOf(focused)
                    ? new WeakReference<Control>(focused)
                    : null;
            }
            ++_templateGeneration;
        }
        base.OnPropertyChanged(change);
        if (this.IsAttachedToVisualTree())
        {
            if (change.Property == PresetImageProperty ||
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

    private void SetupImage()
    {
        CheckImageSource();
        if (_svg is null)
        {
            return;
        }

        string? imageSource = null;
        var imagePath = ImagePath;
        if (PresetImage is not null)
        {
            if (BorderColor is null || BorderColorSecondary is null || ShadowColor is null || ContentColor is null || BgColor is null)
            {
                return;
            }

            var colorBgContainer = ((ISolidColorBrush)BgColor).Color;
            var borderColor      = ColorUtils.OnBackground(((ISolidColorBrush)BorderColor).Color, colorBgContainer);
            var borderColorSecondary =
                ColorUtils.OnBackground(((ISolidColorBrush)BorderColorSecondary).Color, colorBgContainer);
            var shadowColor  = ColorUtils.OnBackground(((ISolidColorBrush)ShadowColor).Color, colorBgContainer);
            var contentColor = ColorUtils.OnBackground(((ISolidColorBrush)ContentColor).Color, colorBgContainer);
            imageSource = PresetImage.Value == PresetEmptyImage.Default
                ? BuiltInImageBuilder.BuildDefaultImage(shadowColor, borderColor, borderColorSecondary)
                : BuiltInImageBuilder.BuildSimpleImage(contentColor, borderColor, shadowColor);
        }
        else if (ImageSource is not null)
        {
            imageSource = ImageSource;
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

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        ++_templateGeneration;
        _pendingTemplateFocus = null;
        base.OnDetachedFromVisualTree(e);
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
        CheckImageSource();
        SetupImage();
        if (_pendingTemplateFocus is { } focus)
        {
            _pendingTemplateFocus = null;
            var generation = _templateGeneration;
            var weakOwner = new WeakReference<AbstractEmpty>(this);
            // A native Footer is reparented by the new presenter. Restore only
            // after it attaches, without retaining it or replacing later focus.
            Dispatcher.UIThread.Post(() =>
            {
                if (weakOwner.TryGetTarget(out var owner) && owner._templateGeneration == generation &&
                    focus.TryGetTarget(out var control) && owner.IsVisualAncestorOf(control) &&
                    TopLevel.GetTopLevel(owner) is { } topLevel &&
                    ReferenceEquals(TopLevel.GetTopLevel(control), topLevel) &&
                    topLevel.FocusManager?.GetFocusedElement() is null)
                {
                    control.Focus();
                }
            }, DispatcherPriority.Loaded);
        }
    }
}
