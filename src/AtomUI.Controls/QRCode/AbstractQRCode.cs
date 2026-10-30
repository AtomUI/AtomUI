using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Metadata;
using SkiaSharp;
using SkiaSharp.QrCode;

namespace AtomUI.Controls.Commons;

[TemplatePart("PART_RefreshButton", typeof(Button))]
public abstract class AbstractQRCode : TemplatedControl
{
    #region 公共属性定义

    public static readonly StyledProperty<string> ValueProperty =
        AvaloniaProperty.Register<AbstractQRCode, string>(nameof(Value));

    public static readonly StyledProperty<bool> IsBorderedProperty =
        AvaloniaProperty.Register<AbstractQRCode, bool>(nameof(IsBordered), true);

    public static readonly StyledProperty<IBrush?> ColorProperty =
        AvaloniaProperty.Register<AbstractQRCode, IBrush?>(nameof(Color));

    public static readonly StyledProperty<QRCodeEccLevel> EccLevelProperty =
        AvaloniaProperty.Register<AbstractQRCode, QRCodeEccLevel>(nameof(EccLevel), QRCodeEccLevel.M);

    public static readonly StyledProperty<int> SizeProperty =
        AvaloniaProperty.Register<AbstractQRCode, int>(nameof(Size), 160);

    public static readonly StyledProperty<int> IconSizeProperty =
        AvaloniaProperty.Register<AbstractQRCode, int>(nameof(IconSize), 40);

    public static readonly StyledProperty<IImage?> IconProperty =
        AvaloniaProperty.Register<AbstractQRCode, IImage?>(nameof(Icon));

    public static readonly StyledProperty<IBrush?> IconBgColorProperty =
        AvaloniaProperty.Register<AbstractQRCode, IBrush?>(nameof(IconBgColor));

    public static readonly StyledProperty<QRCodeStatus> StatusProperty =
        AvaloniaProperty.Register<AbstractQRCode, QRCodeStatus>(nameof(Status));

    public static readonly StyledProperty<object?> LoadingContentProperty =
        AvaloniaProperty.Register<AbstractQRCode, object?>(nameof(LoadingContent));

    public static readonly StyledProperty<IDataTemplate?> LoadingContentTemplateProperty =
        AvaloniaProperty.Register<AbstractQRCode, IDataTemplate?>(nameof(LoadingContentTemplate));

    public static readonly StyledProperty<object?> ExpiredContentProperty =
        AvaloniaProperty.Register<AbstractQRCode, object?>(nameof(ExpiredContent));

    public static readonly StyledProperty<IDataTemplate?> ExpiredContentTemplateProperty =
        AvaloniaProperty.Register<AbstractQRCode, IDataTemplate?>(nameof(ExpiredContentTemplate));

    public static readonly StyledProperty<object?> ScannedContentProperty =
        AvaloniaProperty.Register<AbstractQRCode, object?>(nameof(ScannedContent));

    public static readonly StyledProperty<IDataTemplate?> ScannedContentTemplateProperty =
        AvaloniaProperty.Register<AbstractQRCode, IDataTemplate?>(nameof(ScannedContentTemplate));

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

    internal static readonly StyledProperty<Bitmap?> BitmapProperty =
        AvaloniaProperty.Register<AbstractQRCode, Bitmap?>(nameof(Bitmap));
    
    internal Bitmap? Bitmap
    {
        get => GetValue(BitmapProperty);
        set => SetValue(BitmapProperty, value);
    }

    #endregion

    private Button? _refreshButton;

    static AbstractQRCode()
    {
        AffectsMeasure<AbstractQRCode>(BitmapProperty);
    }
    
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (_refreshButton != null)
        {
            _refreshButton.Click -= HandleRefreshButtonClicked;
        }
        
        _refreshButton = e.NameScope.Find<Button>("PART_RefreshButton");
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
        if (string.IsNullOrEmpty(Value))
        { 
            return;
        }
        var eccLevel = EccLevel switch
        {
            QRCodeEccLevel.L => ECCLevel.L,
            QRCodeEccLevel.M => ECCLevel.M,
            QRCodeEccLevel.Q => ECCLevel.Q,
            QRCodeEccLevel.H => ECCLevel.H,
            _                => ECCLevel.M
        };
        var       qrcode  = QRCodeGenerator.CreateQrCode(Value, eccLevel, quietZoneSize: 0);
        var       info    = new SKImageInfo(Size, Size);
        using var surface = SKSurface.Create(info);
        var       canvas  = surface.Canvas;
        var       color   = ((ISolidColorBrush?)Color)?.Color ?? Colors.Black;

        canvas.Render(
            qrcode,
            info.Width,
            info.Height,
            SKColor.Empty,
            new SKColor(color.R, color.G, color.B, color.A),
            SKColors.Transparent
        );

        if (Icon is not null && IconSize > 0)
        {
            var iconOffset = (Size - IconSize) / 2f;
            using var clearPaint = new SKPaint
            {
                BlendMode = SKBlendMode.Clear,
                IsAntialias = false
            };
            canvas.DrawRect(iconOffset, iconOffset, IconSize, IconSize, clearPaint);
        }

        using var image     = surface.Snapshot();
        using var data      = image.Encode(SKEncodedImageFormat.Png, 100);
        var       oldBitmap = Bitmap;
        Bitmap = new Bitmap(data.AsStream());
        oldBitmap?.Dispose();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ValueProperty || 
            change.Property == ColorProperty || 
            change.Property == EccLevelProperty || 
            change.Property == SizeProperty ||
            change.Property == IconProperty ||
            change.Property == IconSizeProperty)
        {
            SetupQRCode();
        }
    }
}
