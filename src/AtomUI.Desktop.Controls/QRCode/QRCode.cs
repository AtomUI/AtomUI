using AtomUI.Desktop.Controls.Themes;
using AtomUI.Theme;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Metadata;
using Avalonia.VisualTree;
using SkiaSharp;
using SkiaSharp.QrCode;
using static System.Enum;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// 二维码纠错等级
/// </summary>
public enum QRCodeEccLevel
{
    L = 0,
    M = 1,
    Q = 2,
    H = 3
}

/// <summary>
/// QRCode状态
/// </summary>
public enum QRCodeStatus
{
    Active = 0,
    Expired,
    Loading,
    Scanned,
}

public class QRCode : TemplatedControl, IControlSharedTokenResourcesHost
{
    #region 公共属性定义

    public static readonly StyledProperty<string> ValueProperty =
        AvaloniaProperty.Register<QRCode, string>(nameof(Value));

    public static readonly StyledProperty<bool> IsBorderedProperty =
        AvaloniaProperty.Register<QRCode, bool>(nameof(IsBordered), true);

    public static readonly StyledProperty<IBrush?> ColorProperty =
        AvaloniaProperty.Register<QRCode, IBrush?>(nameof(Color));

    public static readonly StyledProperty<QRCodeEccLevel> EccLevelProperty =
        AvaloniaProperty.Register<QRCode, QRCodeEccLevel>(nameof(EccLevel), QRCodeEccLevel.M);

    public static readonly StyledProperty<int> SizeProperty =
        AvaloniaProperty.Register<QRCode, int>(nameof(Size), 160);

    public static readonly StyledProperty<int> IconSizeProperty =
        AvaloniaProperty.Register<QRCode, int>(nameof(IconSize), 40);

    public static readonly StyledProperty<IImage?> IconProperty =
        AvaloniaProperty.Register<QRCode, IImage?>(nameof(Icon));

    public static readonly StyledProperty<IBrush?> IconBgColorProperty =
        AvaloniaProperty.Register<QRCode, IBrush?>(nameof(IconBgColor));

    public static readonly StyledProperty<QRCodeStatus> StatusProperty =
        AvaloniaProperty.Register<QRCode, QRCodeStatus>(nameof(Status));

    public static readonly StyledProperty<object?> LoadingContentProperty =
        AvaloniaProperty.Register<QRCode, object?>(nameof(LoadingContent));

    public static readonly StyledProperty<IDataTemplate?> LoadingContentTemplateProperty =
        AvaloniaProperty.Register<QRCode, IDataTemplate?>(nameof(LoadingContentTemplate));

    public static readonly StyledProperty<object?> ExpiredContentProperty =
        AvaloniaProperty.Register<QRCode, object?>(nameof(ExpiredContent));

    public static readonly StyledProperty<IDataTemplate?> ExpiredContentTemplateProperty =
        AvaloniaProperty.Register<QRCode, IDataTemplate?>(nameof(ExpiredContentTemplate));

    public static readonly StyledProperty<object?> ScannedContentProperty =
        AvaloniaProperty.Register<QRCode, object?>(nameof(ScannedContent));

    public static readonly StyledProperty<IDataTemplate?> ScannedContentTemplateProperty =
        AvaloniaProperty.Register<QRCode, IDataTemplate?>(nameof(ScannedContentTemplate));

    public string Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public bool IsBordered
    {
        get => GetValue(IsBorderedProperty);
        set => SetValue(IsBorderedProperty, value);
    }

    public IBrush? Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    public int Size
    {
        get => GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    public QRCodeEccLevel EccLevel
    {
        get => GetValue(EccLevelProperty);
        set => SetValue(EccLevelProperty, value);
    }

    public int IconSize
    {
        get => GetValue(IconSizeProperty);
        set => SetValue(IconSizeProperty, value);
    }

    public IImage? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public IBrush? IconBgColor
    {
        get => GetValue(IconBgColorProperty);
        set => SetValue(IconBgColorProperty, value);
    }


    public QRCodeStatus Status
    {
        get => GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    [DependsOn(nameof(LoadingContentTemplate))]
    public object? LoadingContent
    {
        get => GetValue(LoadingContentProperty);
        set => SetValue(LoadingContentProperty, value);
    }

    public IDataTemplate? LoadingContentTemplate
    {
        get => GetValue(LoadingContentTemplateProperty);
        set => SetValue(LoadingContentTemplateProperty, value);
    }

    [DependsOn(nameof(ExpiredContentTemplate))]
    public object? ExpiredContent
    {
        get => GetValue(ExpiredContentProperty);
        set => SetValue(ExpiredContentProperty, value);
    }

    public IDataTemplate? ExpiredContentTemplate
    {
        get => GetValue(ExpiredContentTemplateProperty);
        set => SetValue(ExpiredContentTemplateProperty, value);
    }

    [DependsOn(nameof(ScannedContentTemplate))]
    public object? ScannedContent
    {
        get => GetValue(ScannedContentProperty);
        set => SetValue(ScannedContentProperty, value);
    }

    public IDataTemplate? ScannedContentTemplate
    {
        get => GetValue(ScannedContentTemplateProperty);
        set => SetValue(ScannedContentTemplateProperty, value);
    }

    #endregion

    #region 公共事件定义

    public event EventHandler? RefreshRequested;

    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<Bitmap> BitmapProperty =
        AvaloniaProperty.Register<QRCode, Bitmap>(nameof(Bitmap));
    
    internal Bitmap Bitmap
    {
        get => GetValue(BitmapProperty);
        set => SetValue(BitmapProperty, value);
    }

    #endregion

    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => OptionButtonToken.ID;

    private Button? _refreshButton;

    static QRCode()
    {
        AffectsMeasure<QRCode>(BitmapProperty);
    }

    public QRCode()
    {
        this.RegisterResources();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (_refreshButton != null)
        {
            _refreshButton.Click -= HandleRefreshButtonClicked;
        }
        
        _refreshButton = e.NameScope.Find<Button>(QRCodeThemeConstants.RefreshPart);
        if (_refreshButton != null)
        {
            _refreshButton.Click += HandleRefreshButtonClicked;
        }
        SetupQRCode();
    }

    private void HandleRefreshButtonClicked(object? sender, RoutedEventArgs args)
    {
        RefreshRequested?.Invoke(this, EventArgs.Empty);
    }

    private void SetupQRCode()
    {
        TryParse(EccLevel.ToString(), out ECCLevel eccLevel);
        using var generator = new QRCodeGenerator();
        var       qrcode    = generator.CreateQrCode(Value, eccLevel, quietZoneSize: 0);
        var       info      = new SKImageInfo(Size, Size);
        using var surface   = SKSurface.Create(info);
        var       canvas    = surface.Canvas;
        var       color     = ((ISolidColorBrush?)Color)?.Color ?? Colors.Black;
        var       bgColor   = ((ISolidColorBrush?)Background)?.Color ?? Colors.Transparent;

        canvas.Render(
            qrcode,
            info.Width,
            info.Height,
            SKColor.Empty,
            new SKColor(color.R, color.G, color.B, color.A),
            new SKColor(bgColor.R, bgColor.G, bgColor.B, bgColor.A)
        );

        using var image = surface.Snapshot();
        using var data  = image.Encode(SKEncodedImageFormat.Png, 100);
        Bitmap = new Bitmap(data.AsStream());
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (this.IsAttachedToVisualTree())
        {
            if (change.Property == ValueProperty || 
                change.Property == ColorProperty || 
                change.Property == BackgroundProperty || 
                change.Property == EccLevelProperty || 
                change.Property == SizeProperty)
            {
                SetupQRCode();
            }
        }
    }
}