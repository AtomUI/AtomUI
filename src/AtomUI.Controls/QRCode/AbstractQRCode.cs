using AtomUI.Theme.Resources;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
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

/// <summary>
/// Base class for controls that render <see cref="Value" /> as a QR code image.
/// </summary>
[TemplatePart("PART_RefreshButton", typeof(Button))]
public abstract class AbstractQRCode : TemplatedControl
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="Value" /> property.
    /// </summary>
    public static readonly StyledProperty<string> ValueProperty =
        AvaloniaProperty.Register<AbstractQRCode, string>(nameof(Value));

    /// <summary>
    /// Identifies the <see cref="IsBordered" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsBorderedProperty =
        AvaloniaProperty.Register<AbstractQRCode, bool>(nameof(IsBordered), true);

    /// <summary>
    /// Identifies the <see cref="Color" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> ColorProperty =
        AvaloniaProperty.Register<AbstractQRCode, IBrush?>(nameof(Color));

    /// <summary>
    /// Identifies the <see cref="EccLevel" /> property.
    /// </summary>
    public static readonly StyledProperty<QRCodeEccLevel> EccLevelProperty =
        AvaloniaProperty.Register<AbstractQRCode, QRCodeEccLevel>(nameof(EccLevel), QRCodeEccLevel.M);

    /// <summary>
    /// Identifies the <see cref="Size" /> property.
    /// </summary>
    public static readonly StyledProperty<int> SizeProperty =
        AvaloniaProperty.Register<AbstractQRCode, int>(nameof(Size), 160);

    /// <summary>
    /// Identifies the <see cref="IconSize" /> property.
    /// </summary>
    public static readonly StyledProperty<int> IconSizeProperty =
        AvaloniaProperty.Register<AbstractQRCode, int>(nameof(IconSize), 40);

    /// <summary>
    /// Identifies the <see cref="Icon" /> property.
    /// </summary>
    public static readonly StyledProperty<IImage?> IconProperty =
        AvaloniaProperty.Register<AbstractQRCode, IImage?>(nameof(Icon));

    /// <summary>
    /// Identifies the <see cref="IconBgColor" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> IconBgColorProperty =
        AvaloniaProperty.Register<AbstractQRCode, IBrush?>(nameof(IconBgColor));

    /// <summary>
    /// Identifies the <see cref="Status" /> property.
    /// </summary>
    public static readonly StyledProperty<QRCodeStatus> StatusProperty =
        AvaloniaProperty.Register<AbstractQRCode, QRCodeStatus>(nameof(Status));

    /// <summary>
    /// Identifies the <see cref="LoadingContent" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> LoadingContentProperty =
        AvaloniaProperty.Register<AbstractQRCode, object?>(nameof(LoadingContent));

    /// <summary>
    /// Identifies the <see cref="LoadingContentTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> LoadingContentTemplateProperty =
        AvaloniaProperty.Register<AbstractQRCode, IDataTemplate?>(nameof(LoadingContentTemplate));

    /// <summary>
    /// Identifies the <see cref="ExpiredContent" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> ExpiredContentProperty =
        AvaloniaProperty.Register<AbstractQRCode, object?>(nameof(ExpiredContent));

    /// <summary>
    /// Identifies the <see cref="ExpiredContentTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> ExpiredContentTemplateProperty =
        AvaloniaProperty.Register<AbstractQRCode, IDataTemplate?>(nameof(ExpiredContentTemplate));

    /// <summary>
    /// Identifies the <see cref="ScannedContent" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> ScannedContentProperty =
        AvaloniaProperty.Register<AbstractQRCode, object?>(nameof(ScannedContent));

    /// <summary>
    /// Identifies the <see cref="ScannedContentTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> ScannedContentTemplateProperty =
        AvaloniaProperty.Register<AbstractQRCode, IDataTemplate?>(nameof(ScannedContentTemplate));

    /// <summary>
    /// The text encoded in the QR code. Changing it regenerates the code.
    /// When it is empty or <see langword="null" />, no code is generated and a code drawn for an earlier value stays
    /// visible.
    /// </summary>
    public string Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>
    /// Controls whether the theme frames the code and insets it with a padding. <see langword="true" /> draws a border
    /// in the split-line color and insets the code by a small padding; <see langword="false" /> draws the code without
    /// a visible border, filling the whole frame.
    /// </summary>
    public bool IsBordered
    {
        get => GetValue(IsBorderedProperty);
        set => SetValue(IsBorderedProperty, value);
    }

    /// <summary>
    /// The solid brush that paints the modules of the code. Changing it regenerates the code.
    /// The theme sets it to its QR code text color. When the value is <see langword="null" /> or not a solid brush,
    /// the code is painted in the text color of the current theme instead.
    /// </summary>
    public IBrush? Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    /// <summary>
    /// The edge length of the square code area in device-independent units. It sets the width and height of the
    /// control and the resolution of the generated image, which is scaled uniformly into the area left by the padding.
    /// </summary>
    public int Size
    {
        get => GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <summary>
    /// The error-correction level used to encode <see cref="Value" />. Higher levels keep the code readable when more
    /// of it is damaged, at the cost of more modules for the same text. Changing it regenerates the code.
    /// </summary>
    public QRCodeEccLevel EccLevel
    {
        get => GetValue(EccLevelProperty);
        set => SetValue(EccLevelProperty, value);
    }

    /// <summary>
    /// The side length of the centered icon area, in the same units as <see cref="Size" />. It sizes the frame that
    /// shows <see cref="Icon" /> and the square cut out of the code for it; changing it regenerates the code.
    /// A square is cut only when an icon is set and the value is greater than zero.
    /// </summary>
    public int IconSize
    {
        get => GetValue(IconSizeProperty);
        set => SetValue(IconSizeProperty, value);
    }

    /// <summary>
    /// The image shown in the center of the code, stretched to the square given by <see cref="IconSize" />.
    /// Setting it cuts a square of that size out of the code so that the image does not cover its modules, and
    /// regenerates the code. <see langword="null" /> shows no icon and leaves the code complete.
    /// </summary>
    public IImage? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>
    /// The brush painted behind <see cref="Icon" />, over the square cut out of the code.
    /// The theme sets it to transparent, so the cut-out square shows the control background.
    /// </summary>
    public IBrush? IconBgColor
    {
        get => GetValue(IconBgColorProperty);
        set => SetValue(IconBgColorProperty, value);
    }


    /// <summary>
    /// The status presented over the code. <see cref="QRCodeStatus.Active" /> shows the code alone.
    /// <see cref="QRCodeStatus.Loading" />, <see cref="QRCodeStatus.Expired" /> and
    /// <see cref="QRCodeStatus.Scanned" /> cover the code with a mask and show the content set for that status, or the
    /// theme default when none is set.
    /// </summary>
    public QRCodeStatus Status
    {
        get => GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    /// <summary>
    /// The content shown over the code while <see cref="Status" /> is <see cref="QRCodeStatus.Loading" />.
    /// When it is <see langword="null" />, the theme shows its default spinner instead.
    /// </summary>
    [DependsOn(nameof(LoadingContentTemplate))]
    public object? LoadingContent
    {
        get => GetValue(LoadingContentProperty);
        set => SetValue(LoadingContentProperty, value);
    }

    /// <summary>
    /// The template used to present <see cref="LoadingContent" />. It applies only when that content is set.
    /// </summary>
    public IDataTemplate? LoadingContentTemplate
    {
        get => GetValue(LoadingContentTemplateProperty);
        set => SetValue(LoadingContentTemplateProperty, value);
    }

    /// <summary>
    /// The content shown over the code while <see cref="Status" /> is <see cref="QRCodeStatus.Expired" />.
    /// When it is <see langword="null" />, the theme shows its default text and refresh button instead. Setting it
    /// replaces that button, which is the only trigger of <see cref="RefreshRequested" />.
    /// </summary>
    [DependsOn(nameof(ExpiredContentTemplate))]
    public object? ExpiredContent
    {
        get => GetValue(ExpiredContentProperty);
        set => SetValue(ExpiredContentProperty, value);
    }

    /// <summary>
    /// The template used to present <see cref="ExpiredContent" />. It applies only when that content is set.
    /// </summary>
    public IDataTemplate? ExpiredContentTemplate
    {
        get => GetValue(ExpiredContentTemplateProperty);
        set => SetValue(ExpiredContentTemplateProperty, value);
    }

    /// <summary>
    /// The content shown over the code while <see cref="Status" /> is <see cref="QRCodeStatus.Scanned" />.
    /// When it is <see langword="null" />, the theme shows its default text instead.
    /// </summary>
    [DependsOn(nameof(ScannedContentTemplate))]
    public object? ScannedContent
    {
        get => GetValue(ScannedContentProperty);
        set => SetValue(ScannedContentProperty, value);
    }

    /// <summary>
    /// The template used to present <see cref="ScannedContent" />. It applies only when that content is set.
    /// </summary>
    public IDataTemplate? ScannedContentTemplate
    {
        get => GetValue(ScannedContentTemplateProperty);
        set => SetValue(ScannedContentTemplateProperty, value);
    }

    #endregion

    #region 公共事件定义

    /// <summary>
    /// Raised when the refresh button is clicked. The theme shows that button only in its default presentation of
    /// <see cref="QRCodeStatus.Expired" />, so the event occurs only while <see cref="Status" /> has that value and
    /// <see cref="ExpiredContent" /> is <see langword="null" />. The control takes no action itself; the handler is
    /// expected to provide a fresh <see cref="Value" />.
    /// </summary>
    public event EventHandler? RefreshRequested;

    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<Bitmap?> BitmapProperty =
        AvaloniaProperty.Register<AbstractQRCode, Bitmap?>(nameof(Bitmap));
    
    /// <summary>
    /// The generated code, shown by the image of the theme template. The control writes it whenever it generates the
    /// code: a square bitmap of <see cref="Size" /> by <see cref="Size" /> pixels with a transparent background.
    /// It is <see langword="null" /> until the first code is generated, and the bitmap it replaces is disposed.
    /// </summary>
    internal Bitmap? Bitmap
    {
        get => GetValue(BitmapProperty);
        set => SetValue(BitmapProperty, value);
    }

    #endregion

    private static readonly ThemeTokenResolver TokenResolver = new();
    private IDisposable? _themeSubscription;
    private Button? _refreshButton;

    static AbstractQRCode()
    {
        AffectsMeasure<AbstractQRCode>(BitmapProperty);
    }
    
    /// <summary>
    /// Subscribes to theme changes after the base member is called, so that a theme change regenerates the code
    /// while <see cref="Color" /> is <see langword="null" /> and the fallback color comes from the theme.
    /// An override must call the base member to keep such a code in step with the theme.
    /// </summary>
    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        _themeSubscription?.Dispose();
        // The raster fallback is computed at runtime when an application explicitly clears Color; a ControlTheme
        // cannot replace that local null value. Rebuild only this fallback on a theme/context change.
        _themeSubscription = TokenResolver.Subscribe(this, _ =>
        {
            if (Color is null)
            {
                SetupQRCode();
            }
        });
    }

    /// <summary>
    /// Disposes the theme subscription made by <see cref="OnAttachedToLogicalTree" /> before the base member is
    /// called, so that a detached control no longer regenerates its code on theme changes.
    /// An override must call the base member, or the subscription made on attachment is not released.
    /// </summary>
    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        _themeSubscription?.Dispose();
        _themeSubscription = null;
        base.OnDetachedFromLogicalTree(e);
    }

    /// <summary>
    /// Generates the code after the base member is called when the control joins a visual tree while
    /// <see cref="Color" /> is <see langword="null" />. The fallback color is read from the themed tree, so such a
    /// code is not generated before this point. An override must call the base member for that case to work.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (Color is null)
        {
            SetupQRCode();
        }
    }

    /// <summary>
    /// Wires the template's <c>PART_RefreshButton</c> to <see cref="RefreshRequested" />, detaching the handler of
    /// the button found by an earlier template first, and then regenerates the code. An override must call the base
    /// member, or the refresh button stays inactive.
    /// </summary>
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
        if (string.IsNullOrEmpty(Value) || (Color is null && !this.IsAttachedToVisualTree()))
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
        var       color   = ((ISolidColorBrush?)Color)?.Color ??
                            ControlTokenAccessor.Capture(this).GetEffectiveGlobal<Color>(SharedTokenKind.ColorText);

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

    /// <summary>
    /// Regenerates the code after the base member is called whenever <see cref="Value" />, <see cref="Color" />,
    /// <see cref="EccLevel" />, <see cref="Size" />, <see cref="Icon" /> or <see cref="IconSize" /> changes.
    /// An override must call the base member, or the code no longer follows those properties.
    /// </summary>
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
