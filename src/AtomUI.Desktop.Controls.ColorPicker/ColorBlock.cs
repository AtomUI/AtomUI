using AtomUI.Controls;
using AtomUI.Desktop.Controls.Primitives;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

internal class ColorBlock : TemplatedControl, ICustomizableSizeTypeAware
{
    private static readonly IPen EmptyColorPen = new ImmutablePen(Colors.Red.ToUInt32(), 2);

    #region 公共属性定义

    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<ColorBlock>();

    public static readonly StyledProperty<double> SizeProperty =
        AvaloniaProperty.Register<ColorBlock, double>(nameof(Size), Double.NaN);

    public static readonly StyledProperty<bool> IsEmptyColorModeProperty =
        AvaloniaProperty.Register<ColorBlock, bool>(nameof(IsEmptyColorMode));

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

    public bool IsEmptyColorMode
    {
        get => GetValue(IsEmptyColorModeProperty);
        set => SetValue(IsEmptyColorModeProperty, value);
    }

    #endregion

    #region 公共属性定义

    public static readonly RoutedEvent<RoutedEventArgs> ClearRequestEvent =
        RoutedEvent.Register<ColorBlock, RoutedEventArgs>(
            nameof(ClearRequest),
            RoutingStrategies.Bubble);

    public event EventHandler<RoutedEventArgs>? ClearRequest
    {
        add => AddHandler(ClearRequestEvent, value);
        remove => RemoveHandler(ClearRequestEvent, value);
    }

    #endregion

    #region 内部属性定义

    internal static readonly DirectProperty<ColorBlock, IBrush?> TransparentBgBrushProperty =
        AvaloniaProperty.RegisterDirect<ColorBlock, IBrush?>(
            nameof(TransparentBgBrush),
            o => o.TransparentBgBrush,
            (o, v) => o.TransparentBgBrush = v);

    internal static readonly StyledProperty<IBrush?> TransparentBgIntervalColorProperty =
        AvaloniaProperty.Register<ColorBlock, IBrush?>(nameof(TransparentBgIntervalColor));

    internal static readonly StyledProperty<double> TransparentBgSizeProperty =
        AvaloniaProperty.Register<ColorBlock, double>(nameof(TransparentBgSize), 4.0);

    internal static readonly DirectProperty<ColorBlock, bool> IsCustomSizeProperty =
        AvaloniaProperty.RegisterDirect<ColorBlock, bool>(
            nameof(IsCustomSize),
            o => o.IsCustomSize,
            (o, v) => o.IsCustomSize = v);

    private IBrush? _transparentBgBrush;

    internal IBrush? TransparentBgBrush
    {
        get => _transparentBgBrush;
        set => SetAndRaise(TransparentBgBrushProperty, ref _transparentBgBrush, value);
    }

    internal IBrush? TransparentBgIntervalColor
    {
        get => GetValue(TransparentBgIntervalColorProperty);
        set => SetValue(TransparentBgIntervalColorProperty, value);
    }

    internal double TransparentBgSize
    {
        get => GetValue(TransparentBgSizeProperty);
        set => SetValue(TransparentBgSizeProperty, value);
    }

    private bool _isCustomSize;
    private IDisposable? _widthValue;
    private IDisposable? _heightValue;

    internal bool IsCustomSize
    {
        get => _isCustomSize;
        set => SetAndRaise(IsCustomSizeProperty, ref _isCustomSize, value);
    }

    #endregion

    static ColorBlock()
    {
        AffectsRender<ColorBlock>(TransparentBgBrushProperty, IsEmptyColorModeProperty);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        ConfigureTransparentBgBrush();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (this.IsAttachedToVisualTree())
        {
            if (change.Property == TransparentBgIntervalColorProperty ||
                change.Property == TransparentBgSizeProperty ||
                change.Property == IsEmptyColorModeProperty)
            {
                ConfigureTransparentBgBrush();
            }
        }

        if (change.Property == SizeProperty)
        {
            ConfigureSize();
            IsCustomSize = !double.IsNaN(Size);
        }
    }

    private void ConfigureTransparentBgBrush()
    {
        if (IsEmptyColorMode)
        {
            TransparentBgBrush = Brushes.Transparent;
        }
        else
        {
            if (TransparentBgIntervalColor != null && TransparentBgIntervalColor is ISolidColorBrush solidColorBrush)
            {
                TransparentBgBrush = TransparentBgBrushUtils.Build(TransparentBgSize, solidColorBrush.Color);
            }
        }
    }

    private void ConfigureSize()
    {
        _widthValue?.Dispose();
        _widthValue = null;
        _heightValue?.Dispose();
        _heightValue = null;

        if (!double.IsNaN(Size))
        {
            // 不影响模板设置
            _widthValue  = SetValue(WidthProperty, Size, BindingPriority.Template);
            _heightValue = SetValue(HeightProperty, Size, BindingPriority.Template);
        }
    }

    public override void Render(DrawingContext context)
    {
        if (IsEmptyColorMode)
        {
            using var optionState = context.PushRenderOptions(new RenderOptions
            {
                EdgeMode = EdgeMode.Antialias
            });
            var startPoint = new Point(2, Bounds.Height - 2);
            var endPoint = new Point(Bounds.Width - 2, 2);
            context.DrawLine(EmptyColorPen, startPoint, endPoint);
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        RaiseEvent(new RoutedEventArgs(ClearRequestEvent));
    }
}
