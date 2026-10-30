using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using AvaloniaFlowDirection = Avalonia.Media.FlowDirection;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Draws the Panel item frame and applies the Filled variant's leading notch.
/// </summary>
internal sealed class StepsPanelItemFrame : Border
{
    public static readonly StyledProperty<StepsPanelVariant> PanelVariantProperty =
        AvaloniaProperty.Register<StepsPanelItemFrame, StepsPanelVariant>(nameof(PanelVariant), StepsPanelVariant.Filled);

    public static readonly StyledProperty<StepsType> TypeProperty =
        AvaloniaProperty.Register<StepsPanelItemFrame, StepsType>(nameof(Type), StepsType.Default);

    public static readonly StyledProperty<bool> IsFirstProperty =
        AvaloniaProperty.Register<StepsPanelItemFrame, bool>(nameof(IsFirst));

    public static readonly StyledProperty<double> ArrowWidthProperty =
        AvaloniaProperty.Register<StepsPanelItemFrame, double>(nameof(ArrowWidth));

    public StepsPanelVariant PanelVariant
    {
        get => GetValue(PanelVariantProperty);
        set => SetValue(PanelVariantProperty, value);
    }

    public StepsType Type
    {
        get => GetValue(TypeProperty);
        set => SetValue(TypeProperty, value);
    }

    public bool IsFirst
    {
        get => GetValue(IsFirstProperty);
        set => SetValue(IsFirstProperty, value);
    }

    public double ArrowWidth
    {
        get => GetValue(ArrowWidthProperty);
        set => SetValue(ArrowWidthProperty, value);
    }

    static StepsPanelItemFrame()
    {
        AffectsRender<StepsPanelItemFrame>(
            TypeProperty,
            PanelVariantProperty,
            IsFirstProperty,
            ArrowWidthProperty,
            BorderThicknessProperty,
            FlowDirectionProperty);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        UpdateFilledClip(finalSize);
        return base.ArrangeOverride(finalSize);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == TypeProperty ||
            change.Property == PanelVariantProperty ||
            change.Property == IsFirstProperty ||
            change.Property == ArrowWidthProperty ||
            change.Property == BorderThicknessProperty ||
            change.Property == FlowDirectionProperty)
        {
            UpdateFilledClip(Bounds.Size);
        }
    }

    private void UpdateFilledClip(Size size)
    {
        // The Filled leading notch mirrors antd's `.ant-steps-panel-filled
        // .ant-steps-item:not(:first-child)` clip-path and must stay exclusive to
        // the Panel type: every other type only paints this frame as a hover or
        // focus background, where an arrow-shaped clip would corrupt it.
        Clip = Type == StepsType.Panel &&
               PanelVariant == StepsPanelVariant.Filled &&
               !IsFirst
            ? CreateFilledClip(size)
            : null;
    }

    private Geometry? CreateFilledClip(Size size)
    {
        if (size.Width <= 0 || size.Height <= 0)
        {
            return null;
        }

        var edgeInset = Math.Max(BorderThickness.Top, BorderThickness.Bottom);
        var notchX = Math.Clamp(edgeInset + Math.Max(0, ArrowWidth), edgeInset, size.Width);
        var centerY = size.Height / 2;
        var geometry = new StreamGeometry();
        using (var context = geometry.Open())
        {
            if (FlowDirection == AvaloniaFlowDirection.RightToLeft)
            {
                context.BeginFigure(new Point(size.Width - edgeInset, 0), true);
                context.LineTo(new Point(0, 0));
                context.LineTo(new Point(0, size.Height));
                context.LineTo(new Point(size.Width - edgeInset, size.Height));
                context.LineTo(new Point(size.Width - notchX, centerY));
            }
            else
            {
                context.BeginFigure(new Point(edgeInset, 0), true);
                context.LineTo(new Point(size.Width, 0));
                context.LineTo(new Point(size.Width, size.Height));
                context.LineTo(new Point(edgeInset, size.Height));
                context.LineTo(new Point(notchX, centerY));
            }

            context.EndFigure(true);
        }

        return geometry;
    }
}
