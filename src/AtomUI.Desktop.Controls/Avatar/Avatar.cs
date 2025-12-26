using System.Diagnostics;
using AtomUI.Controls;
using AtomUI.Desktop.Controls.Themes;
using AtomUI.Media;
using AtomUI.Theme;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Media.Transformation;
using Avalonia.Metadata;

namespace AtomUI.Desktop.Controls;

public enum AvatarShape
{
    Circle,
    Square
}

internal enum AvatarContentType
{
    Icon,
    BitmapImage,
    SvgImage,
    Text
}

public class Avatar : TemplatedControl, IControlSharedTokenResourcesHost, IMotionAwareControl
{
    #region 公共属性定义

    public static readonly StyledProperty<double> GapProperty =
        AvaloniaProperty.Register<Avatar, double>(nameof(Gap), defaultValue: 4.0);

    public static readonly StyledProperty<PathIcon?> IconProperty =
        AvaloniaProperty.Register<Avatar, PathIcon?>(nameof(Icon));
    
    public static readonly StyledProperty<IImage?> BitmapSrcProperty =
        AvaloniaProperty.Register<Avatar, IImage?>(nameof(BitmapSrc));
    
    public static readonly StyledProperty<string?> SrcProperty =
        AvaloniaProperty.Register<Avatar, string?>(nameof(Src));
    
    public static readonly StyledProperty<string?> TextProperty = 
        AvaloniaProperty.Register<TemplatedControl, string?>(nameof (Text));

    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        AvaloniaProperty.Register<Avatar, CustomizableSizeType>(nameof(SizeType),
            CustomizableSizeType.Middle);
    
    public static readonly StyledProperty<double> SizeProperty =
        AvaloniaProperty.Register<Avatar, double>(nameof(Size), Double.NaN);
    
    public static readonly StyledProperty<AvatarShape> ShapeProperty =
        AvaloniaProperty.Register<Avatar, AvatarShape>(nameof(Shape), AvatarShape.Circle);

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<Avatar>();

    public double Gap
    {
        get => GetValue(GapProperty);
        set => SetValue(GapProperty, value);
    }

    public PathIcon? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
    
    public IImage? BitmapSrc
    {
        get => GetValue(BitmapSrcProperty);
        set => SetValue(BitmapSrcProperty, value);
    }
    
    public string? Src
    {
        get => GetValue(SrcProperty);
        set => SetValue(SrcProperty, value);
    }
    
    [Content]
    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
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
    
    public AvatarShape Shape
    {
        get => GetValue(ShapeProperty);
        set => SetValue(ShapeProperty, value);
    }

    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    #endregion

    #region 内部属性定义
    internal static readonly StyledProperty<double> EffectiveIconSizeProperty =
        AvaloniaProperty.Register<Avatar, double>(nameof(EffectiveIconSize));
    
    internal static readonly StyledProperty<ITransform?> TextRenderTransformProperty = 
        AvaloniaProperty.Register<Avatar, ITransform?>(nameof(TextRenderTransform));
    
    internal static readonly DirectProperty<Avatar, AvatarContentType> ContentTypeProperty =
        AvaloniaProperty.RegisterDirect<Avatar, AvatarContentType>(
            nameof(ContentType),
            o => o.ContentType,
            (o, v) => o.ContentType = v);

    internal double EffectiveIconSize
    {
        get => GetValue(EffectiveIconSizeProperty);
        set => SetValue(EffectiveIconSizeProperty, value);
    }
    
    public ITransform? TextRenderTransform
    {
        get => GetValue(TextRenderTransformProperty);
        set => SetValue(TextRenderTransformProperty, value);
    }
    
    private AvatarContentType _contentType = AvatarContentType.Icon;

    internal AvatarContentType ContentType
    {
        get => _contentType;
        set => SetAndRaise(ContentTypeProperty, ref _contentType, value);
    }
    
    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => ButtonToken.ID;

    #endregion

    private CustomizableSizeType? _originSizeType;
    private TextBlock? _textPresenter;
    
    static Avatar()
    {
        AffectsMeasure<Avatar>(SizeTypeProperty, TextProperty);
        AffectsRender<Avatar>(ShapeProperty, IconProperty, SrcProperty, GapProperty);
    }
    
    public Avatar()
    {
        this.RegisterResources();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == SizeProperty)
        {
            if (!double.IsNaN(Size))
            {
                _originSizeType = SizeType;
                SizeType        = CustomizableSizeType.Custom;
            }
            else
            {
                if (_originSizeType.HasValue)
                {
                    SizeType = _originSizeType.Value;
                }
            }
        }
        else if (change.Property == SizeTypeProperty)
        {
            ConfigureIconSize();
        }
        else if (change.Property == SrcProperty ||
                 change.Property == IconProperty ||
                 change.Property == TextProperty)
        {
            ConfigureContentType();
        }
        if (change.Property == ContentTypeProperty ||
            change.Property == GapProperty)
        {
            ConfigureTextRenderTransform();
        }
        else if (change.Property == ShapeProperty)
        {
            ConfigureShape();
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (_textPresenter != null)
        {
            _textPresenter.SizeChanged -= HandleTextPresenterSizeChanged;
        }
        _textPresenter = e.NameScope.Find<TextBlock>(AvatarThemeConstants.TextPresenterPart);
        if (_textPresenter != null)
        {
            _textPresenter.SizeChanged += HandleTextPresenterSizeChanged;
        }
        ConfigureShape();
        ConfigureIconSize();
        ConfigureContentType();
    }

    private void HandleTextPresenterSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        ConfigureTextRenderTransform();
    }

    private void ConfigureIconSize()
    {
        if (SizeType == CustomizableSizeType.Custom)
        {
            Debug.Assert(!double.IsNaN(Size));
            // 不影响模板设置
            SetValue(WidthProperty, Size, BindingPriority.Template);
            SetValue(HeightProperty, Size, BindingPriority.Template);
            SetValue(EffectiveIconSizeProperty, 
                (Icon != null || Src != null) ? Size / 2 : 18, BindingPriority.Template);
        }
    }

    private void ConfigureContentType()
    {
        if (Src != null)
        {
            ContentType = AvatarContentType.SvgImage;
        }
        else if (BitmapSrc != null)
        {
            ContentType = AvatarContentType.BitmapImage;
        }
        else if (Text != null)
        {
            ContentType = AvatarContentType.Text;
        }
        else
        {
            ContentType = AvatarContentType.Icon;
        }
    }

    private void ConfigureTextRenderTransform()
    {
        if (ContentType != AvatarContentType.Text)
        {
            TextRenderTransform = null;
        }
        else
        {
            if (_textPresenter != null && Gap * 2 < Width)
            {
                double scale     = 1;
                double offsetX   = 0;
                var    textWidth = TextUtils.CalculateTextSize(Text ?? string.Empty, FontSize, FontFamily).Width;
                if (Gap * 2 < Width)
                {
                    scale = (Width - Gap * 2) / textWidth;
                    scale = Math.Min(scale, 1.0);
                    offsetX = (scale * (textWidth - Width)) / 2;
                }
                var builder = new TransformOperations.Builder(2);
                builder.AppendScale(scale, scale);
                if (scale < 1.0)
                {
                    builder.AppendTranslate(-offsetX, 0);
                }
                TextRenderTransform = builder.Build();
            }
            else
            {
                TextRenderTransform = null;
            }
        }
    }

    private void ConfigureShape()
    {
        if (Shape == AvatarShape.Circle)
        {
            SetValue(CornerRadiusProperty, new CornerRadius(Width / 2), BindingPriority.Template);
        }
    }
}