using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Toolkits.GalleryBase.Controls;

internal sealed class SemanticPartPreviewLayoutPanel : Panel
{
    public static readonly StyledProperty<double> CompactBreakpointProperty =
        AvaloniaProperty.Register<SemanticPartPreviewLayoutPanel, double>(nameof(CompactBreakpoint), 820);

    public static readonly StyledProperty<double> SpacingProperty =
        AvaloniaProperty.Register<SemanticPartPreviewLayoutPanel, double>(nameof(Spacing), 24);

    public static readonly StyledProperty<double> CompactPaneMaxHeightProperty =
        AvaloniaProperty.Register<SemanticPartPreviewLayoutPanel, double>(nameof(CompactPaneMaxHeight), 400);

    public static readonly StyledProperty<double> PaneMaxHeightProperty =
        AvaloniaProperty.Register<SemanticPartPreviewLayoutPanel, double>(nameof(PaneMaxHeight), 400);

    /// <summary>
    /// 预览画布的期望最小高度。测量阶段参与期望高度(向宿主要空间),
    /// 排布阶段不设下限,宿主钳制高度不足时画布随之收缩。
    /// </summary>
    public static readonly StyledProperty<double> PreviewStageMinHeightProperty =
        AvaloniaProperty.Register<SemanticPartPreviewLayoutPanel, double>(nameof(PreviewStageMinHeight), 360);

    public double PreviewStageMinHeight
    {
        get => GetValue(PreviewStageMinHeightProperty);
        set => SetValue(PreviewStageMinHeightProperty, value);
    }

    public double CompactBreakpoint
    {
        get => GetValue(CompactBreakpointProperty);
        set => SetValue(CompactBreakpointProperty, value);
    }

    public double Spacing
    {
        get => GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    public double CompactPaneMaxHeight
    {
        get => GetValue(CompactPaneMaxHeightProperty);
        set => SetValue(CompactPaneMaxHeightProperty, value);
    }

    public double PaneMaxHeight
    {
        get => GetValue(PaneMaxHeightProperty);
        set => SetValue(PaneMaxHeightProperty, value);
    }

    static SemanticPartPreviewLayoutPanel()
    {
        AffectsMeasure<SemanticPartPreviewLayoutPanel>(
            CompactBreakpointProperty,
            SpacingProperty,
            CompactPaneMaxHeightProperty,
            PaneMaxHeightProperty,
            PreviewStageMinHeightProperty);
        AffectsArrange<SemanticPartPreviewLayoutPanel>(
            CompactBreakpointProperty,
            SpacingProperty,
            CompactPaneMaxHeightProperty,
            PaneMaxHeightProperty);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        if (Children.Count == 0)
        {
            return default;
        }

        // 画布下限写回 Stage 的 MinHeight:宿主钳制高度时下限随之收缩,
        // 保证测量期望、排布尺寸与可视区域一致,不产生可视区之外的布局;
        // 无界宿主使用完整配置值。
        if (Children.Count > 0)
        {
            Children[0].MinHeight = double.IsFinite(availableSize.Height)
                ? Math.Min(PreviewStageMinHeight, Math.Max(0, availableSize.Height))
                : PreviewStageMinHeight;
        }

        if (Children.Count == 1)
        {
            Children[0].Measure(availableSize);
            return Children[0].DesiredSize;
        }

        if (ShouldUseCompactLayout(availableSize.Width))
        {
            // The compact layout stacks the preview above the parts pane. A bounded
            // available height (the host clamps the semantic tab content to the page
            // viewport remainder) is split between the preview and the pane, so the
            // pane scrolls internally instead of pushing the preview out of the page
            // viewport. An unbounded host falls back to the fixed pane cap.
            Children[0].Measure(new Size(availableSize.Width, double.PositiveInfinity));
            var compactPaneMeasureHeight = double.IsFinite(availableSize.Height)
                ? Math.Max(0, availableSize.Height - Children[0].DesiredSize.Height - Spacing)
                : CompactPaneMaxHeight;
            Children[1].Measure(new Size(availableSize.Width, compactPaneMeasureHeight));
            var paneHeight = Math.Min(Children[1].DesiredSize.Height, compactPaneMeasureHeight);
            return new Size(
                Math.Max(Children[0].DesiredSize.Width, Children[1].DesiredSize.Width),
                Children[0].DesiredSize.Height + Spacing + paneHeight);
        }

        // The wide layout measures the pane with the full available height: when the
        // host bounds the semantic tab content to the page viewport remainder, the
        // pane fills that remainder and scrolls its card list internally. An
        // unbounded host falls back to the fixed PaneMaxHeight cap, so a pane whose
        // list grows without limit can never push the preview out of the page
        // viewport.
        var rightWidth = ResolveRightWidth(availableSize.Width);
        var leftWidth = Math.Max(0, availableSize.Width - Spacing - rightWidth);
        var paneMeasureHeight = double.IsFinite(availableSize.Height)
            ? availableSize.Height
            : PaneMaxHeight;
        Children[0].Measure(new Size(leftWidth, availableSize.Height));
        Children[1].Measure(new Size(rightWidth, paneMeasureHeight));
        return new Size(
            Children[0].DesiredSize.Width + Spacing + Children[1].DesiredSize.Width,
            Math.Max(Children[0].DesiredSize.Height, Children[1].DesiredSize.Height));
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        if (Children.Count == 0)
        {
            return finalSize;
        }

        if (Children.Count == 1)
        {
            Children[0].Arrange(new Rect(finalSize));
            return finalSize;
        }

        if (ShouldUseCompactLayout(finalSize.Width))
        {
            var firstHeight = Children[0].DesiredSize.Height;
            Children[0].Arrange(new Rect(0, 0, finalSize.Width, firstHeight));
            Children[1].Arrange(new Rect(
                0,
                firstHeight + Spacing,
                finalSize.Width,
                Math.Max(0, finalSize.Height - firstHeight - Spacing)));
            return finalSize;
        }

        var rightWidth = ResolveRightWidth(finalSize.Width);
        var leftWidth = Math.Max(0, finalSize.Width - Spacing - rightWidth);
        Children[0].Arrange(new Rect(0, 0, leftWidth, finalSize.Height));
        Children[1].Arrange(new Rect(leftWidth + Spacing, 0, rightWidth, finalSize.Height));
        return finalSize;
    }

    private bool ShouldUseCompactLayout(double width)
    {
        return double.IsFinite(width) && width < CompactBreakpoint;
    }

    private static double ResolveRightWidth(double width)
    {
        if (!double.IsFinite(width))
        {
            return 420;
        }
        return Math.Clamp(width * 0.38, 340, 460);
    }
}
