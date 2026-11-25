using System.Diagnostics;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Thumb = AtomUI.Controls.Primitives.Thumb;

namespace AtomUI.Desktop.Controls;

internal class ColorSliderThumb : Thumb
{
    public static readonly StyledProperty<IBrush?> ColorValueBrushProperty =
        AvaloniaProperty.Register<ColorSliderThumb, IBrush?>(nameof(ColorValueBrush));

    public IBrush? ColorValueBrush
    {
        get => GetValue(ColorValueBrushProperty);
        set => SetValue(ColorValueBrushProperty, value);
    }

    #region 内部属性定义

    internal static readonly StyledProperty<CornerRadius> InnerCornerRadiusProperty =
        AvaloniaProperty.Register<ColorSliderThumb, CornerRadius>(nameof(InnerCornerRadius));

    internal CornerRadius InnerCornerRadius
    {
        get => GetValue(InnerCornerRadiusProperty);
        set => SetValue(InnerCornerRadiusProperty, value);
    }

    #endregion

    private Control? _innerEllipse;

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        Debug.Assert(MathUtils.AreClose(e.NewSize.Width, e.NewSize.Height));
        // Runtime measured radius cannot be expressed by a static ControlTheme setter.
        SetCurrentValue(CornerRadiusProperty, new CornerRadius(e.NewSize.Width / 2));
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (_innerEllipse != null)
        {
            _innerEllipse.SizeChanged -= HandleInnerEllipseSizeChanged;
        }
        _innerEllipse = e.NameScope.Find<Control>("PART_InnerEllipse");
        if (_innerEllipse != null)
        {
            _innerEllipse.SizeChanged += HandleInnerEllipseSizeChanged;
        }
    }

    private void HandleInnerEllipseSizeChanged(object? sender, SizeChangedEventArgs args)
    {
        InnerCornerRadius = new CornerRadius(args.NewSize.Width / 2);
    }
}
