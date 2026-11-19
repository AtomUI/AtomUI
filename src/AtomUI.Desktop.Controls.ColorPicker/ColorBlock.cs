using System.Globalization;
using AtomUI.Controls;
using AtomUI.Desktop.Controls.Primitives;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

internal class ColorBlock : TemplatedControl, ICustomizableSizeTypeAware
{
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

    #region 公共事件定义

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

    internal static readonly StyledProperty<bool> UsePreviewPaintTokenResourcesProperty =
        AvaloniaProperty.Register<ColorBlock, bool>(nameof(UsePreviewPaintTokenResources));

    internal static readonly DirectProperty<ColorBlock, IBrush?> TransparentBgBrushProperty =
        AvaloniaProperty.RegisterDirect<ColorBlock, IBrush?>(
            nameof(TransparentBgBrush),
            o => o.TransparentBgBrush,
            (o, v) => o.TransparentBgBrush = v);

    internal static readonly DirectProperty<ColorBlock, bool> IsCustomSizeProperty =
        AvaloniaProperty.RegisterDirect<ColorBlock, bool>(
            nameof(IsCustomSize),
            o => o.IsCustomSize,
            (o, v) => o.IsCustomSize = v);

    internal bool UsePreviewPaintTokenResources
    {
        get => GetValue(UsePreviewPaintTokenResourcesProperty);
        set => SetValue(UsePreviewPaintTokenResourcesProperty, value);
    }

    private IBrush? _transparentBgBrush;

    internal IBrush? TransparentBgBrush
    {
        get => _transparentBgBrush;
        set => SetAndRaise(TransparentBgBrushProperty, ref _transparentBgBrush, value);
    }

    private bool _isCustomSize;

    internal bool IsCustomSize
    {
        get => _isCustomSize;
        set => SetAndRaise(IsCustomSizeProperty, ref _isCustomSize, value);
    }

    #endregion

    #region 运行时字段

    private readonly Pen _emptyColorPen = new();
    private OwnerTokenSet? _paintTokens;

    #endregion

    static ColorBlock()
    {
        AffectsRender<ColorBlock>(TransparentBgBrushProperty, IsEmptyColorModeProperty);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        UpdatePaint();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == UsePreviewPaintTokenResourcesProperty)
        {
            UpdatePreviewPaintTokenSubscription();
        }
        else if (change.Property == IsEmptyColorModeProperty ||
                 change.Property == ColorPickerThemeInputs.ThemeTransparentBgSizeProperty)
        {
            if (this.IsAttachedToVisualTree())
            {
                UpdatePaint();
            }
        }
        else if (change.Property == SizeProperty)
        {
            IsCustomSize = !double.IsNaN(Size);
        }
    }

    private void UpdatePreviewPaintTokenSubscription()
    {
        _paintTokens?.Dispose();
        _paintTokens = null;
        if (UsePreviewPaintTokenResources)
        {
            _paintTokens = new OwnerTokenSet(this, 3, UpdatePaint);
            _paintTokens.SetKind(0, SharedTokenKind.ColorError);
            _paintTokens.SetKind(1, SharedTokenKind.LineWidthBold);
            _paintTokens.SetKind(2, SharedTokenKind.ColorFillSecondary);
        }
        UpdatePaint();
    }

    private void UpdatePaint()
    {
        if (!IsEmptyColorMode)
        {
            if (_paintTokens?.GetValue(2) is ISolidColorBrush interval)
            {
                TransparentBgBrush = TransparentBgBrushUtils.Build(
                    ColorPickerThemeInputs.GetThemeTransparentBgSize(this), interval.Color);
            }
            return;
        }

        TransparentBgBrush = Brushes.Transparent;
        var rawBrush = _paintTokens?.GetValue(0);
        var brush = ReferenceEquals(rawBrush, BindingOperations.DoNothing) ? _emptyColorPen.Brush : rawBrush as IBrush;
        var width = ReadEmptyLineWidth(_paintTokens?.GetValue(1));
        if (!Equals(brush, _emptyColorPen.Brush) || !width.Equals(_emptyColorPen.Thickness))
        {
            _emptyColorPen.Brush = brush;
            _emptyColorPen.Thickness = width;
            InvalidateVisual();
        }
    }

    private double ReadEmptyLineWidth(object? value)
    {
        if (value is double width)
        {
            return width;
        }
        if (ReferenceEquals(value, BindingOperations.DoNothing))
        {
            return _emptyColorPen.Thickness;
        }
        try
        {
            return value is IConvertible convertible ? convertible.ToDouble(CultureInfo.CurrentCulture) : 0;
        }
        catch (Exception error) when (error is FormatException or InvalidCastException or OverflowException)
        {
            return 0;
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
            context.DrawLine(_emptyColorPen, startPoint, endPoint);
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        RaiseEvent(new RoutedEventArgs(ClearRequestEvent));
    }
}
